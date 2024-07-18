using AutoMapper;
using FilmPreview.Data;
using FilmPreview.IResponsitory;
using FilmPreview.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FilmPreview.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoiceController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ILogger<InvoiceController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApiUser> _userManager;

        public InvoiceController(IMapper mapper, ILogger<InvoiceController> logger, IUnitOfWork unitOfWork, UserManager<ApiUser> userManager)
        {
            _logger = logger;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        [Authorize]
        [HttpGet]
        [Route("GetInvoices")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetInvoices()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                //Validate User
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
                var invoices = await _unitOfWork.Invoices.GetAll(q => q.ApiUserId == user.Id);
                foreach ( var invoice in invoices)
                {
                    invoice.Items = await _unitOfWork.InvoiceItems.GetAll(q => q.InvoiceId == invoice.Id);
                }
                var invoicesDTO = _mapper.Map<IList<CommentDTO>>(invoices);

                //Get the product info
                foreach (var invoiceDTO in invoicesDTO)
                {
                    foreach (var item in invoiceDTO.Items)
                    {
                        var productItem = await _unitOfWork.Products.Get(q => q.Id ==  item.ProductId);
                        item.UnitPrice = productItem.Cost;
                        item.Image = productItem.Image;
                        item.Name = productItem.Name;
                    }
                }

                return Ok(invoicesDTO);
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, $"Something went wrong in the {nameof(GetInvoices)}");

                return StatusCode(500, $"{ex}");
            }
        }

        [Authorize]
        [HttpGet]
        [Route("CreateInvoice")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateInvoice()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                //Validate User
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
                var cartItems = await _unitOfWork.CartItems.GetAll(q => q.ApiUserId == user.Id);

                var invoiceDTO = new CommentDTO
                {
                    Items = _mapper.Map<IList<InvoiceItemDTO>>(cartItems),
                    CreatedDate = DateTime.Now,
                    PhoneNumber = user.PhoneNumber,
                    Address = user.Address,
                    TotalPrice = 0,
                    Status = "Chưa giao"
                };
                //Remove items in cart and count totalPrice
                foreach(var cartItem in cartItems)
                {
                    invoiceDTO.TotalPrice +=  cartItem.Price;
                    await _unitOfWork.CartItems.Delete(cartItem.Id);
                }
                var invoice = _mapper.Map<Comment>(invoiceDTO);
                invoice.ApiUserId = user.Id;
                invoice.CreatedDate = DateTime.Now;
                await _unitOfWork.Invoices.Insert(invoice);
                await _unitOfWork.Save();

                return Accepted(new { message = "Đặt đơn hàng thành công"});
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, $"Something went wrong in the {nameof(CreateInvoice)}");

                return StatusCode(500, $"{ex}");
            }
        }

        [Authorize]
        [HttpPost]
        [Route("CreateInvoiceUnauth")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateInvoiceUnauth([FromBody] CreateInvoiceDTO invoiceDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                //Validate User
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
                var cartItems = await _unitOfWork.CartItems.GetAll(q => q.ApiUserId == user.Id);
                if(invoiceDTO == null)
                {
                    return BadRequest();
                }
                invoiceDTO.Items = _mapper.Map<IList<InvoiceItemDTO>>(cartItems);
                invoiceDTO.CreatedDate = DateTime.Now;
                invoiceDTO.Status = "Chưa giao";
                var invoice = _mapper.Map<Comment>(invoiceDTO);
                
                //Remove items in cart and count totalPrice
                foreach (var cartItem in cartItems)
                {
                    invoice.TotalPrice += cartItem.Price;
                    await _unitOfWork.CartItems.Delete(cartItem.Id);
                }
                invoice.ApiUserId = user.Id;
                await _unitOfWork.Invoices.Insert(invoice);
                await _unitOfWork.Save();

                return Accepted(new {message = "Đơn hàng mới đã được tạo" });
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, $"Something went wrong in the {nameof(CreateInvoiceUnauth)}");

                return StatusCode(500, $"{ex}");
            }
        }

        
    }
    
}
