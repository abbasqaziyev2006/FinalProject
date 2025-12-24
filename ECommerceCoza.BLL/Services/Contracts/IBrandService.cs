using EcommerceCoza.BLL.ViewModels;
using ECommerceCoza.DAL.DataContext.Entities;

namespace EcommerceCoza.BLL.Services.Contracts
{
    public interface IBrandService
         : ICrudService<Brand, BrandViewModel, BrandCreateViewModel, BrandUpdateViewModel>
    {
        Task<List<BrandViewModel>> GetActiveBrandsAsync();
        Task<BrandViewModel?> GetByNameAsync(string name);
        Task<bool> ExistsByNameAsync(string name, int? excludeId = null);
        Task<int> GetTotalBrandsCountAsync();
        Task<int> GetActiveBrandsCountAsync();
        Task<List<BrandViewModel>> GetBrandsWithPaginationAsync(int pageNumber, int pageSize);
        Task<bool> ToggleActiveStatusAsync(int id);
    }
}


