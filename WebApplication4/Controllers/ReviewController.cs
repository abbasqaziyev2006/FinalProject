using EcommerceCoza.BLL.Services.Contracts;
using EcommerceCoza.BLL.ViewModels;
using ECommerceCoza.DAL.DataContext.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;

[Authorize]
public class ReviewController : Controller
{
    private readonly IReviewService _reviewService;
    private readonly IProductService _productService;
    private readonly UserManager<AppUser> _userManager;
    private readonly IMapper _mapper;

    public ReviewController(IReviewService reviewService, IProductService productService, UserManager<AppUser> userManager, IMapper mapper)
    {
        _reviewService = reviewService;
        _productService = productService;
        _userManager = userManager;
        _mapper = mapper;
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
    public async Task<IActionResult> Delete(int id, string returnUrl = null)
    {
        var review = await _reviewService.GetReviewByIdAsync(id);
        if (review == null) return NotFound();

        var userId = _userManager.GetUserId(User);

        // Yalnız öz rəyini silə bilər
        if (review.AppUserId != userId)
            return Forbid();

        int productId = review.ProductId;
        await _reviewService.DeleteReviewAsync(id);
        await UpdateProductRating(productId);

        TempData["Success"] = "Rəyiniz uğurla silindi.";

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        var referer = Request.Headers["Referer"].ToString();
        if (referer.Contains("/Account/Reviews"))
        {
            return RedirectToAction("Reviews", "Account");
        }

        return RedirectToAction("Details", "Product", new { id = productId });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, string returnUrl = null)
    {
        var review = await _reviewService.GetReviewByIdAsync(id);
        if (review == null) return NotFound();

        var userId = _userManager.GetUserId(User);

        // Yalnız öz rəyini redaktə edə bilər
        if (review.AppUserId != userId)
            return Forbid();

        var product = await _productService.GetByIdAsync(review.ProductId);
        var viewModel = _mapper.Map<ReviewViewModel>(review);
        viewModel.ProductName = product?.Name;
        ViewBag.ReturnUrl = returnUrl;

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ReviewUpdateViewModel model, string returnUrl = null)
    {
        var existingReview = await _reviewService.GetReviewByIdAsync(model.Id);
        if (existingReview == null) return NotFound();

        var userId = _userManager.GetUserId(User);

        // Yalnız öz rəyini redaktə edə bilər
        if (existingReview.AppUserId != userId)
            return Forbid();

        if (!ModelState.IsValid)
        {
            var product = await _productService.GetByIdAsync(existingReview.ProductId);
            var viewModel = _mapper.Map<ReviewViewModel>(existingReview);
            viewModel.ProductName = product?.Name;
            viewModel.Comment = model.Comment;
            viewModel.Rating = model.Rating;
            ViewBag.ReturnUrl = returnUrl;
            return View(viewModel);
        }

        existingReview.Comment = model.Comment;
        existingReview.Rating = model.Rating;
        existingReview.UpdatedAt = DateTime.Now;

        await _reviewService.UpdateReviewAsync(existingReview);
        await UpdateProductRating(existingReview.ProductId);

        TempData["Success"] = "Rəyiniz uğurla yeniləndi.";

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Reviews", "Account");
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