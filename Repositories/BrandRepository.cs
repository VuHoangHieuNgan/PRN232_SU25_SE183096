using Microsoft.EntityFrameworkCore;
using Repositories.Entities;

namespace Repositories
{
    public class BrandRepository
    {
        protected readonly Summer2025HandbagDbContext _context;
        public BrandRepository()
        {
            _context ??= new();
        }

        public BrandRepository(Summer2025HandbagDbContext context) => _context = context;

        public async Task<List<Brand>> GetAllAsync() => await _context.Brands.ToListAsync();

    }
}
