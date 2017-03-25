using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyCodeCamp.Data;
using MyCodeCamp.Data.Entities;
using MyCodeCamp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyCodeCamp.Controllers
{
    [Route("api/[controller]")]
    public class CampsController : BaseController
    {
        private ICampRepository _repo;
        private ILogger<CampsController> _logger;
        private IMapper _mapper;

        public CampsController(ICampRepository repo,
                                ILogger<CampsController> logger,
                                IMapper mapper)
        {
            _repo = repo;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpGet("")]
        public IActionResult Get()
        {
            var camps = _repo.GetAllCamps();

            return Ok(_mapper.Map<IEnumerable<CampModel>>(camps));
        }

        [HttpGet("{moniker}", Name = "CampGet")]
        public IActionResult Get(string moniker, bool includeSpeakers = false)
        {
            try
            {
                Camp camp = null;

                if (includeSpeakers)
                {
                    camp = _repo.GetCampByMonikerWithSpeakers(moniker);
                }
                else
                {
                    camp = _repo.GetCampByMoniker(moniker);
                }

                if (camp == null)
                {
                    return NotFound($"Camp {moniker} was not found.");
                }

                // We're mapping our camp but also map an URL helper that we'll use in our
                // AutoMapper profile resolver.
                return Ok(_mapper.Map<CampModel>(camp));
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

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Camp model)
        {
            try
            {
                var oldCamp = _repo.GetCamp(id);

                if (oldCamp == null)
                {
                    return NotFound($"Could not find a camp with Id of {id}");
                }

                // Map model to the old camp.
                oldCamp.Name = model.Name ?? oldCamp.Name;
                oldCamp.Description = model.Description ?? oldCamp.Description;
                oldCamp.Location = model.Location ?? oldCamp.Location;
                oldCamp.Length = model.Length > 0 ? model.Length : oldCamp.Length;
                oldCamp.Moniker = model.Moniker ?? oldCamp.Moniker;
                oldCamp.EventDate = model.EventDate != DateTime.MinValue ? model.EventDate : oldCamp.EventDate;

                if (await _repo.SaveAllAsync())
                {
                    return Ok(oldCamp);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"Threw exception while updating camp: {ex}");
            }

            return BadRequest("Could not update camp");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var oldCamp = _repo.GetCamp(id);

                if (oldCamp == null)
                {
                    return NotFound($"Could not find camp with Id of {id}");
                }

                _repo.Delete(oldCamp);

                if (await _repo.SaveAllAsync())
                {
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Threw exception while deleting camp: {ex}");
            }

            return BadRequest("Could not delete camp");
        }
    }
}
