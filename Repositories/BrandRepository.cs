using Microsoft.EntityFrameworkCore;
using Repositories.Basic;
using Repositories.Entities;

namespace Repositories
{
    public class BrandRepository : GenericRepository<Brand>
    {
        public BrandRepository(Summer2025HandbagDbContext context) : base(context)
        {
        }
    }
}
