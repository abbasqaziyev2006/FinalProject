using AutoMapper;
using EcommerceCoza.BLL.Services.Contracts;
using EcommerceCoza.BLL.ViewModels;
using ECommerceCoza.DAL.DataContext.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;


namespace WebApplication4.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
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

        // GET: Admin/Review
        public async Task<IActionResult> Index(string searchString = "", int pageNumber = 1, int pageSize = 10)
        {
          
            var reviews = (await _reviewService.GetAllReviewsAsync()).ToList();

            foreach (var review in reviews)
            {
                if (review.Product == null && review.ProductId > 0)
                {
       
                    var productVm = await _productService.GetByIdAsync(review.ProductId);
                    if (productVm != null)
                    {
                        review.Product = new Product
                        {
                            Id = productVm.Id,
                            Name = productVm.Name ?? string.Empty
                        };
                    }
                }

                if (review.AppUser == null && !string.IsNullOrEmpty(review.AppUserId))
                {
                    review.AppUser = await _userManager.FindByIdAsync(review.AppUserId);
                }
            }

           
            if (!string.IsNullOrEmpty(searchString))
            {
                reviews = reviews.Where(r =>
                    (r.Comment != null && r.Comment.Contains(searchString, StringComparison.OrdinalIgnoreCase)) ||
                    (r.Product?.Name != null && r.Product.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase)) ||
                    (r.AppUser?.UserName != null && r.AppUser.UserName.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }

            var totalItems = reviews.Count;
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var paginatedReviews = reviews
                .OrderByDescending(r => r.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.SearchString = searchString;

            return View(paginatedReviews);
        }

        // GET: Admin/Review/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var review = await _reviewService.GetReviewByIdAsync(id);
            if (review == null) return NotFound();

            var viewModel = _mapper.Map<ReviewViewModel>(review);

            if (review.ProductId > 0)
            {
                var product = await _productService.GetByIdAsync(review.ProductId);
                viewModel.ProductName = product?.Name;
            }

            if (!string.IsNullOrEmpty(review.AppUserId))
            {
                var user = await _userManager.FindByIdAsync(review.AppUserId);
                viewModel.UserName = user?.UserName;
            }

            return View(viewModel);
        }

        // GET: Admin/Review/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var review = await _reviewService.GetReviewByIdAsync(id);
            if (review == null) return NotFound();

            var viewModel = _mapper.Map<ReviewUpdateViewModel>(review);

            var product = await _productService.GetByIdAsync(review.ProductId);
            ViewBag.ProductName = product?.Name;

            var user = await _userManager.FindByIdAsync(review.AppUserId);
            ViewBag.UserName = user?.UserName;

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ReviewUpdateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var review = await _reviewService.GetReviewByIdAsync(model.Id);
                if (review != null)
                {
                    var product = await _productService.GetByIdAsync(review.ProductId);
                    ViewBag.ProductName = product?.Name;
                    var user = await _userManager.FindByIdAsync(review.AppUserId);
                    ViewBag.UserName = user?.UserName;
                }
                return View(model);
            }

            var existingReview = await _reviewService.GetReviewByIdAsync(model.Id);
            if (existingReview == null) return NotFound();

            existingReview.Rating = model.Rating;
            existingReview.Comment = model.Comment;
            existingReview.UpdatedAt = DateTime.Now;

            await _reviewService.UpdateReviewAsync(existingReview);
            await UpdateProductRating(existingReview.ProductId);

            TempData["Success"] = "Review updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var review = await _reviewService.GetReviewByIdAsync(id);
            if (review == null)
            {
                TempData["Error"] = "Review not found.";
                return RedirectToAction(nameof(Index));
            }

            int productId = review.ProductId;
            await _reviewService.DeleteReviewAsync(id);
            await UpdateProductRating(productId);

            TempData["Success"] = "Review deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkDelete(List<int> ids)
        {
            if (ids == null || !ids.Any())
            {
                TempData["Error"] = "No reviews selected.";
                return RedirectToAction(nameof(Index));
            }

            var deletedCount = 0;
            var productIdsToUpdate = new HashSet<int>();

            foreach (var id in ids)
            {
                var review = await _reviewService.GetReviewByIdAsync(id);
                if (review != null)
                {
                    productIdsToUpdate.Add(review.ProductId);
                    await _reviewService.DeleteReviewAsync(id);
                    deletedCount++;
                }
            }

            foreach (var productId in productIdsToUpdate)
            {
                await UpdateProductRating(productId);
            }

            TempData["Success"] = $"{deletedCount} review(s) deleted successfully.";
            return RedirectToAction(nameof(Index));
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
}