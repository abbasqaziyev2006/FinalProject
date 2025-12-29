using EcommerceCoza.DAL.DataContext.Repositories.Contracts;

public interface IReviewRepository : IRepository<Review>
{
    Task<IEnumerable<Review>> GetReviewsByProductIdAsync(int productId);
    Task<Review?> GetReviewByUserAndProductAsync(string userId, int productId);
    Task<double> GetAverageRatingByProductIdAsync(int productId);
    Task<int> GetReviewCountByProductIdAsync(int productId);
}