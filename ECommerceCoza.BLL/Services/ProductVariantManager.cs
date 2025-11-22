using AutoMapper;
using EcommerceCoza.BLL.Services.Contracts;
using EcommerceCoza.BLL.ViewModels;
using EcommerceCoza.DAL.DataContext.Repositories.Contracts;
using ECommerceCoza.BLL.Constants;
using ECommerceCoza.DAL.DataContext.Entities;
using Microsoft.EntityFrameworkCore;

namespace EcommerceCoza.BLL.Services
{
    public class ProductVariantManager : CrudManager<ProductVariant, ProductVariantViewModel, ProductVariantCreateViewModel, ProductVariantUpdateViewModel>,
        IProductVariantService
    {
        private readonly IColorService _colorService;
        private readonly FileService _fileService;
        private readonly IProductService _productService;

        public ProductVariantManager(IRepository<ProductVariant> repository, IMapper mapper, IColorService colorService, FileService fileService, IProductService productService)
            : base(repository, mapper)
        {
            _colorService = colorService;
            _fileService = fileService;
            _productService = productService;
        }

        public async Task<ProductVariantCreateViewModel> GetCreateViewModelAsync()
        {
            var model = new ProductVariantCreateViewModel();
            model.ColorSelectListItems = await _colorService.GetColorSelectListItemsAsync();
            model.ProductSelectListItems = await _productService.GetProductSelectListItemsAsync();
            
            return model;
        }

        public override async Task CreateAsync(ProductVariantCreateViewModel model)
        {
            var productVariant = Mapper.Map<ProductVariant>(model);


            if (model.CoverImageFile != null)
            {
                if (!_fileService.IsImageFile(model.CoverImageFile))
                    throw new ArgumentException("the file is not a valid image", nameof(model.CoverImageFile));

                productVariant.CoverImageName = await _fileService.GenerateFile(model.CoverImageFile, FilePathConstants.ProductImagePath);
            }

            if (model.ImageFiles != null && model.ImageFiles.Any())
            {
                productVariant.ProductImages = new List<ProductImage>();

                foreach (var imageFile in model.ImageFiles)
                {
                    if (!_fileService.IsImageFile(imageFile))
                        throw new ArgumentException("one of the files is not a valid image", nameof(model.ImageFiles));
                }

                foreach (var imageFile in model.ImageFiles)
                {
                    var imageName = await _fileService.GenerateFile(imageFile, FilePathConstants.ProductImagePath);
                    productVariant.ProductImages.Add(new ProductImage
                    {
                        ImageName = imageName,
                    });
                }
            }

            await Repository.CreateAsync(productVariant);
        }

        public async Task<ProductVariantUpdateViewModel> GetProductVariantUpdateViewModelAsync(int id)
        {
            var productVariant = await Repository.GetAsync(
                predicate: p => p.Id == id,
                include: x => x.Include(i => i.ProductImages!)
                .Include(c=>c.Color!)
                .Include(p=>p.Product!));

            if (productVariant == null)
                return null!;

            var model = Mapper.Map<ProductVariantUpdateViewModel>(productVariant);
            model.ProductSelectListItems = await _productService.GetProductSelectListItemsAsync();
            model.ColorSelectListItems = await _colorService.GetColorSelectListItemsAsync();

            return model;
        }

        public override async Task<bool> UpdateAsync(int id, ProductVariantUpdateViewModel model)
        {
            var existingVariant = await Repository.GetByIdAsync(id);

            if (existingVariant == null)
                return false;

            existingVariant = Mapper.Map(model, existingVariant);

            if (model.CoverImageFile != null)
            {
                if (!_fileService.IsImageFile(model.CoverImageFile))
                    throw new ArgumentException("The file is not a valid image.", nameof(model.CoverImageFile));

                var oldCoverImageName = existingVariant.CoverImageName;
                existingVariant.CoverImageName = await _fileService.GenerateFile(model.CoverImageFile, FilePathConstants.ProductImagePath);

                if (!string.IsNullOrEmpty(oldCoverImageName))
                {
                    var oldFilePath = Path.Combine(FilePathConstants.ProductImagePath, oldCoverImageName);
                    if (File.Exists(oldFilePath))
                        File.Delete(oldFilePath);
                }
            }

            if (model.ImageFiles != null && model.ImageFiles.Any())
            {
                existingVariant.ProductImages = new List<ProductImage>();
                foreach (var imageFile in model.ImageFiles)
                {
                    if (!_fileService.IsImageFile(imageFile))
                        throw new ArgumentException("One of the files is not a valid image.", nameof(model.ImageFiles));
                }

                foreach (var imageFile in model.ImageFiles)
                {
                    var imageName = await _fileService.GenerateFile(imageFile, FilePathConstants.ProductImagePath);
                    existingVariant.ProductImages.Add(new ProductImage
                    {
                        ImageName = imageName
                    });
                }
            }

            await Repository.UpdateAsync(existingVariant);

            return true;
        }
    }
}
