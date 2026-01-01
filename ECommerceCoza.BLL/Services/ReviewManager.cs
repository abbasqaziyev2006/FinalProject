using EcommerceCoza.BLL.Services.Contracts;
public class ReviewManager : IReviewService
{
    private readonly IReviewRepository _reviewRepo;
    public ReviewManager(IReviewRepository reviewRepo) => _reviewRepo = reviewRepo;

    public async Task<Review?> GetReviewByIdAsync(int id) => await _reviewRepo.GetByIdAsync(id);

    public async Task<IEnumerable<Review>> GetReviewsByProductIdAsync(int productId) => await _reviewRepo.GetReviewsByProductIdAsync(productId);

    public async Task<IEnumerable<Review>> GetReviewsByUserIdAsync(string userId) => await _reviewRepo.GetReviewsByUserIdAsync(userId);

    public async Task<Review?> GetReviewByUserAndProductAsync(string userId, int productId) => await _reviewRepo.GetReviewByUserAndProductAsync(userId, productId);

    public async Task CreateReviewAsync(Review review) => await _reviewRepo.CreateAsync(review);

    public async Task UpdateReviewAsync(Review review) => await _reviewRepo.UpdateAsync(review);

    public async Task DeleteReviewAsync(int reviewId)
    {
        var review = await _reviewRepo.GetByIdAsync(reviewId);
        if (review != null) await _reviewRepo.DeleteAsync(review);
    }

    public async Task<bool> UserHasReviewedProductAsync(string userId, int productId) =>
        await _reviewRepo.GetReviewByUserAndProductAsync(userId, productId) != null;

    public async Task<double> GetAverageRatingByProductIdAsync(int productId) => await _reviewRepo.GetAverageRatingByProductIdAsync(productId);

    public async Task<int> GetReviewCountByProductIdAsync(int productId) => await _reviewRepo.GetReviewCountByProductIdAsync(productId);
}