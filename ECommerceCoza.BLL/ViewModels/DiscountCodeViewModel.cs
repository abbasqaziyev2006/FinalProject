using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceCoza.BLL.ViewModels
{
    public class DiscountCodeViewModel
    {
        public int Id { get; set; }
        public string? Code { get; set; }
        public int SalePercentage { get; set; }
    }
    public class DiscountCodeCreateViewModel
    {

    }

    public class DiscountCodeUpdateViewModel { }

}
