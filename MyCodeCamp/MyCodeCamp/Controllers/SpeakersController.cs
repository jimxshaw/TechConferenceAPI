using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyCodeCamp.Data;
using MyCodeCamp.Data.Entities;
using MyCodeCamp.Filters;
using MyCodeCamp.Models;

namespace MyCodeCamp.Controllers
{
    [Route("api/camps/{moniker}/speakers")]
    [ValidateModel] // Utilizes filters in ValidateModelAttribute.cs. This applies to every action if put on class level.
    public class SpeakersController : BaseController
    {
        private IMapper _mapper;
        private ILogger<SpeakersController> _logger;
        private ICampRepository _repository;
        private UserManager<CampUser> _userManager;

        public SpeakersController(ICampRepository repository,
                                    ILogger<SpeakersController> logger,
                                    IMapper mapper,
                                    UserManager<CampUser> userManager)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Get(string moniker, bool includeTalks = false)
        {
            var speakers = includeTalks ? _repository.GetSpeakersByMonikerWithTalks(moniker) : _repository.GetSpeakersByMoniker(moniker);

            return Ok(_mapper.Map<IEnumerable<SpeakerModel>>(speakers));
        }

        [HttpGet("{id}", Name = "SpeakerGet")]
        public IActionResult Get(string moniker, int id, bool includeTalks = false)
        {
            // Every speaker has a collection of talks but that talks object will
            // only get returned depending on the value of the includeTalks boolean.
            var speaker = includeTalks ? _repository.GetSpeakerWithTalks(id) : _repository.GetSpeaker(id);

            if (speaker == null)
            {
                return NotFound();
            }

            if (speaker.Camp.Moniker.ToLower() != moniker.ToLower())
            {
                return BadRequest("Speaker not in specified Camp");
            }

            return Ok(_mapper.Map<SpeakerModel>(speaker));
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Post(string moniker, [FromBody] SpeakerModel model)
        {
            try
            {
                var camp = _repository.GetCampByMoniker(moniker);

                if (camp == null)
                {
                    return BadRequest("Could not find camp");
                }

                var speaker = _mapper.Map<Speaker>(model);
                speaker.Camp = camp;

                var campUser = await _userManager.FindByNameAsync(this.User.Identity.Name);

                if (campUser != null)
                {
                    speaker.User = campUser;

                    _repository.Add(speaker);

                    if (await _repository.SaveAllAsync())
                    {
                        var url = Url.Link("SpeakerGet", new { moniker = camp.Moniker, id = speaker.Id });

                        return Created(url, _mapper.Map<SpeakerModel>(speaker));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception thrown while adding new speaker: {ex}");
            }

            return BadRequest("Could not add new speaker");
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string moniker, int id, [FromBody] SpeakerModel model)
        {
            try
            {
                var speaker = _repository.GetSpeaker(id);

                if (speaker == null)
                {
                    return NotFound();
                }

                if (speaker.Camp.Moniker.ToLower() != moniker.ToLower())
                {
                    return BadRequest("Speaker and camp do not match");
                }

                if (speaker.User.UserName != this.User.Identity.Name)
                {
                    return Forbid();
                }

                // Copy the data in the model (source) to the speaker (destination) where appropriate. 
                _mapper.Map(model, speaker);

                if (await _repository.SaveAllAsync())
                {
                    return Ok(_mapper.Map<SpeakerModel>(speaker));
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception thrown while updating speaker: {ex}");
            }

            return BadRequest("Could not update speaker");
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string moniker, int id)
        {
            try
            {
                var speaker = _repository.GetSpeaker(id);

                if (speaker == null)
                {
                    return NotFound();
                }

                if (speaker.Camp.Moniker.ToLower() != moniker.ToLower())
                {
                    return BadRequest("Speaker and camp do not match");
                }

                if (speaker.User.UserName != this.User.Identity.Name)
                {
                    return Forbid();
                }

                _repository.Delete(speaker);

                if (await _repository.SaveAllAsync())
                {
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception thrown while deleting speaker: {ex}");
            }

            return BadRequest("Could not delete speaker");
        }
    }
}
