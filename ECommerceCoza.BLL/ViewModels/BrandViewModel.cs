using EcommerceCoza.BLL.ViewModels;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

public class BrandViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public List<ProductViewModel>? Products { get; set; }
}

public class BrandCreateViewModel
{
    [Required(ErrorMessage = "Brand name is required")]
    [StringLength(100, ErrorMessage = "Brand name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }

    public IFormFile? Logo { get; set; }

    public bool IsActive { get; set; } = true;
}

public class BrandUpdateViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Brand name is required")]
    [StringLength(100, ErrorMessage = "Brand name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }

    public string? CurrentLogoUrl { get; set; }

    public IFormFile? LogoFile { get; set; }

    public bool IsActive { get; set; }
}