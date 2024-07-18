using AutoMapper;
using FilmPreview.Data;
using FilmPreview.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FilmPreview.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserListController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ILogger<UserListController> _logger;
        private readonly DatabaseContext _context;
        private readonly UserManager<ApiUser> _userManager;

        public UserListController(IMapper mapper, ILogger<UserListController> logger, UserManager<ApiUser> userManager, DatabaseContext context)
        {
            _logger = logger;
            _mapper = mapper;
            _userManager = userManager;
            _context = context;
        }

        [Authorize]
        [HttpGet]
        [Route("GetUserList")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCart()
        {
            try
            {
                var username = User.Claims.FirstOrDefault(o => o.Type.Equals("Name", StringComparison.InvariantCultureIgnoreCase));
                if (username == null)
                {
                    return BadRequest();
                }
                var user = await _userManager.FindByNameAsync(username.Value);

                if (user == null)
                {
                    return BadRequest();
                }
                var cart = await _context.UserFilms.Where(q => q.Us == user.Id).ToListAsync();
                var results = _mapper.Map<IList<CartItemDTO>>(cart);
                foreach (var item in results)
                {
                    var product = await _context.Products.FirstOrDefaultAsync(q => q.Id == item.ProductId);
                    item.Name = product.Name;
                    item.Image = product.Image;
                }
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Something went wrong in the {nameof(GetCart)}");

                return StatusCode(500, $"{ex}");
            }
        }

        [Authorize]
        [HttpPost]
        [Route("AddProductToCart/")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddProductToCart([FromBody] AddCartDTO addCartDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                if(addCartDTO.Quantity < 0)
                {
                    return BadRequest();
                }
                var username = User.Claims.FirstOrDefault(o => o.Type.Equals("Name", StringComparison.InvariantCultureIgnoreCase));

                if (addCartDTO == null || username == null)
                {
                    return BadRequest();
                }

                var user = await _userManager.FindByNameAsync(username.Value);

                if (user == null)
                {
                    return BadRequest();
                }
                //Create cart item

                var product = await _context.Products.FirstOrDefaultAsync(q => q.Id == addCartDTO.ProductId);
                if(addCartDTO.Quantity == 0)
                {
                    return BadRequest();
                }

                var cartItem = _mapper.Map<UserList>(addCartDTO);

                //Check whether product exist in cart
                var cart = await _unitOfWork.CartItems.Get(q => q.ApiUserId == user.Id && q.ProductId == cartItem.ProductId);
                if (cart != null )
                {
                    if(cart.Quantity > 0)
                    {
                        cart.Quantity += cartItem.Quantity;
                        if(cart.Quantity <= 0)
                        {
                            await _unitOfWork.CartItems.Delete(cart.Id);
                            await _unitOfWork.Save();
                            return Accepted();
                        }
                        cart.Price += cartItem.Price;
                        await _unitOfWork.Save();
                        return Accepted(new { message = "Đã thêm vào giỏ hàng" });
                    }
                    else
                    {
                        return BadRequest();
                    }

                }
                //Case product donot exist in cart
                cartItem.ApiUserId = user.Id;
                cartItem.Price = product.Cost * cartItem.Quantity;
                await _unitOfWork.CartItems.Insert(cartItem);
                await _unitOfWork.Save();

                return Accepted(new { message = "Đã thêm vào giỏ hàng" });
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, $"Something went wrong in the {nameof(AddProductToCart)}");

                return StatusCode(500, $"{ex}");
            }
        }

    }
}
