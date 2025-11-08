using EVCharging.Repositories.NganVHH.ModelExtensions;
using Microsoft.EntityFrameworkCore;
using Repositories.Entities;

namespace Repositories
{
    public class HandbagRepository
    {
        protected readonly Summer2025HandbagDbContext _context;
        private readonly DbSet<Handbag> _dbSet;
        public HandbagRepository()
        {
            _context ??= new();
            _dbSet = _context.Handbags;
        }

        public HandbagRepository(Summer2025HandbagDbContext context)
        {
            _context = context;
            _dbSet = context.Handbags;
        }

        public async Task<List<Handbag>> GetAllAsync()
            => await _dbSet
            .Include(i => i.Brand)
            .OrderByDescending(i => i.HandbagId)
            .ToListAsync();

        public IQueryable<Handbag> GetQueryable()
            => _dbSet
            .Include(i => i.Brand)
            .AsQueryable();

        public async Task<Handbag?> GetByIdAsync(int id)
            => await _dbSet
            .Where(i => i.HandbagId == id)
            .Include(i => i.Brand)
            .FirstOrDefaultAsync();

        public async Task<int> CreateAsync(Handbag entity)
        {
            entity.HandbagId = _dbSet.Max(x => x.HandbagId) + 1;
            
            _context.Add(entity);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> UpdateAsync(Handbag entity)
        {
            _context.ChangeTracker.Clear();
            entity.Brand = null;
            var tracker = _context.Attach(entity);
            tracker.State = EntityState.Modified;
            return await _context.SaveChangesAsync();
        }

        public async Task<bool> RemoveAsync(Handbag entity)
        {
            _context.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Handbag>> SearchAsync(string? modelName, string? material)
        {
            var query = _dbSet.Include(h => h.Brand).AsQueryable();

            if (!string.IsNullOrWhiteSpace(modelName))
            {
                query = query.Where(h => h.ModelName.Contains(modelName));
            }

            if (!string.IsNullOrWhiteSpace(material))
            {
                query = query.Where(h => h.Material != null && h.Material.Contains(material));
            }

            return await query.ToListAsync();
        }

        public async Task<List<Handbag>> SearchNumericalAsync(string? modelName, decimal? price)
        {
            var query = _dbSet.Include(h => h.Brand).AsQueryable();

            if (!string.IsNullOrWhiteSpace(modelName))
            {
                query = query.Where(h => h.ModelName.Contains(modelName));
            }

            if (price.HasValue)
            {
                query = query.Where(h => h.Price == price);
            }

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
