using Microsoft.EntityFrameworkCore;
using Repositories.Entities;

namespace Repositories
{
    public class AccountsRepository
    {
        protected readonly Summer2025HandbagDbContext _context;

        public AccountsRepository() 
        {
            _context ??= new();
        }

        public AccountsRepository(Summer2025HandbagDbContext context)
        {
            _context = context;
        }

        public async Task<SystemAccount?> GetUserAccount(string email, string password)
        {
            return await _context.SystemAccounts.FirstOrDefaultAsync(u => u.Email == email && u.Password == password);

            //return await _context.SystemAccounts.FirstOrDefaultAsync(u => u.Email == email && u.Password == password && u.IsActive == true);
        }
    }
}
