using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Repositories.Entities;
using Repositories.ModelExtensions;
using Services;
using System.ComponentModel.DataAnnotations;

namespace PRN232_SU25_SE183096.api.Controllers
{
    public record CreateRequest
    (
        [Required(ErrorMessage = "modelName is required")]
        [RegularExpression(@"^([A-Za-z0-9]+(?:\s+[A-Za-z0-9]+)*)$",
            ErrorMessage = "modelName must contain only letters/numbers, separated by spaces")]
        string ModelName,

        [Required(ErrorMessage = "material is required")]
        string Material,

        [Required(ErrorMessage = "price is required")]
        [Range(typeof(decimal), "0.000000001", "79228162514264337593543950335",
            ErrorMessage = "price must be greater than 0")]
        decimal Price,

        [Required(ErrorMessage = "stock is required")]
        [Range(1, int.MaxValue, ErrorMessage = "stock must be greater than 0")]
        int Stock,

        [Required(ErrorMessage = "brandId is required")]
        [Range(1, int.MaxValue, ErrorMessage = "brandId must be greater than 0")]
        int BrandId
    );

    public record UpdateRequest : CreateRequest
    {
        public string? Color { get; init; }
        public DateOnly? ReleaseDate { get; init; }



        public UpdateRequest(string modelName, string material, decimal price, int stock, int brandId,
                                    string? color, DateOnly? releaseDate)
            : base(modelName, material, price, stock, brandId)
        {
            Color = color;
            ReleaseDate = releaseDate;
        }
    }

    [Route("api/handbags")]
    [ApiController]
    [Authorize]
    public class HandbagController : ControllerBase
    {
        private readonly HandbagService _service;

        public HandbagController(HandbagService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Handbag>>> GetAll()
        {
            return await _service.GetAllAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Handbag>> GetById(int id)
        {
            var entity = await _service.GetByIdAsync(id);

            if (entity == null)
                throw new KeyNotFoundException($"ID {id} not found");

            return entity;
        }

        [HttpPost]
        [Authorize(Roles = "1, 2")]
        public async Task<IActionResult> Create([FromBody] CreateRequest request)
        {
            var entity = new Handbag
            {
                ModelName = request.ModelName,
                Material = request.Material,
                Price = request.Price,
                Stock = request.Stock,
                BrandId = request.BrandId
            };

            var newId = await _service.CreateAsync(entity);

            entity = await _service.GetByIdAsync(entity.HandbagId);

            return CreatedAtAction(nameof(GetById), new { id = entity.HandbagId }, entity);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "1, 2")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateRequest request)
        {
            var existingEntity = await _service.GetByIdAsync(id);

            if (existingEntity == null)
                throw new KeyNotFoundException($"ID {id} not found");

            existingEntity.ModelName = request.ModelName;
            existingEntity.Material = request.Material;
            existingEntity.Price = request.Price;
            existingEntity.Stock = request.Stock;
            existingEntity.BrandId = request.BrandId;
            existingEntity.Color = request.Color;
            existingEntity.ReleaseDate = request.ReleaseDate;

            await _service.UpdateAsync(existingEntity);

            var updatedEntity = await _service.GetByIdAsync(id);

            return Ok(updatedEntity);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "1, 2")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _service.DeleteAsync(id);

            if (!success)
                throw new KeyNotFoundException($"ID {id} not found");

            return Ok(new { message = "Deleted successfully"});
        }

        [EnableQuery] // vẫn hỗ trợ $select, $orderby, $top, $expand...
        [HttpGet("search")]
        public ActionResult<IQueryable<Handbag>> Search()
        {
            var q = _service.GetQueryable(); 

            return Ok(q);
        }

        [HttpGet("search-no-paging")]
        public async Task<ActionResult<IEnumerable<Handbag>>> Search(string? modelName, string? material)
        {
            try
            {
                return await _service.SearchAsync(modelName, material);
            }
            catch (Exception ex)
            {
                return NotFound();
            }
        }

        // POST: api/PaymentNganVhh
        [Authorize(Roles = "1, 2")]
        [HttpPost("search-paging")]
        public async Task<PaginationResult<List<Handbag>>> SearchWithPaging(SearchRequestDto request)
        {
            return await _service.SearchWithPaginationAsync(request);
        }



    }
}
