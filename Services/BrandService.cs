using Repositories;
using Repositories.Entities;

namespace Services
{
    public class BrandService
    {
        private readonly BrandRepository _repo;
        public BrandService(BrandRepository repo) => _repo = repo;
        public async Task<List<Brand>> GetAllAsync() => await _repo.GetAllAsync();
    }
}
