using Microsoft.EntityFrameworkCore;
using Repositories.Entities;

namespace Repositories
{
    public class AccountsRepository
    {
        protected readonly Summer2025HandbagDbContext _context;
        private readonly DbSet<SystemAccount> _dbSet;

        public AccountsRepository() 
        {
            _context ??= new();
            _dbSet = _context.SystemAccounts;
        }

        public AccountsRepository(Summer2025HandbagDbContext context)
        {
            _context = context;
            _dbSet = _context.SystemAccounts;
        }

        public async Task<SystemAccount?> GetUserAccount(string email, string password)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email && u.Password == password);

            //return await _context.SystemAccounts.FirstOrDefaultAsync(u => u.Email == email && u.Password == password && u.IsActive == true);
        }
    }
}
