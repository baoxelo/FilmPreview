using AutoMapper;
using FilmPreview.Data;
using FilmPreview.Model;
using FilmPreview.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace FilmPreview.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ILogger<AccountController> _logger;
        private readonly UserManager<ApiUser> _userManager;
        private readonly IAuthManager _authManager;
        private readonly DatabaseContext _dbContext;

        public AdminController(IMapper mapper, ILogger<AccountController> logger, UserManager<ApiUser> userManager, IAuthManager authManager, DatabaseContext context)
        {
            _mapper = mapper;
            _logger = logger;
            _userManager = userManager;
            _authManager = authManager;
            _dbContext = context;
        }
        [Authorize(Roles = "administrator")]
        [HttpGet]
        [Route("GetAllUsers")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _userManager.GetUsersInRoleAsync("user");
                var results = _mapper.Map<IList<UserDTO>>(users);
                return Ok(results);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Something went wrong in the {nameof(GetAllUsers)}");
                return StatusCode(500, "Internal Server Error. Please try again later.");
            }
        }

        [Authorize(Roles = "administrator")]
        [HttpPut]
        [Route("BanUser/{email}")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> BanUser(string email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return BadRequest();
                }
                else
                {
                    user.Status = "banned";
                    await _userManager.UpdateAsync(user);
                    return Accepted(new { message = $"Cấm tài khoản có email {email} thành công" });
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Something went wrong in the {nameof(BanUser)}");
                return StatusCode(500, "Internal Server Error. Please try again later.");
            }
        }
        [Authorize(Roles = "administrator")]
        [HttpPut]
        [Route("UnbanUser/{email}")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UnBanUser(string email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return BadRequest();
                }
                else
                {
                    user.Status = "active";
                    await _userManager.UpdateAsync(user);
                    return Accepted(new { message = $"Khôi phục tài khoản có email {email} thành công" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Something went wrong in the {nameof(UnBanUser)}");
                return StatusCode(500, "Internal Server Error. Please try again later.");
            }
        }

        [Authorize(Roles = "administrator")]
        [HttpGet]
        [Route("GetInvoices")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetInvoicesAdmin()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var invoices = await _dbContext.Invoices.ToListAsync();
                foreach (var invoice in invoices)
                {
                    invoice.Items = await _dbContext.InvoiceItems.Where(q => q.InvoiceId == invoice.Id).ToListAsync();
                }
                var invoicesDTO = _mapper.Map<IList<CommentDTO>>(invoices);

                //Get the product info
                foreach (var invoiceDTO in invoicesDTO)
                {
                    foreach (var item in invoiceDTO.Items)
                    {
                        var productItem = await _dbContext.Products.FirstOrDefaultAsync(q => q.Id == item.ProductId);
                        item.UnitPrice = productItem.Cost;
                        item.Image = productItem.Image;
                        item.Name = productItem.Name;
                    }
                }

                return Ok(invoicesDTO);
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, $"Something went wrong in the {nameof(GetInvoicesAdmin)}");

                return StatusCode(500, $"{ex}");
            }
        }

        [Authorize(Roles = "administrator")]
        [HttpGet]
        [Route("CompleteInvoice/{id:int}")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CompleteInvoice(int id)
        {
            try
            {
                var invoice = await _unitOfWork.Invoices.Get(q => q.Id == id);
                if (invoice == null)
                {
                    return BadRequest();
                }
                invoice.Status = "Đã giao";
                await _unitOfWork.Save();
                return Accepted();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Something went wrong in the {nameof(CompleteInvoice)}");
                return StatusCode(500, "Internal Server Error. Please try again later.");
            }
        }

        [Authorize(Roles = "administrator")]
        [HttpGet]
        [Route("CancelInvoice/{id:int}")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CancelInvoice(int id)
        {
            try
            {
                var invoice = await _unitOfWork.Invoices.Get(q => q.Id == id);
                if (invoice == null)
                {
                    return BadRequest();
                }
                await _unitOfWork.Invoices.Delete(invoice.Id);
                await _unitOfWork.Save();
                return Accepted();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Something went wrong in the {nameof(CancelInvoice)}");
                return StatusCode(500, "Internal Server Error. Please try again later.");
            }
        }
    }
}
