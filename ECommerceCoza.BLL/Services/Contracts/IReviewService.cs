using ECommerceCoza.DAL.DataContext.Entities;

namespace EcommerceCoza.BLL.Services.Contracts
{
    public interface IReviewService
    {
        Task<IEnumerable<Review>> GetReviewsByProductIdAsync(int productId);
        Task<IEnumerable<Review>> GetReviewsByUserIdAsync(string userId);
        Task<Review?> GetReviewByIdAsync(int id);
        Task<Review?> GetReviewByUserAndProductAsync(string userId, int productId);
        Task CreateReviewAsync(Review review);
        Task UpdateReviewAsync(Review review);
        Task DeleteReviewAsync(int reviewId);
        Task<bool> UserHasReviewedProductAsync(string userId, int productId);
        Task<double> GetAverageRatingByProductIdAsync(int productId);
        Task<int> GetReviewCountByProductIdAsync(int productId);
    }
}