using Microsoft.EntityFrameworkCore;
using Repositories.Basic;
using Repositories.Entities;
using Repositories.ModelExtensions;

namespace Repositories
{
    public class HandbagRepository : GenericRepository<Handbag>
    {
        public HandbagRepository(Summer2025HandbagDbContext context) : base(context)
        {
        }

        public async Task<List<Handbag>> GetAllAsync()
        {
            var query = GetQueryable();

            query = query.OrderByDescending(i => i.HandbagId).AsNoTracking();
            return await query.ToListAsync();
        }


        public IQueryable<Handbag> GetQueryable()
            => _dbSet
            .Include(i => i.Brand)
            .AsQueryable();


        public override async Task<Handbag?> GetByIdAsync(int id) 
            => await GetQueryable().FirstAsync(h => h.HandbagId == id);

        public async Task<int> CreateAsync(Handbag entity)
        {
            entity.HandbagId = _dbSet.Max(x => x.HandbagId) + 1;

            return await base.CreateAsync(entity);
        }

        public async Task<int> UpdateAsync(Handbag entity)
        {
            entity.Brand = null;
            return await base.UpdateAsync(entity);
        }

        public async Task<List<Handbag>> SearchAsync(string? modelName, string? material)
        {
            var query = GetQueryable();

            if (!string.IsNullOrWhiteSpace(modelName))
            {
                query = query.Where(h => h.ModelName.Contains(modelName));
            }

            if (!string.IsNullOrWhiteSpace(material))
            {
                query = query.Where(h => h.Material != null && h.Material.Contains(material));
            }

            query.OrderByDescending(h => h.HandbagId);

            query.AsNoTracking();

            return await query.ToListAsync();
        }

        public async Task<List<Handbag>> SearchAsync(string? modelName, decimal? price)
        {
            var query = GetQueryable();

            if (!string.IsNullOrWhiteSpace(modelName))
            {
                query = query.Where(h => h.ModelName.Contains(modelName));
            }

            if (price.HasValue)
            {
                query = query.Where(h => h.Price == price);
            }

            query.OrderByDescending(h => h.HandbagId);

            query.AsNoTracking();

            return await query.ToListAsync();
        }



        public async Task<PaginationResult<List<Handbag>>> SearchWithPaginationAsync(SearchRequestDto request)
        {
            var items = await SearchAsync(request.ModelName, request.Material);
            var totalItems = items.Count();
            var totalPages = (int)Math.Ceiling((double)totalItems / request.PageSize.Value);

            items = items.Skip((request.CurrentPage.Value - 1) * request.PageSize.Value).Take(request.PageSize.Value).ToList();

            var result = new PaginationResult<List<Handbag>>
            {
                TotalItems = totalItems,
                TotalPages = totalPages,
                CurrentPages = request.CurrentPage.Value,
                PageSizes = request.PageSize.Value,
                Items = items
            };
            return result;
        }
    }
}
