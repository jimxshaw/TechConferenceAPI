using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyCodeCamp.Data;

namespace MyCodeCamp.Controllers
{
    [Route("api/camps/{moniker}/speakers")]
    public class SpeakersController : BaseController
    {
        private IMapper _mapper;
        private ILogger<SpeakersController> _logger;
        private ICampRepository _repository;

        public SpeakersController(ICampRepository repository,
                                    ILogger<SpeakersController> logger,
                                    IMapper mapper)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult Get(string moniker)
        {
            var speakers = _repository.GetSpeakersByMoniker(moniker);

            return Ok(speakers);
        }

        [HttpGet("{id}")]
        public IActionResult Get(string moniker, int id)
        {
            var speaker = _repository.GetSpeaker(id);

            if (speaker == null)
            {
                return NotFound();
            }

            if (speaker.Camp.Moniker.ToLower() != moniker.ToLower())
            {
                return BadRequest("Speaker not in specified Camp");
            }

            return Ok(speaker);
        }
    }
}
