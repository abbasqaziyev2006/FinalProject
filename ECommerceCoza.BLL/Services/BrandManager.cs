using AutoMapper;
using EcommerceCoza.BLL.Services;
using EcommerceCoza.BLL.Services.Contracts;
using EcommerceCoza.BLL.ViewModels;
using EcommerceCoza.DAL.DataContext.Repositories.Contracts;
using ECommerceCoza.BLL.Constants;
using ECommerceCoza.DAL.DataContext.Entities;

namespace ECommerceCoza.BLL.Services
{
    public class BrandManager : CrudManager<Brand, BrandViewModel, BrandCreateViewModel, BrandUpdateViewModel>,
        IBrandService
    {
        private readonly IBrandRepository _brandRepository;
        private readonly IMapper _mapper;
        private readonly FileService _fileService;

        public BrandManager(IBrandRepository brandRepository, IMapper mapper, FileService fileService)
            : base(brandRepository, mapper)
        {
            _brandRepository = brandRepository;
            _mapper = mapper;
            _fileService = fileService;
        }

        public override async Task CreateAsync(BrandCreateViewModel model)
        {
            var brand = _mapper.Map<Brand>(model);

            if (model.Logo != null)
            {
                if (!_fileService.IsImageFile(model.Logo))
                    throw new ArgumentException("The file is not a valid image.", nameof(model.Logo));

                var imageName = await _fileService.GenerateFile(model.Logo, FilePathConstants.BrandImagePath);
                brand.LogoUrl = "/images/brands/" + imageName;
            }

            await _brandRepository.CreateAsync(brand);
        }

        public override async Task<bool> UpdateAsync(int id, BrandUpdateViewModel model)
        {
            var existingBrand = await _brandRepository.GetByIdAsync(id);

            if (existingBrand == null)
                return false;

            _mapper.Map(model, existingBrand);

            if (model.LogoFile != null)
            {
                if (!_fileService.IsImageFile(model.LogoFile))
                    throw new ArgumentException("The file is not a valid image.", nameof(model.LogoFile));

                // Delete old logo if exists
                if (!string.IsNullOrEmpty(existingBrand.LogoUrl))
                {
                    var oldFileName = Path.GetFileName(existingBrand.LogoUrl);
                    var oldFilePath = Path.Combine(FilePathConstants.BrandImagePath, oldFileName);
                    if (File.Exists(oldFilePath))
                        File.Delete(oldFilePath);
                }

                var imageName = await _fileService.GenerateFile(model.LogoFile, FilePathConstants.BrandImagePath);
                existingBrand.LogoUrl = "/images/brands/" + imageName;
            }

            await _brandRepository.UpdateAsync(existingBrand);
            return true;
        }

        // ... existing methods (GetActiveBrandsAsync, GetByNameAsync, etc.)
        public async Task<List<BrandViewModel>> GetActiveBrandsAsync()
        {
            var brands = await _brandRepository.GetAllAsync(
                predicate: b => b.IsActive,
                orderBy: q => q.OrderBy(b => b.Name),
                AsNoTracking: true);

            return _mapper.Map<List<BrandViewModel>>(brands);
        }

        public async Task<BrandViewModel?> GetByNameAsync(string name)
        {
            var brand = await _brandRepository.GetAsync(
                predicate: b => b.Name == name,
                AsNoTracking: true);

            return _mapper.Map<BrandViewModel?>(brand);
        }

        public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null)
        {
            var exists = await _brandRepository.GetAsync(
                predicate: excludeId.HasValue
                    ? b => b.Name == name && b.Id != excludeId.Value
                    : b => b.Name == name,
                AsNoTracking: true);

            return exists != null;
        }

        public async Task<int> GetTotalBrandsCountAsync()
        {
            var brands = await _brandRepository.GetAllAsync(AsNoTracking: true);
            return brands.Count();
        }

        public async Task<int> GetActiveBrandsCountAsync()
        {
            var brands = await _brandRepository.GetAllAsync(
                predicate: b => b.IsActive,
                AsNoTracking: true);

            return brands.Count();
        }

        public async Task<List<BrandViewModel>> GetBrandsWithPaginationAsync(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var brands = await _brandRepository.GetAllAsync(
                orderBy: q => q.OrderByDescending(b => b.CreatedAt),
                AsNoTracking: true);

            var paginatedBrands = brands
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return _mapper.Map<List<BrandViewModel>>(paginatedBrands);
        }

        public async Task<bool> ToggleActiveStatusAsync(int id)
        {
            var brand = await _brandRepository.GetByIdAsync(id);

            if (brand == null)
                return false;

            brand.IsActive = !brand.IsActive;
            brand.UpdatedAt = DateTime.UtcNow;

            await _brandRepository.UpdateAsync(brand);
            return true;
        }
    }
}