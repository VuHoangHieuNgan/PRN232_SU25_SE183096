using Microsoft.EntityFrameworkCore;
using Repositories.Basic;
using Repositories.Entities;

namespace Repositories
{
    public class BrandRepository(Summer2025HandbagDbContext context) : GenericRepository<Brand>(context)
    {
    }
}
