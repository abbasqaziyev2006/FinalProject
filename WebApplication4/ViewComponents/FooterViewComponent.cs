using EcommerceCoza.BLL.Services.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication4.ViewComponents
{
    public class FooterViewComponent : ViewComponent
    {
        private readonly IFooterService _footerService;

        public FooterViewComponent(IFooterService footerService)
        {
            _footerService = footerService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var footerViewModel = await _footerService.GetFooterViewModelAsync();
            return View(footerViewModel);
        }
    }
}