using EcommerceCoza.BLL.Services.Contracts;
using EcommerceCoza.BLL.ViewModels;

namespace EcommerceCoza.BLL.Services
{
    public class HeaderManager : IHeaderService
    {
        private readonly ISocialService _socialService;

        public HeaderManager(ISocialService socialService)
        {
            _socialService = socialService;
        }

        public async Task<HeaderViewModel> GetHeaderViewModelAsync()
        {
            var socials = await _socialService.GetAllAsync();

            var headerViewModel = new HeaderViewModel
            {
                Socials = socials.ToList()
            };

            return headerViewModel;
        }
    }

    public class FooterManager : IFooterService
    {
        private readonly ISocialService _socialService;
        private readonly IBioService _bioService;

        public FooterManager(ISocialService socialService, IBioService bioService)
        {
            _socialService = socialService;
            _bioService = bioService;
        }

        public async Task<FooterViewModel> GetFooterViewModelAsync()
        {
            var socials = await _socialService.GetAllAsync();
            var bio = await _bioService.GetAllAsync(predicate: x=>!x.IsDeleted);

            var footerViewModel = new FooterViewModel
            {
                Socials = socials.ToList(),
                Bio = bio.ToList().FirstOrDefault(),
            };

            return footerViewModel;
        }
    }
}
