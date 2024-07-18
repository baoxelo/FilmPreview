using AutoMapper;
using FilmPreview.Data;
using FilmPreview.Model;
using FilmPreview.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FilmPreview.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilmController : ControllerBase
    {
        private readonly ConvertSlug _convertSlug;
        private readonly DatabaseContext _databaseContext;
        private readonly IMapper _mapper;
        private readonly ILogger<FilmController> _logger;

        public FilmController(IMapper mapper, ILogger<FilmController> logger, DatabaseContext databaseContext, ConvertSlug convertSlug)
        {
            _mapper = mapper;
            _logger = logger;
            _databaseContext = databaseContext; 
            _convertSlug = convertSlug;
        }
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get()
        {
            try
            {
                var films = await _databaseContext.Films.ToListAsync();
                var results = _mapper.Map<IList<FilmDTO>>(films);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Something went wrong in the {nameof(Get)}");
                return StatusCode(500, "Internal Server Error. Please try again later.");
            }
        }

        [HttpGet("{slug:string}", Name = "GetFilms")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetFilm(string slug)
        {
            try
            {
                var film = await _databaseContext.Films.FirstOrDefaultAsync(q => q.Slug == slug);
                if (film == null)
                {
                    var filmename = _convertSlug.ConvertSlug2String(slug);
                    return NotFound($"Can not found the film {filmename}");
                }
                film.FilmGenres = await _databaseContext.FilmGenre.Where(q => q.FilmId == film.Id).Include(q => q.Genre).ToListAsync();
                var result = _mapper.Map<FilmDTO>(film);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Something went wrong in the {nameof(GetFilm)}");
                return StatusCode(500, "Internal Server Error. Please try again later.");
            }
        }

        [HttpGet("Search={name}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetSearchFilms(string name)
        {
            try
            {
                var films = await _databaseContext.Films.Where(q => q.Name.Contains(name)).Include(q => q.FilmGenres).ThenInclude(q => q.Genre).ToListAsync();
                var results = _mapper.Map<IList<FilmDTO>>(films);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Something went wrong in the {nameof(GetSearchFilms)}");
                return StatusCode(500, "Internal Server Error. Please try again later.");
            }
        }

        [Authorize(Roles = "administrator")]
        [HttpPost]
        [Route("CreateFilm")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateFilm([FromBody] CreateFilmDTO filmDTO)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var film = _mapper.Map<Film>(filmDTO);
                return CreatedAtRoute("GetFilm", new { id = film.Id }, film);
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, $"Something went wrong in the {nameof(CreateFilm)}");

                return StatusCode(500,$"{ex}");
            }
        }

        [Authorize(Roles = "administrator")]
        [HttpPut]
        [Route("UpdateFilm/{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateFilm(int id, [FromBody] UpdateFilmDTO filmDTO)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var film = await _databaseContext.Films.FirstOrDefaultAsync(q => q.Id == id);
                if(film == null)
                {
                    _logger.LogError($"Invalid update attempt in {nameof(UpdateFilm)}");
                    return BadRequest(ModelState);
                };
                _mapper.Map(filmDTO,film);
                _databaseContext.Films.Update(film);
                await _databaseContext.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Something went wrong in the {nameof(UpdateFilm)}");
                return StatusCode(500, "Internal Server Error. Please try again later.");
            }
        }

        [Authorize(Roles = "administrator")]
        [HttpDelete]
        [Route("DeleteFilm/{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteFilm(int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var film = await _databaseContext.Films.FirstOrDefaultAsync(q => q.Id == id);
                if (film == null)
                {
                    _logger.LogError($"Invalid update attempt in {nameof(DeleteFilm)}");
                    return BadRequest(ModelState);
                };
                _databaseContext.Films.Remove(film);
                await _databaseContext.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Something went wrong in the {nameof(DeleteFilm)}");
                return StatusCode(500, "Internal Server Error. Please try again later.");
            }
        }
    }
}
