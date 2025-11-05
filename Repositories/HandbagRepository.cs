using Microsoft.EntityFrameworkCore;
using Repositories.Entities;

namespace Repositories
{
    public class HandbagRepository
    {
        protected readonly Summer2025HandbagDbContext _context;
        public HandbagRepository()
        {
            _context ??= new();
        }

        public HandbagRepository(Summer2025HandbagDbContext context) => _context = context;

        public async Task<List<Handbag>> GetAllAsync() => await _context.Handbags
            .Include(i => i.Brand)
            .OrderByDescending(i => i.HandbagId)
            .ToListAsync();

        public IQueryable<Handbag> GetQueryable()
        {
            return _context.Handbags
                .Include(i => i.Brand)
                .AsQueryable();
        }

        public async Task<Handbag?> GetByIdAsync(int id) => await _context.Handbags.Where(i => i.HandbagId == id).Include(i => i.Brand).FirstOrDefaultAsync();

        public async Task<int> CreateAsync(Handbag entity)
        {
            entity.HandbagId = _context.Handbags.Max(x => x.HandbagId) + 1;
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
        //public async Task<List<Handbag>> SearchAsync(string? planName, string? planType, int? year)
        //{
        //    return await _context.Handbags.Include(i => i.Brand)
        //        .Where(i => (i.PlanName.Contains(planName) || string.IsNullOrEmpty(planName))
        //                && (i.PlanType.Contains(planType) || string.IsNullOrEmpty(planType))
        //                && (i.Vehicle.Year == year || year == 0 || year == null))
        //        .ToListAsync() ?? new List<Handbag>();
        //}

        //public async Task<List<Handbag>> SearchAsync(string modelName, double? weight)
        //{
        //    var list = await _context.Handbags
        //        .Include(i => i.Brand)
        //        .Where(i => (string.IsNullOrEmpty(modelName) || i.ModelName.ToLower().Contains(modelName.ToLower()))
        //        && (!weight.HasValue || i.Weight == weight))
        //        .ToListAsync();

        //    return list ?? new List<Handbag>();
        //}

        //public async Task<List<LionProfile>> SearchAsync(string lionTypeName, string lionName)
        //{
        //    var list = await _context.LionProfiles
        //        .Include(i => i.LionType)
        //        .Where(i => (string.IsNullOrEmpty(lionTypeName) || i.LionType.LionTypeName.ToLower().Contains(lionTypeName.ToLower()))
        //        && (string.IsNullOrEmpty(lionName) || i.LionName.ToLower().Contains(lionName.ToLower())))
        //        .OrderByDescending(i => i.LionProfileId)
        //        .ToListAsync();

        //    return list ?? new List<LionProfile>();
        //}

        //public (List<LionProfile> list, int totalCount) GetPaginatedProfiles(int pageIndex, int pageSize)
        //{
        //    var list = _context.LionProfiles
        //        .Include(i => i.LionType)
        //        .OrderByDescending(i => i.LionProfileId)
        //        .Skip((pageIndex - 1) * pageSize)
        //        .Take(pageSize)
        //        .ToList() ?? new List<LionProfile>();
        //    var totalCount = _context.LionProfiles.Count();

        //    return (list, totalCount);
        //}
    }
}
