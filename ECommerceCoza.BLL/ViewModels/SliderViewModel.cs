using Microsoft.AspNetCore.Http;

namespace EcommerceCoza.BLL.ViewModels
{
    public class SliderViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Line1 { get; set; } = null!; // Tagline
        public string Line2 { get; set; } = null!; // Subtitle
        public string ImageName { get; set; } = null!;
        public string? Link { get; set; }
        public string? CategoryIcon { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class SliderCreateViewModel
    {
        public string Title { get; set; } = null!;
        public string Line1 { get; set; } = null!;
        public string Line2 { get; set; } = null!;
        public IFormFile ImageFile { get; set; } = null!;
        public string? Link { get; set; }
        public string? CategoryIcon { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class SliderUpdateViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Line1 { get; set; } = null!;
        public string Line2 { get; set; } = null!;
        public string? ImageName { get; set; }
        public IFormFile? ImageFile { get; set; }
        public string? Link { get; set; }
        public string? CategoryIcon { get; set; }
        public bool IsActive { get; set; }
    }
}

