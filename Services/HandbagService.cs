using EVCharging.Repositories.NganVHH.ModelExtensions;
using Repositories;
using Repositories.Entities;

namespace Services
{
    public class HandbagService
    {
        private readonly HandbagRepository _repo;
        private readonly BrandRepository _subRepo;
        public HandbagService(HandbagRepository repo, BrandRepository subRepo)
        {
            _repo = repo;
            _subRepo = subRepo;
        }
        public async Task<List<Handbag>> GetAllAsync() => await _repo.GetAllAsync();

        public async Task<Handbag?> GetByIdAsync(int id) => await _repo.GetByIdAsync(id);

        public IQueryable<Handbag> GetQueryable()
        {
            return _repo.GetQueryable();
        }

        public async Task<List<Handbag>> SearchAsync(string? modelName, string? material)
        {
            return await _repo.SearchAsync(modelName, material);
        }

        public async Task<PaginationResult<List<Handbag>>> SearchWithPaginationAsync(SearchRequestDto request)
        {
            return await _repo.SearchWithPaginationAsync(request);
        }

        public async Task<int> CreateAsync(Handbag entity)
        {
            var subEntity = await _subRepo.GetByIdAsync(entity.BrandId ?? 0);
            if (subEntity == null)
                throw new KeyNotFoundException($"Brand with ID {entity.BrandId} not found.");

            return await _repo.CreateAsync(entity);
        }

        public async Task<int> UpdateAsync(Handbag entity)
        {
            var subEntity = await _subRepo.GetByIdAsync(entity.BrandId ?? 0);
            if (subEntity == null)
                throw new KeyNotFoundException($"Brand with ID {entity.BrandId} not found.");

             return await _repo.UpdateAsync(entity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null)
                return false;

            return await _repo.RemoveAsync(existing);
        }
    }
}
