using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyCodeCamp.Data;
using MyCodeCamp.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyCodeCamp.Controllers
{
    [Route("api/[controller]")]
    public class CampsController : Controller
    {
        private ICampRepository _repo;
        private ILogger<CampsController> _logger;

        public CampsController(ICampRepository repo, ILogger<CampsController> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        [HttpGet("")]
        public IActionResult Get()
        {
            var camps = _repo.GetAllCamps();

            return Ok(camps);
        }

        [HttpGet("{id}", Name = "CampGet")]
        public IActionResult Get(int id, bool includeSpeakers = false)
        {
            try
            {
                Camp camp = null; 

                if (includeSpeakers)
                {
                    camp = _repo.GetCampWithSpeakers(id);
                }
                else
                {
                    camp = _repo.GetCamp(id);
                }

                if (camp == null)
                {
                    return NotFound($"Camp {id} was not found.");
                }

                return Ok(camp);
            }
            catch
            {

            }

            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Camp model)
        {
            try
            {
                _logger.LogInformation("Creating a new code camp");

                _repo.Add(model);

                if (await _repo.SaveAllAsync())
                {
                    var newUri = Url.Link("CampGet", new { id = model.Id });

                    return Created(newUri, model);
                }
                else
                {
                    _logger.LogWarning("Could not save camp to the database");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Threw exception while saving camp: {ex}");
            }

            return BadRequest();
        }
    }
}
