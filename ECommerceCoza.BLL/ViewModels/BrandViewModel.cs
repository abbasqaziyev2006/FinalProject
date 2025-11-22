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
}


public class BrandCreateViewModel
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public IFormFile? Logo { get; set; }

    public bool IsActive { get; set; } = true;
}

public class BrandUpdateViewModel
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public string? CurrentLogoUrl { get; set; }

    public IFormFile? NewLogo { get; set; }

    public bool IsActive { get; set; }
}
