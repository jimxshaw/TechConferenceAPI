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
using Microsoft.AspNetCore.Cors;
using MyCodeCamp.Filters;

namespace MyCodeCamp.Controllers
{
    [EnableCors("AnyGET")]
    [Route("api/[controller]")]
    [ValidateModel] // Utilizes filters in ValidateModelAttribute.cs. This applies to every action if put on class level.
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
        public async Task<IActionResult> Post([FromBody] CampModel model)
        {
            try
            {
                // This if block isn't necessary after adding [ValidateModel] at the controller level.
                //if (!ModelState.IsValid)
                //{
                //    return BadRequest(ModelState);
                //}

                _logger.LogInformation("Creating a new code camp");

                // Map CampModel to Camp in order to post to the database
                // because the db accepts a Camp.
                var camp = _mapper.Map<Camp>(model);

                _repo.Add(camp);

                if (await _repo.SaveAllAsync())
                {
                    var newUri = Url.Link("CampGet", new { moniker = camp.Moniker });

                    // After pushing to the db, the Camp may get db assign
                    // fields and when we return this we'd like it to be the CampModel, 
                    // which represents what's actually changed in the db hence why
                    // Camp is being mapped back to CampModel.
                    return Created(newUri, _mapper.Map<CampModel>(camp));
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

        [HttpPut("{moniker}")]
        public async Task<IActionResult> Put(string moniker, [FromBody] CampModel model)
        {
            try
            {
                // This if block isn't necessary after adding [ValidateModel] at the controller level.
                //if (!ModelState.IsValid)
                //{
                //    return BadRequest(ModelState);
                //}

                var oldCamp = _repo.GetCampByMonikerWithSpeakers(moniker);

                if (oldCamp == null)
                {
                    return NotFound($"Could not find a camp with moniker of {moniker}");
                }

                // Map model to the old camp without using automapper.
                //oldCamp.Name = model.Name ?? oldCamp.Name;
                //oldCamp.Description = model.Description ?? oldCamp.Description;
                //oldCamp.Location = model.Location ?? oldCamp.Location;
                //oldCamp.Length = model.Length > 0 ? model.Length : oldCamp.Length;
                //oldCamp.Moniker = model.Moniker ?? oldCamp.Moniker;
                //oldCamp.EventDate = model.EventDate != DateTime.MinValue ? model.EventDate : oldCamp.EventDate;

                // Map model to the old camp with automapper and then modify the old camp.
                _mapper.Map(model, oldCamp);

                if (await _repo.SaveAllAsync())
                {
                    return Ok(_mapper.Map<CampModel>(oldCamp));
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"Threw exception while updating camp: {ex}");
            }

            return BadRequest("Could not update camp");
        }

        [HttpDelete("{moniker}")]
        public async Task<IActionResult> Delete(string moniker)
        {
            try
            {
                var oldCamp = _repo.GetCampByMonikerWithSpeakers(moniker);

                if (oldCamp == null)
                {
                    return NotFound($"Could not find camp with moniker of {moniker}");
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
