using EcommerceCoza.BLL.Services.Contracts;
using ECommerceCoza.DAL.DataContext.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class ReviewController : Controller
{
    private readonly IReviewService _reviewService;
    private readonly IProductService _productService;
    private readonly UserManager<AppUser> _userManager;

    public ReviewController(IReviewService reviewService, IProductService productService, UserManager<AppUser> userManager)
    {
        _reviewService = reviewService;
        _productService = productService;
        _userManager = userManager;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Review review)
    {
        var userId = _userManager.GetUserId(User);

        if (await _reviewService.UserHasReviewedProductAsync(userId, review.ProductId))
        {
            TempData["Error"] = "Siz artıq bu məhsula rəy bildirmisiniz.";
            return RedirectToAction("Details", "Product", new { id = review.ProductId });
        }

        review.AppUserId = userId;
        review.CreatedAt = DateTime.Now;

        await _reviewService.CreateReviewAsync(review);
        await UpdateProductRating(review.ProductId);

        TempData["Success"] = "Rəyiniz uğurla əlavə edildi.";
        return RedirectToAction("Details", "Product", new { id = review.ProductId });
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var review = await _reviewService.GetReviewByIdAsync(id);
        if (review == null) return NotFound();

        var userId = _userManager.GetUserId(User);
        if (review.AppUserId != userId && !User.IsInRole("Admin"))
            return Forbid();

        int productId = review.ProductId;
        await _reviewService.DeleteReviewAsync(id);
        await UpdateProductRating(productId);

        return RedirectToAction("Details", "Product", new { id = productId });
    }


    private async Task UpdateProductRating(int productId)
    {
        var averageRating = await _reviewService.GetAverageRatingByProductIdAsync(productId);
        var reviewCount = await _reviewService.GetReviewCountByProductIdAsync(productId);

        var product = await _productService.GetByIdAsync(productId);
        if (product != null)
        {
            product.Rating = averageRating;
            product.ReviewCount = reviewCount;
            await _productService.UpdateAsync(product);
        }
    }
}