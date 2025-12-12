using EcommerceCoza.BLL.Services.Contracts;
using EcommerceCoza.BLL.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EcommerceCoza.MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IBrandService _brandService;
        private readonly IColorService _colorService;
        private readonly IProductVariantService _productVariantService;

        public ProductController(
            IProductService productService,
            ICategoryService categoryService,
            IBrandService brandService,
            IColorService colorService,
            IProductVariantService productVariantService)
        {
            _productService = productService;
            _categoryService = categoryService;
            _brandService = brandService;
            _colorService = colorService;
            _productVariantService = productVariantService;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _productService.GetAllAsync(
                predicate: p => !p.IsDeleted,
                include: q => q
                    .Include(p => p.Category!)
                    .Include(p => p.Brand!)
                    .Include(p => p.ProductVariants!)
            );

            return View(products.ToList());
        }

        public async Task<IActionResult> Create()
        {
            var categories = await _categoryService.GetAllAsync(predicate: c => !c.IsDeleted);
            var brands = await _brandService.GetAllAsync(predicate: b => !b.IsDeleted && b.IsActive);

            var model = new ProductCreateViewModel
            {
                CategorySelectListItems = categories.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToList(),
                BrandSelectListItems = brands.Select(b => new SelectListItem
                {
                    Value = b.Id.ToString(),
                    Text = b.Name
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] ProductCreateViewModel model, [FromForm] List<VariantDto> variants)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid form data" });
            }

            try
            {
                // Create product first
                await _productService.CreateAsync(model);

                // Get the created product by name (since we just created it)
                var allProducts = await _productService.GetAllAsync(
                    predicate: p => p.Name == model.Name && !p.IsDeleted,
                    include: q => q.Include(p => p.ProductVariants!)
                );

                var product = allProducts.OrderByDescending(p => p.Id).FirstOrDefault();

                if (product == null)
                {
                    return Json(new { success = false, message = "Failed to create product" });
                }

                // Create variants if provided
                if (variants != null && variants.Any())
                {
                    foreach (var variantDto in variants)
                    {
                        var variantModel = new ProductVariantCreateViewModel
                        {
                            ProductId = product.Id,
                            Size = variantDto.Size,
                            ColorId = variantDto.ColorId,
                            Price = variantDto.Price,
                            Quantity = variantDto.Quantity,
                            CoverImageFile = variantDto.CoverImageFile,
                            ImageFiles = variantDto.ImageFiles ?? new List<IFormFile>()
                        };

                        await _productVariantService.CreateAsync(variantModel);
                    }
                }

                TempData["SuccessMessage"] = $"Product '{model.Name}' created with {variants?.Count ?? 0} variant(s)";
                return Json(new
                {
                    success = true,
                    message = $"Product created successfully with {variants?.Count ?? 0} variant(s)"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productService.GetByIdAsync(id);

            if (product == null)
            {
                TempData["ErrorMessage"] = "Product not found.";
                return NotFound();
            }

            var categories = await _categoryService.GetAllAsync(predicate: c => !c.IsDeleted);
            var brands = await _brandService.GetAllAsync(predicate: b => !b.IsDeleted && b.IsActive);

            var model = new ProductUpdateViewModel
            {
                Id = product.Id,
                Name = product.Name!,
                Description = product.Description!,
                AdditionalInformation = product.AdditionalInformation,
                BasePrice = product.BasePrice,
                CategoryId = product.CategoryId,
                BrandId = product.BrandId,
                CategorySelectListItems = categories.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToList(),
                BrandSelectListItems = brands.Select(b => new SelectListItem
                {
                    Value = b.Id.ToString(),
                    Text = b.Name
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductUpdateViewModel model)
        {
            if (id != model.Id)
                return BadRequest();

            if (!ModelState.IsValid)
            {
                await LoadSelectListsAsync(model);
                return View(model);
            }

            try
            {
                var isUpdated = await _productService.UpdateAsync(id, model);

                if (!isUpdated)
                {
                    TempData["ErrorMessage"] = "Product not found.";
                    return NotFound();
                }

                TempData["SuccessMessage"] = $"Product '{model.Name}' updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error updating product: {ex.Message}");
                await LoadSelectListsAsync(model);
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var product = await _productService.GetByIdAsync(id);
                var productName = product?.Name ?? "Unknown";

                var isDeleted = await _productService.DeleteAsync(id);

                if (!isDeleted)
                {
                    TempData["ErrorMessage"] = "Product not found.";
                    return NotFound();
                }

                TempData["SuccessMessage"] = $"Product '{productName}' deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting product: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // ========== AJAX Methods ==========

        [HttpGet]
        public async Task<IActionResult> GetColors()
        {
            try
            {
                var colors = await _colorService.GetAllAsync(predicate: c => !c.IsDeleted);
                var colorList = colors.Select(c => new
                {
                    id = c.Id,
                    name = c.Name,
                    hexCode = c.HexCode
                }).ToList();

                return Json(colorList);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetVariants(int productId)
        {
            try
            {
                var variants = await _productVariantService.GetAllAsync(
                    predicate: pv => !pv.IsDeleted && pv.ProductId == productId,
                    include: q => q
                        .Include(pv => pv.Color!)
                        .Include(pv => pv.ProductImages!)
                );

                return PartialView("_VariantsList", variants.ToList());
            }
            catch (Exception ex)
            {
                return PartialView("_VariantsList", new List<ProductVariantViewModel>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetVariantForEdit(int id)
        {
            var model = await _productVariantService.GetProductVariantUpdateViewModelAsync(id);
            if (model == null)
                return NotFound("Variant not found");

            return PartialView("_EditVariantForm", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateVariant([FromForm] ProductVariantCreateViewModel model)
        {
            try
            {
                await _productVariantService.CreateAsync(model);
                return Json(new { success = true, message = "Variant created successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateVariant([FromForm] ProductVariantUpdateViewModel model)
        {
            try
            {
                var isUpdated = await _productVariantService.UpdateAsync(model.Id, model);

                if (!isUpdated)
                    return Json(new { success = false, message = "Variant not found" });

                return Json(new { success = true, message = "Variant updated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteVariant(int id)
        {
            try
            {
                var isDeleted = await _productVariantService.DeleteAsync(id);

                if (!isDeleted)
                    return Json(new { success = false, message = "Variant not found" });

                return Json(new { success = true, message = "Variant deleted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private async Task LoadSelectListsAsync(dynamic model)
        {
            var categories = await _categoryService.GetAllAsync(predicate: c => !c.IsDeleted);
            var brands = await _brandService.GetAllAsync(predicate: b => !b.IsDeleted && b.IsActive);

            model.CategorySelectListItems = categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }).ToList();

            model.BrandSelectListItems = brands.Select(b => new SelectListItem
            {
                Value = b.Id.ToString(),
                Text = b.Name
            }).ToList();
        }
    }

    // Helper DTO class
    public class VariantDto
    {
        public string? Size { get; set; }
        public int ColorId { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public IFormFile? CoverImageFile { get; set; }
        public List<IFormFile>? ImageFiles { get; set; }
    }
}