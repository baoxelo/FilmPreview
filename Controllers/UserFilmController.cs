using AutoMapper;
using FilmPreview.Data;
using FilmPreview.Model;
using FilmPreview.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FilmPreview.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class UserFilmController : ControllerBase
    {
        private readonly DatabaseContext _databaseContext;
        private readonly UserManager<ApiUser> _userManager;
        private readonly IMapper _mapper;
        private readonly ILogger<UserFilmController> _logger;
        private readonly ConvertSlug _convertSlug;

        public UserFilmController(DatabaseContext databaseContext ,IMapper mapper, ILogger<UserFilmController> logger, UserManager<ApiUser> userManager, ConvertSlug convertSlug)
        {
            _databaseContext = databaseContext;
            _userManager = userManager;
            _mapper = mapper;
            _logger = logger;
            _convertSlug = convertSlug;

        }
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserFilms()
        {
            try
            {
                UserListDTO results;
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return NotFound();

                UserList userList = await _databaseContext.UserLists.FirstOrDefaultAsync(q => q.ApiUserId == user.Id);
                if(userList == null)
                {
                    userList = await CreateUserList(user);
                    results = _mapper.Map<UserListDTO>(userList);
                    return Ok(results);
                }

                results = _mapper.Map<UserListDTO>(userList);
                var films = await _databaseContext.UserFilms.Where(q => q.UserListId == userList.Id).Include(q => q.Film).Select(q => q.Film).ToListAsync();
                results.UserFilms = _mapper.Map<List<FilmDTO>>(films);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Something went wrong in the {nameof(GetUserFilms)}");
                return StatusCode(500, "Internal Server Error. Please try again later.");
            }
        }
        
        [HttpGet("{slug:string}", Name = "GetFilm")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetFilm(string slug)
        {
            try
            {
                var film = await _databaseContext.Films.FirstOrDefaultAsync(q => q.Slug == slug);
                if (film == null) return NotFound();

                var user = await _userManager.GetUserAsync(User);
                if (user == null) return NotFound();

                var userList = await _databaseContext.UserLists.FirstOrDefaultAsync(q => q.ApiUserId == user.Id);
                if (userList == null)
                {
                    userList = await CreateUserList(user);
                }
                var userFilm = new UserFilm()
                {
                    FilmId = film.Id,
                    UserListId = userList.Id,
                };
                await _databaseContext.AddAsync(userFilm);
                await _databaseContext.SaveChangesAsync();

                var filmName = _convertSlug.ConvertSlug2String(slug);
                return Ok(new { message = $"{filmName} has been added to your favorites." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Something went wrong in the {nameof(GetFilm)}");
                return StatusCode(500, "Internal Server Error. Please try again later.");
            }
        }


        private async Task<UserList> CreateUserList(ApiUser user)
        {
            UserList userList = new UserList() { ApiUserId = user.Id };
            await _databaseContext.AddAsync(userList);
            await _databaseContext.SaveChangesAsync();
            return userList;
        }
    }
}
