using System.ComponentModel.DataAnnotations;
using EcommerceCoza.BLL.ViewModels;

namespace EcommerceCoza.BLL.ViewModels
{
    public class FooterViewModel
    {
        public BioViewModel? Bio { get; set; }
        public List<SocialViewModel> Socials { get; set; } = [];
    }

}
