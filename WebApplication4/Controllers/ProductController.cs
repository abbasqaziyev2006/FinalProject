using EcommerceCoza.BLL.Services.Contracts;
using EcommerceCoza.BLL.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace EcommerceCoza.MVC.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly IReviewService _reviewService;
        private readonly IMapper _mapper;

        public ProductController(IProductService productService, IReviewService reviewService, IMapper mapper)
        {
            _productService = productService;
            _reviewService = reviewService;
            _mapper = mapper;
        }

        public IActionResult Index()
        {
            return View();
        }


        public async Task<IActionResult> Details(string id)
        {
            int productId = int.Parse(id.Split('-').Last());

            var model = await _productService.GetAsync(predicate: x => x.Id == productId && !x.IsDeleted
            , include: x => x
                .Include(c => c.Category)
                .Include(pv => pv.ProductVariants)
                .ThenInclude(i => i.ProductImages)
                .Include(pv => pv.ProductVariants)
                .ThenInclude(pc => pc.Color!)
                .Include(p => p.Reviews)
                .ThenInclude(r => r.AppUser)
                );

            if (model == null) return NotFound();

           
            var reviewEntities = await _reviewService.GetReviewsByProductIdAsync(productId);
            model.Reviews = _mapper.Map<List<ReviewViewModel>>(reviewEntities);
            model.ReviewCount = await _reviewService.GetReviewCountByProductIdAsync(productId);
            model.Rating = await _reviewService.GetAverageRatingByProductIdAsync(productId);

            return View(model);
        }
    }
}