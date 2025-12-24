using AutoMapper;
using EcommerceCoza.BLL.Services.Contracts;
using EcommerceCoza.BLL.ViewModels;
using EcommerceCoza.DAL.DataContext.Repositories.Contracts;
using ECommerceCoza.BLL.Constants;
using ECommerceCoza.DAL.DataContext.Entities;

namespace EcommerceCoza.BLL.Services
{
    public class SliderManager : CrudManager<Slider, SliderViewModel, SliderCreateViewModel, SliderUpdateViewModel>,
        ISliderService
    {
        private readonly FileService _fileService;

        public SliderManager(IRepository<Slider> repository, IMapper mapper, FileService fileService)
            : base(repository, mapper)
        {
            _fileService = fileService;
        }

        public async Task<SliderUpdateViewModel> GetSliderUpdateViewModelAsync(int id)
        {
            var slider = await Repository.GetByIdAsync(id);

            if (slider == null)
                return null!;

            var sliderUpdateViewModel = Mapper.Map<SliderUpdateViewModel>(slider);

            return sliderUpdateViewModel;
        }

        public override async Task<bool> UpdateAsync(int id, SliderUpdateViewModel model)
        {
            var existingSlider = await Repository.GetByIdAsync(id);

            if (existingSlider == null)
                return false;

            // map incoming values into tracked entity
            Mapper.Map(model, existingSlider);

            if (model.ImageFile != null)
            {
                if (!_fileService.IsImageFile(model.ImageFile))
                    throw new ArgumentException("The file is not a valid image.", nameof(model.ImageFile));

                var prevImageName = existingSlider.ImageName;
                existingSlider.ImageName = await _fileService.GenerateFile(model.ImageFile, FilePathConstants.SliderImagePath);

                if (!string.IsNullOrEmpty(prevImageName))
                {
                    var prevFilePath = Path.Combine(FilePathConstants.SliderImagePath, prevImageName);

                    if (File.Exists(prevFilePath))
                        File.Delete(prevFilePath);
                }
            }

            await Repository.UpdateAsync(existingSlider);

            return true;
        }

        public override async Task CreateAsync(SliderCreateViewModel model)
        {
            var slider = Mapper.Map<Slider>(model);

            if (model.ImageFile != null)
            {
                if (!_fileService.IsImageFile(model.ImageFile))
                    throw new ArgumentException("The file is not a valid image", nameof(model.ImageFile));

                slider.ImageName = await _fileService.GenerateFile(model.ImageFile, FilePathConstants.SliderImagePath);
            }

            await Repository.CreateAsync(slider);
        }
    }
}

