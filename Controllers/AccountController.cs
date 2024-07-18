using AutoMapper;
using FilmPreview.Data;
using FilmPreview.Model;
using FilmPreview.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FilmPreview.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ILogger<AccountController> _logger;
        private readonly UserManager<ApiUser> _userManager;
        private readonly IAuthManager _authManager;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly SignInManager<ApiUser> _signInManager;

        public AccountController(IMapper mapper, ILogger<AccountController> logger, UserManager<ApiUser> userManager, IAuthManager authManager, IHttpContextAccessor contextAccessor, SignInManager<ApiUser> signInManager)
        {
            _mapper = mapper;
            _logger = logger;
            _userManager = userManager;
            _authManager = authManager;
            _contextAccessor = contextAccessor;
            _signInManager = signInManager;
        }

        [HttpPost]
        [Route("registrater")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Register([FromBody] RegistryUserDTO userDTO)
        {
            _logger.LogError($"Registration attempt for {userDTO.Email}");
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var findemail = await _userManager.FindByEmailAsync(userDTO.Email);
                var findphone = await _userManager.FindByNameAsync(userDTO.PhoneNumber);
                if (findemail != null || findphone != null)
                {
                    if (findemail != null)
                    {
                        return BadRequest(new { message = "Email đã được sử dụng để đăng kí" });
                    }
                    return BadRequest(new { message = "Số điện thoại đã được sử dụng để đăng kí" });

                }
                else
                {
                    var user = _mapper.Map<ApiUser>(userDTO);
                    user.UserName = userDTO.PhoneNumber;
                    user.UserList = new UserList();
                    user.Image = "https://static2.yan.vn/YanNews/2167221/202102/facebook-cap-nhat-avatar-doi-voi-tai-khoan-khong-su-dung-anh-dai-dien-e4abd14d.jpg";
                    var result = await _userManager.CreateAsync(user, userDTO.Password);
                    if (!result.Succeeded)
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(error.Code, error.Description);
                        }
                        return BadRequest(ModelState);
                    }
                    await _userManager.AddToRoleAsync(user, "user");
                    return Accepted(new { message = "Đăng kí thành công !!"});
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Something went wrong in the {nameof(Register)}");
                return Problem($"Something went wrong in the {nameof(Register)}", statusCode: 500);
            }
        }

        [HttpPost]
        [Route("login")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> login([FromBody] LoginUserDTO userDTO)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var user = await _userManager.FindByEmailAsync(userDTO.Account);
                if (user == null)
                {
                    return Unauthorized(new { message = "Số điện thoại hoặc email không chính xác" });
                }
                if (!await _authManager.ValidateUser(userDTO))
                {
                    return Unauthorized(new {message = "Mật khẩu không chính xác"});
                }
                _contextAccessor.HttpContext.Response.Cookies.Append("token", await _authManager.CreateToken(userDTO), new CookieOptions { HttpOnly = true });
                
                return Accepted();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Something went wrong in the {nameof(login)}");
                return Problem($"Something went wrong in the {nameof(login)}", statusCode: 500);
            }

        }
        [HttpGet]
        [Route("logout")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> logout()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                if(_contextAccessor.HttpContext != null)
                {
                    _contextAccessor.HttpContext.Response.Cookies.Delete("token");
                }
                return Accepted();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Something went wrong in the {nameof(logout)}");
                return Problem($"Something went wrong in the {nameof(logout)}", statusCode: 500);
            }

        }


        [Authorize]
        [HttpGet]
        [Route("UserInformation")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUser()
        {
            try
            {
                var username = User.Claims.FirstOrDefault(o => o.Type.Equals("Name", StringComparison.InvariantCultureIgnoreCase));
                if (username == null)
                {
                    var nouser = new UserDTO();
                    nouser.FullName = "";
                    nouser.Image = "";
                    return Ok(nouser);

                }
                var user = await _userManager.FindByNameAsync(username.Value);
                var result = _mapper.Map<UserDTO>(user);
                result.Roles = await _userManager.GetRolesAsync(user);
                return Ok(result);

            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, $"Something went wrong in the {nameof(GetUser)}");
                return StatusCode(500, "Internal Server Error. Please try again later.");
            }
        }

        [Authorize]
        [HttpPut]
        [Route("UpdateUser")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserDTO userDTO)
        {
            try
            {
                var username = User.Claims.FirstOrDefault(o => o.Type.Equals("Name", StringComparison.InvariantCultureIgnoreCase));
                if (username == null)
                {
                    return BadRequest();

                }
                var user = await _userManager.FindByNameAsync(username.Value);

                if(userDTO.PhoneNumber != user.PhoneNumber)
                {
                    user.PhoneNumber = userDTO.PhoneNumber;
                    user.UserName = userDTO.PhoneNumber;
                }
                if (userDTO.Address != user.Address)
                {
                    user.Address = userDTO.Address;
                }
                if (userDTO.FullName != user.FullName)
                {
                    user.FullName = userDTO.FullName;
                }
                if (userDTO.FullName != user.FullName)
                {
                    user.FullName = userDTO.FullName;
                }
                if (userDTO.Image != user.Image && userDTO.Image != "")
                {
                    user.Image = userDTO.Image;
                }
                await _userManager.UpdateAsync(user);

                return Accepted();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Something went wrong in the {nameof(UpdateUser)}");
                return StatusCode(500, "Internal Server Error. Please try again later.");
            }
        }

        
    }
}
