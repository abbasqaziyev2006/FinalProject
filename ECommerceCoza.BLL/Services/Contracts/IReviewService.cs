using ECommerceCoza.DAL.DataContext.Entities;

namespace EcommerceCoza.BLL.Services.Contracts
{
    public interface IReviewService
    {
        Task<IEnumerable<Review>> GetReviewsByProductIdAsync(int productId);
        Task<Review?> GetReviewByIdAsync(int id);
        Task CreateReviewAsync(Review review);
        Task DeleteReviewAsync(int reviewId);
        Task<bool> UserHasReviewedProductAsync(string userId, int productId);
        Task<double> GetAverageRatingByProductIdAsync(int productId);
        Task<int> GetReviewCountByProductIdAsync(int productId);
    }
}