using EcommerceCoza.DAL.DataContext.Repositories.Contracts;
using ECommerceCoza.DAL.DataContext;
using ECommerceCoza.DAL.DataContext.Entities;
using Microsoft.EntityFrameworkCore;

namespace EcommerceCoza.DAL.DataContext.Repositories
{
    public class BrandRepository : EFCoreRepository<Brand>, IBrandRepository
    {
        private readonly AppDbContext _dbContext;

        public BrandRepository(AppDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Brand?> GetByIdWithProductsAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Brands
                .Include(b => b.Products)
                .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        }

        public async Task<List<Brand>> GetAllWithProductsAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Brands
                .Include(b => b.Products)
                .ToListAsync(cancellationToken);
        }

        public async Task<Brand?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Brands
                .FirstOrDefaultAsync(b => b.Name == name, cancellationToken);
        }

        public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Brands
                .AnyAsync(b => b.Id == id, cancellationToken);
        }

        public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Brands
                .AnyAsync(b => b.Name == name, cancellationToken);
        }

        public async Task<List<Brand>> SearchByNameAsync(string searchTerm, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Brands
                .Where(b => b.Name.Contains(searchTerm))
                .ToListAsync(cancellationToken);
        }

        public async Task<int> GetProductCountAsync(int brandId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Products
                .CountAsync(p => p.BrandId == brandId, cancellationToken);
        }

        //public async Task<List<Brand>> GetActiveBrandsAsync(CancellationToken cancellationToken = default)
        //{
        //    return await _dbContext.Brands
        //        .Where(b => b.IsActive) // Assuming you have an IsActive property
        //        .ToListAsync(cancellationToken);
        //}
    }
}