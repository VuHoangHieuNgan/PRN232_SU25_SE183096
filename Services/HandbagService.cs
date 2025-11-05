using Microsoft.EntityFrameworkCore;
using Repositories;
using Repositories.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class HandbagService
    {
        private readonly HandbagRepository _repo;
        public HandbagService() => _repo = new();
        public async Task<List<Handbag>> GetAllAsync() => await _repo.GetAllAsync();

        public async Task<Handbag?> GetByIdAsync(int id) => await _repo.GetByIdAsync(id);

        public IQueryable<Handbag> GetQueryable()
        {
            return _repo.GetQueryable();
        }

        public async Task<int> CreateAsync(Handbag entity)
        {
            return await _repo.CreateAsync(entity);
        }

        public async Task<int> UpdateAsync(Handbag entity) => await _repo.UpdateAsync(entity);

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null)
                return false;

            return await _repo.RemoveAsync(existing);
        }
    }
}
