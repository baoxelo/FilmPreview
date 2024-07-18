using AutoMapper;
using FilmPreview.Data;
using FilmPreview.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FilmPreview.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GenreController : ControllerBase
    {
        private readonly DatabaseContext _databaseContext;
        private readonly IMapper _mapper;
        private readonly ILogger<GenreController> _logger;
        public GenreController(IMapper mapper, ILogger<GenreController> logger, DatabaseContext databaseContext)
        {
            _mapper = mapper;
            _logger = logger;
            _databaseContext = databaseContext;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetGenres()
        {
            try
            {
                var genres = await _databaseContext.Genres.ToListAsync();
                foreach (var item in genres)
                {
                    item.FilmList = await _databaseContext.FilmGenre.Where(q => q.GenreId == item.Id).Include(q => q.Film).ToListAsync();
                }
                var result = _mapper.Map<IList<GenreDTO>>(genres);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Something went wrong in the {nameof(GetGenres)}");
                return StatusCode(500, "Internal Server Error. Please try again later.");
            }
        }

        [HttpGet("{slug:string}", Name = "GetGenre")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetGenre(string slug)
        {
            try
            {
                var genre = await _databaseContext.Genres.FirstOrDefaultAsync(q => q.Slug == slug);
                var result = _mapper.Map<GenreDTO>(genre);

                var film = await _databaseContext.FilmGenre.Where(q => q.GenreId == genre.Id).Include(q => q.Film).ToListAsync();
                result.FilmList = _mapper.Map<List<FilmGenreDTO>>(film);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Something went wrong in the {nameof(GetGenre)}");
                return StatusCode(500, "Internal Server Error. Please try again later.");
            }
        }
        [Authorize(Roles = "administrator")]
        [HttpPost]
        [Route("CreateGenre")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateGenre([FromBody] CreateGenreDTO genreDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var genre = _mapper.Map<Genre>(genreDTO);
                var existGenre = await _databaseContext.Genres.AnyAsync(q => q.Name == genre.Name);
                if (existGenre) return Ok(new { message = "Genre is already exist" });

                await _databaseContext.Genres.AddAsync(genre);
                await _databaseContext.SaveChangesAsync();
                return Accepted(new {message = "Create genre successfully"});
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Something went wrong in the {nameof(CreateGenre)}");
                return StatusCode(500, "Internal Server Error. Please try again later.");
            }
        }

        [Authorize(Roles = "administrator")]
        [HttpPut]
        [Route("UpdateGenre/{id:int}")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateGenre(int id, [FromBody] UpdateGenreDTO genreDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {

                var genre = await _databaseContext.Genres.FirstOrDefaultAsync(q => q.Id == id );
                if (genre == null)
                {
                    _logger.LogError($"Invalid update attempt in {nameof(UpdateGenre)}");
                    return BadRequest(ModelState);
                };
                _mapper.Map(genreDTO,genre);
                _databaseContext.Genres.Update(genre);
                await _databaseContext.SaveChangesAsync();
                return Accepted(new { message = "Cập nhật danh mục thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Something went wrong in the {nameof(CreateGenre)}");
                return StatusCode(500, "Internal Server Error. Please try again later.");
            }
        }
    }
}
