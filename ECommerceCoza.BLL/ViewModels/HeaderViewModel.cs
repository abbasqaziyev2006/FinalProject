namespace EcommerceCoza.BLL.ViewModels
{
    public class HeaderViewModel
    {

        public List<BasketItemViewModel> BasketItems { get; set; } = new List<BasketItemViewModel>();

        public List<SocialViewModel> Socials { get; set; } = [];

    }

}
