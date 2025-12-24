using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class DiscountCodeCreateViewModel
    {
        public string Code { get; set; } = null!;

        public int SalePercentage { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class DiscountCodeUpdateViewModel
    {
        public int Id { get; set; }

        public string Code { get; set; } = null!;

        public int SalePercentage { get; set; }

        public bool IsActive { get; set; }
    }
}