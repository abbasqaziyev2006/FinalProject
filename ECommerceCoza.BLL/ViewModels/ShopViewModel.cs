using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceCoza.BLL.ViewModels
{
    public class ShopViewModel
    {
        public List<CategoryViewModel> Categories { get; set; } = [];
        public List<ProductViewModel> Products { get; set; } = [];
        public List<BrandViewModel> Brands { get; set; } = [];
        public List<ColorViewModel> Colors { get; set; } = [];
    }
}
