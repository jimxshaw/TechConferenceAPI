using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCodeCamp.Controllers
{
    // Multiple controllers will utilize the url helper that represents the
    // particular url hit during controller action execution. So it's best 
    // to create an abstract class of which child controllers can inherit.
    // The url helper will be used in conjunction with the AutoMapper profile
    // and resolver to dynamically generate urls. 
    public abstract class BaseController : Controller
    {
        public const string URLHELPER = "URLHELPER";

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            // Whenever a child controller action executes, this URLHELPER will
            // be included in the http context. 
            context.HttpContext.Items[URLHELPER] = this.Url;
        }
    }
}
