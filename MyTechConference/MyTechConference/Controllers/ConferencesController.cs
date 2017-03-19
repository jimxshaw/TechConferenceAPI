using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyTechConference.Controllers
{
    [Route("api/[controller]")]
    public class ConferencesController : Controller
    {
        [HttpGet("")]
        public IActionResult Get()
        {

            return Ok(new { Name = "James", Title = "Engineer" }); 
        }


    }
}
