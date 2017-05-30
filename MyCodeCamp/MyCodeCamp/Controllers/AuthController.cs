using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using MyCodeCamp.Data;
using MyCodeCamp.Data.Entities;
using MyCodeCamp.Filters;
using MyCodeCamp.Models;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MyCodeCamp.Controllers
{
    public class AuthController : Controller
    {
        private CampContext _context;
        private SignInManager<CampUser> _signInManager;
        private ILogger<AuthController> _logger;
        private UserManager<CampUser> _userMgr;
        private IPasswordHasher<CampUser> _hasher;

        public AuthController(CampContext context,
                                SignInManager<CampUser> signInManager,
                                UserManager<CampUser> userMgr,
                                IPasswordHasher<CampUser> hasher,
                                ILogger<AuthController> logger)
        {
            _context = context;
            _signInManager = signInManager;
            _userMgr = userMgr;
            _hasher = hasher;
            _logger = logger;
        }

        [HttpPost("api/auth/login")]
        [ValidateModel]
        public async Task<IActionResult> Login([FromBody] CredentialModel model)
        {
            try
            {
                var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, false, false);

                if (result.Succeeded)
                {
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Threw exception when during login: {ex}");
            }

            return BadRequest("Failed to login");
        }

        [ValidateModel]
        [HttpPost("api/auth/token")]
        public async Task<IActionResult> CreateToken([FromBody] CredentialModel model)
        {
            try
            {
                var user = await _userMgr.FindByNameAsync(model.UserName);
                if (user != null)
                {
                    if (_hasher.VerifyHashedPassword(user, user.PasswordHash, model.Password) == PasswordVerificationResult.Success)
                    {
                        var claims = new[]
                        {
                            new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                        };

                        // In an actual production app, never put the key directly in the code. Place it
                        // in config settings for example.
                        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("UNIQUELONGKEYVALUETHATISSECURE"));

                        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                        var token = new JwtSecurityToken(
                                issuer: "http://mycodecamp.org",
                                audience: "http://mycodecamp.org",
                                claims: claims,
                                expires: DateTime.UtcNow.AddMinutes(15),
                                signingCredentials: creds
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception thrown while creating JWT: {ex}");
            }

            return BadRequest("Failed to generate token");
        }
    }
}
