using AutoMapper;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using MyCodeCamp.Data;
using MyCodeCamp.Data.Entities;
using Newtonsoft.Json;
using System.Text;

namespace MyCodeCamp
{
    public class Startup
    {
        private IHostingEnvironment _env;
        private IConfigurationRoot _config { get; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            _env = env;
            _config = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(_config);

            // Usually the db context and the repository that uses the db context have the same scope.
            // E.g. If the db context is transient then the repo is transient too. 
            services.AddDbContext<CampContext>(ServiceLifetime.Scoped);
            services.AddScoped<ICampRepository, CampRepository>();
            services.AddTransient<CampDbInitializer>();
            services.AddTransient<CampIdentityInitializer>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // In addition to registering AutoMapper as a service here, we must also define an 
            // AutoMapper profile in our project that shows how one type connects to another type.
            services.AddAutoMapper();

            services.AddIdentity<CampUser, IdentityRole>()
                    .AddEntityFrameworkStores<CampContext>();

            services.Configure<IdentityOptions>(config =>
            {
                // Tell Identity what to do under circumstances/events we specify.
                config.Cookies.ApplicationCookie.Events = new CookieAuthenticationEvents()
                {
                    //OnRedirectToLogin = (context) =>
                    //{
                    //    if (context.Request.Path.StartsWithSegments("/api") && context.Response.StatusCode == 200)
                    //    {
                    //        context.Response.StatusCode = 401;
                    //    }
                    //    return Task.CompletedTask;
                    //},
                    //OnRedirectToAccessDenied = (context) =>
                    //{
                    //    if (context.Request.Path.StartsWithSegments("/api") && context.Response.StatusCode == 200)
                    //    {
                    //        context.Response.StatusCode = 403;
                    //    }

                    //    return Task.CompletedTask;
                    //}
                };
            });

            // Allows Cors to be used throughout the project.
            services.AddCors(config =>
            {
                // Add specific policies.
                config.AddPolicy("ESPN", builder =>
                {
                    builder.AllowAnyHeader()
                           .AllowAnyMethod()
                           .WithOrigins("http://www.espn.com");
                });

                config.AddPolicy("AnyGET", builder =>
                {
                    builder.AllowAnyHeader()
                           .WithMethods("GET")
                           .AllowAnyOrigin();
                });
            });

            // Authorize certain users.
            services.AddAuthorization(config =>
            {
                config.AddPolicy("SuperUsers", p => p.RequireClaim("SuperUser", "True"));
            });

            // Add framework services.
            services.AddMvc(options =>
                {
                    //if (!_env.IsProduction())
                    //{
                    //    options.SslPort = 44300;
                    //}
                    // These global filters will be added to every controller in the project.
                    //options.Filters.Add(new RequireHttpsAttribute());
                })
                .AddJsonOptions(options =>
                                {
                                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app,
                                IHostingEnvironment env,
                                ILoggerFactory loggerFactory,
                                CampDbInitializer seeder,
                                CampIdentityInitializer identitySeeder)
        {
            loggerFactory.AddConsole(_config.GetSection("Logging"));
            loggerFactory.AddDebug();

            // Configuring Cors to be used globally.
            //app.UseCors(config =>
            //{
            //    config.AllowAnyHeader()
            //          .AllowAnyMethod()
            //          .WithOrigins("http://www.google.com");
            //});

            app.UseIdentity();

            app.UseJwtBearerAuthentication(new JwtBearerOptions()
            {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidIssuer = _config["Tokens:Issuer"],
                    ValidAudience = _config["Tokens:Audience"],
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Tokens:Key"])),
                    ValidateLifetime = true
                }
            });

            app.UseMvc(config =>
            {
                //config.MapRoute("MainAPIRoute", "api/{controller}/{action}");
            });

            // If there's no data in our Db then the injected db initializer will seed the db.
            seeder.Seed().Wait();
            identitySeeder.Seed().Wait();
        }
    }
}
