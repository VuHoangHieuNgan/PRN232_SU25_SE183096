using Microsoft.EntityFrameworkCore;
using Repositories.Entities;

namespace Repositories
{
    public class BrandRepository
    {
        protected readonly Summer2025HandbagDbContext _context;
        private readonly DbSet<Brand> _dbSet;
        public BrandRepository(Summer2025HandbagDbContext context)
        {
            _context = context;
            _dbSet = _context.Brands;
        }

        public async Task<List<Brand>> GetAllAsync() => await _dbSet.ToListAsync();

        public async Task<Brand?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);

    }
}
