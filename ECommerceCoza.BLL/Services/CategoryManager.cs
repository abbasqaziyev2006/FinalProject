using AutoMapper;
using EcommerceCoza.BLL.Services.Contracts;
using EcommerceCoza.BLL.ViewModels;
using EcommerceCoza.DAL.DataContext.Repositories.Contracts;
using ECommerceCoza.BLL.Constants;
using ECommerceCoza.DAL.DataContext.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EcommerceCoza.BLL.Services
{
    public class CategoryManager : CrudManager<Category, CategoryViewModel, CategoryCreateViewModel, CategoryUpdateViewModel>,
        ICategoryService
    {
        private readonly FileService _fileService;

        public CategoryManager(IRepository<Category> repository, IMapper mapper, FileService fileService)
            : base(repository, mapper)
        {
            _fileService = fileService;
        }

        public async Task<CategoryUpdateViewModel> GetCategoryUpdateViewModelAsync(int id)
        {
            var category = await Repository.GetByIdAsync(id);

            if (category == null)
                return null!;

            var categoryUpdateViewModel = Mapper.Map<CategoryUpdateViewModel>(category);
            
            return categoryUpdateViewModel;
        }

        public override async Task<bool> UpdateAsync(int id, CategoryUpdateViewModel model)
        {
            var existingCategory = await Repository.GetByIdAsync(id);

            if (existingCategory == null)
                return false;

            existingCategory = Mapper.Map(model, existingCategory);

            if (model.ImageFile != null)
            {
                if(!_fileService.IsImageFile(model.ImageFile))
                    throw new ArgumentException("The file is not a valid image. ", nameof(model.ImageFile));

                var prevImageName = existingCategory.ImageName;
                existingCategory.ImageName = await _fileService.GenerateFile(model.ImageFile, FilePathConstants.CategoryImagePath);

                if(!string.IsNullOrEmpty(prevImageName))
                {
                    var prevFilePath = Path.Combine(FilePathConstants.CategoryImagePath, prevImageName);

                    if(File.Exists(prevFilePath)) 
                        File.Delete(prevFilePath);
                }
            }

            await Repository.UpdateAsync(existingCategory);

            return true;
        }

        public override async Task CreateAsync (CategoryCreateViewModel model)
        {
            var category = Mapper.Map<Category>(model);

            if (model.ImageFile != null)
            {
                if (!_fileService.IsImageFile(model.ImageFile))
                    throw new ArgumentException("the file is not a valid image", nameof(model.ImageFile));

                category.ImageName = await _fileService.GenerateFile(model.ImageFile, FilePathConstants.CategoryImagePath);
            }

            await Repository.CreateAsync(category);
        }

        public async Task<List<SelectListItem>> GetCategorySelectListItemsAsync()
        {
            var categories = await GetAllAsync(predicate: x => !x.IsDeleted);

            return categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name,
            }).ToList();
        }


    }
}
