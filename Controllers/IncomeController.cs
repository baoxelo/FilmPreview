using AutoMapper;
using FilmPreview.Data;
using FilmPreview.IResponsitory;
using FilmPreview.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace FilmPreview.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "administrator")]
    public class IncomeController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ILogger<InvoiceController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApiUser> _userManager;
        public IncomeController(IMapper mapper, ILogger<InvoiceController> logger, IUnitOfWork unitOfWork, UserManager<ApiUser> userManager)
        {
            _logger = logger;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }
        [HttpPost]
        [Route("Income")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> StaticIncome([FromBody] CreateIncomeDTO createIncomeDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                //Init income static
                Income incomeStatic = new Income();
                incomeStatic.Name = createIncomeDTO.Name;
                incomeStatic.TotalInvoices = 0;
                incomeStatic.CreateDate = DateTime.Now;
                incomeStatic.BeginDate = createIncomeDTO.BeginDate;
                incomeStatic.FinalDate = createIncomeDTO.EndDate;

                //Find all invoices between the time period
                var invoices = await _unitOfWork.Invoices.GetAll();

                foreach (var invoice in invoices)
                {
                    if (invoice.Status == "Đã giao")
                    {
                        incomeStatic.TotalInvoices++;
                        incomeStatic.TotalIncome += invoice.TotalPrice;
                        var invoiceItems = await _unitOfWork.InvoiceItems.GetAll(q => q.InvoiceId == invoice.Id);
                        foreach (var item in invoiceItems)
                        {
                            var productStatic = await _unitOfWork.ProductStatics.Get(q => q.ProductId == item.ProductId);
                            if(productStatic == null)
                            {
                                productStatic = new UserFilm();
                                productStatic.ProductId = item.ProductId;
                                productStatic.IncomeName = incomeStatic.Name;
                                productStatic.TotalQuantity = item.Quantity;
                                productStatic.TotalIncome = item.Price;
                                await _unitOfWork.ProductStatics.Insert(productStatic);
                            }
                            else
                            {
                                productStatic.TotalQuantity += item.Quantity;
                                productStatic.TotalIncome += item.Price;
                            }
                        }
                    }
                }
                await _unitOfWork.Incomes.Insert(incomeStatic);
                await _unitOfWork.Save();
                var incomeDTO = _mapper.Map<UserFilmDTO>(incomeStatic);
                return Accepted(incomeStatic);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Something went wrong in the {nameof(StaticIncome)}");

                return StatusCode(500, $"{ex}");
            }
        }
    }
}
