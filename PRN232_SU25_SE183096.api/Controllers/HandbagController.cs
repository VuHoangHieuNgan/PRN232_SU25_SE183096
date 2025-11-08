using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Validator;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
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
        private readonly BrandService _subService;

        public HandbagController(HandbagService service, BrandService subService)
        {
            _service = service;
            _subService = subService;
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

            return Ok(new { message = "Deleted successfully" });
        }

        [HttpGet("search-group-by-odata")]
        public async Task<IActionResult> Search([FromServices] IEdmModel edmModel)
        {
            var ctx = new ODataQueryContext(edmModel, typeof(Handbag), new ODataPath()); 
            var options = new ODataQueryOptions<Handbag>(ctx, Request);

            IQueryable<Handbag> q = _service.GetQueryable().AsNoTracking();

            var validate = new ODataValidationSettings
            {
                AllowedQueryOptions = AllowedQueryOptions.Filter |
                                      AllowedQueryOptions.OrderBy |
                                      AllowedQueryOptions.Skip |
                                      AllowedQueryOptions.Top
            };

            
            options.Validate(validate);

            var settings = new ODataQuerySettings { HandleNullPropagation = HandleNullPropagationOption.True };

            if (options.Filter != null) q = (IQueryable<Handbag>)options.Filter.ApplyTo(q, settings);
            if (options.OrderBy != null) q = options.OrderBy.ApplyTo(q, settings);
            if (options.Skip != null) q = options.Skip.ApplyTo(q, settings);
            if (options.Top != null) q = options.Top.ApplyTo(q, settings);

            var filtered = (IQueryable<Handbag>)options.ApplyTo(q, settings);


            var list = await filtered.ToListAsync();

            var grouped = list
                .GroupBy(h => h.Brand?.BrandName ?? "(No brand)")
                .Select(g => new
                {
                    brandName = g.Key,
                    handbags = g.Select(h => new
                    {
                        h.HandbagId,
                        h.ModelName,
                        h.Material,
                        h.Color,
                        h.Price,
                        h.Stock,
                        h.ReleaseDate
                    }).ToList()
                })
                .ToList();

            return Ok(grouped);
        }


        [HttpGet("search-group-by")]
        public async Task<IActionResult> Search([FromQuery] string? modelName, [FromQuery] string? material)
        {
            var handbags = await _service.SearchAsync(modelName, material);

            // Group by brand name
            var groupedHandbags = handbags
                .GroupBy(h => h.Brand?.BrandName ?? "Unknown")
                .Select(g => new
                {
                    brandName = g.Key,
                    handbags = g.Select(h => new
                    {
                        h.HandbagId,
                        h.ModelName,
                        h.Material,
                        h.Color,
                        h.Price,
                        h.Stock,
                        h.ReleaseDate
                    }).ToList()
                })
                .ToList();

            return Ok(groupedHandbags);
        }


        [HttpGet("search-no-paging")]
        public async Task<ActionResult<IEnumerable<Handbag>>> SearchNoPaging(string? modelName, string? material)
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
