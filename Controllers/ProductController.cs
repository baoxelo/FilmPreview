using AutoMapper;
using FilmPreview.Data;
using FilmPreview.IResponsitory;
using FilmPreview.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FilmPreview.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IUnitOfWork unitOfWork, IMapper mapper, ILogger<ProductController> logger)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _logger = logger;

        }
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProducts()
        {
            try
            {
                var products = await _unitOfWork.Products.GetAll(q => q.Status != "delete");
                var results = _mapper.Map<IList<FilmDTO>>(products);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Something went wrong in the {nameof(GetProducts)}");
                return StatusCode(500, "Internal Server Error. Please try again later.");
            }
        }
        [Authorize(Roles = "administrator")]
        [HttpGet]
        [Route("Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProductsByAdmin()
        {
            try
            {
                var products = await _unitOfWork.Products.GetAll();
                var results = _mapper.Map<IList<FilmDTO>>(products);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Something went wrong in the {nameof(GetProducts)}");
                return StatusCode(500, "Internal Server Error. Please try again later.");
            }
        }
        [HttpGet("{id:int}", Name = "GetProduct")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProduct(int id)
        {
            try
            {
                var product = await _unitOfWork.Products.Get(q => q.Id == id && q.Status != "delete", new List<string> { "Category" });
                var result = _mapper.Map<FilmDTO>(product);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Something went wrong in the {nameof(GetProduct)}");
                return StatusCode(500, "Internal Server Error. Please try again later.");
            }
        }

        [HttpGet("Search={name}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetSearchProduct(string name)
        {
            try
            {
                var products = await _unitOfWork.Products.GetAll(q => q.Name.Contains(name) && q.Status != "delete");
                var results = _mapper.Map<IList<FilmDTO>>(products);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Something went wrong in the {nameof(GetProduct)}");
                return StatusCode(500, "Internal Server Error. Please try again later.");
            }
        }

        [Authorize(Roles = "administrator")]
        [HttpPost]
        [Route("CreateProduct")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductDTO productDTO)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var product = _mapper.Map<Film>(productDTO);
                product.Status = "exist";
                await _unitOfWork.Products.Insert(product);
                await _unitOfWork.Save();
                return CreatedAtRoute("GetProduct", new { id = product.Id }, product);
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, $"Something went wrong in the {nameof(CreateProduct)}");

                return StatusCode(500,$"{ex}");
            }
        }

        [Authorize(Roles = "administrator")]
        [HttpPut]
        [Route("UpdateProduct/{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDTO productDTO)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var product = await _unitOfWork.Products.Get(q => q.Id == id);
                if(product == null)
                {
                    _logger.LogError($"Invalid update attempt in {nameof(UpdateProduct)}");
                    return BadRequest(ModelState);
                };
                _mapper.Map(productDTO,product);
                _unitOfWork.Products.Update(product);
                await _unitOfWork.Save();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Something went wrong in the {nameof(UpdateProduct)}");
                return StatusCode(500, "Internal Server Error. Please try again later.");
            }
        }

        [Authorize(Roles = "administrator")]
        [HttpDelete]
        [Route("DeleteProduct/{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var product = await _unitOfWork.Products.Get(q => q.Id == id);
                if (product == null)
                {
                    _logger.LogError($"Invalid update attempt in {nameof(DeleteProduct)}");
                    return BadRequest(ModelState);
                };
                product.Status = "delete";
                await _unitOfWork.Save();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Something went wrong in the {nameof(DeleteProduct)}");
                return StatusCode(500, "Internal Server Error. Please try again later.");
            }
        }
    }
}
