using Microsoft.EntityFrameworkCore;
using Repositories.Basic;
using Repositories.Entities;

namespace Repositories
{
    public class AccountsRepository : GenericRepository<SystemAccount>
    {
        public AccountsRepository(Summer2025HandbagDbContext context) : base(context)
        {
        }

        public async Task<SystemAccount?> GetUserAccount(string email, string password)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email && u.Password == password);

            //return await _context.SystemAccounts.FirstOrDefaultAsync(u => u.Email == email && u.Password == password && u.IsActive == true);
        }
    }
}
