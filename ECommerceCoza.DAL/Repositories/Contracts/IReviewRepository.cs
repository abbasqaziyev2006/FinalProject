using EcommerceCoza.DAL.DataContext.Repositories.Contracts;
using ECommerceCoza.DAL.DataContext.Entities;

public interface IReviewRepository : IRepository<Review>
{
    Task<IEnumerable<Review>> GetReviewsByProductIdAsync(int productId);
    Task<IEnumerable<Review>> GetReviewsByUserIdAsync(string userId);
    Task<Review?> GetReviewByUserAndProductAsync(string userId, int productId);
    Task<double> GetAverageRatingByProductIdAsync(int productId);
    Task<int> GetReviewCountByProductIdAsync(int productId);
}