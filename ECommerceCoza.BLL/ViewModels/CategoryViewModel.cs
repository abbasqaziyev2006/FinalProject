using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceCoza.BLL.ViewModels
{
    public class CategoryViewModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? ImageName { get; set; }
        public bool IsDeleted { get; set; }
        public List<ProductViewModel> Products { get; set; } = [];

    }

    public class CategoryCreateViewModel
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? ImageName { get; set; }
        public IFormFile ImageFile { get; set; } = null!;
    }

    public class CategoryUpdateViewModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? ImageName { get; set; }
        public IFormFile? ImageFile { get; set; }
    }
}
