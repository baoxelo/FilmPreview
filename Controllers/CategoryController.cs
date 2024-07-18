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
    public class CategoryController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CategoryController> _logger;
        public CategoryController(IUnitOfWork unitOfWork, IMapper mapper, ILogger<CategoryController> logger)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var categories = await _unitOfWork.Categories.GetAll();
                var result = _mapper.Map<IList<GenreDTO>>(categories);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Something went wrong in the {nameof(GetCategories)}");
                return StatusCode(500, "Internal Server Error. Please try again later.");
            }
        }

        [HttpGet("{id:int}", Name = "GetCategory")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCategory(int id)
        {
            try
            {
                var category = await _unitOfWork.Categories.Get(q => q.Id == id);
                var result = _mapper.Map<GenreDTO>(category);
                var product = await _unitOfWork.Products.GetAll(q => q.CategoryId == id && q.Status != "delete");
                result.Products = _mapper.Map<List<FilmDTO>>(product);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Something went wrong in the {nameof(GetCategory)}");
                return StatusCode(500, "Internal Server Error. Please try again later.");
            }
        }
        [Authorize(Roles = "administrator")]
        [HttpPost]
        [Route("CreateCategory")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDTO categoryDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                
                var category = _mapper.Map<Genre>(categoryDTO);
                await _unitOfWork.Categories.Insert(category);
                await _unitOfWork.Save();
                return Accepted(new {message = "Tạo danh mục thành công"});
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Something went wrong in the {nameof(CreateCategory)}");
                return StatusCode(500, "Internal Server Error. Please try again later.");
            }
        }

        [Authorize(Roles = "administrator")]
        [HttpPut]
        [Route("UpdateCategory/{id:int}")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryDTO categoryDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {

                var category = await _unitOfWork.Categories.Get(q => q.Id == id );
                if (category == null)
                {
                    _logger.LogError($"Invalid update attempt in {nameof(UpdateCategory)}");
                    return BadRequest(ModelState);
                };
                _mapper.Map(categoryDTO,category);
                _unitOfWork.Categories.Update(category);
                await _unitOfWork.Save();
                return Accepted(new { message = "Cập nhật danh mục thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Something went wrong in the {nameof(CreateCategory)}");
                return StatusCode(500, "Internal Server Error. Please try again later.");
            }
        }
    }
}
