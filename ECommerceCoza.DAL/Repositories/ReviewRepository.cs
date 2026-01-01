using EcommerceCoza.DAL.DataContext.Repositories;
using ECommerceCoza.DAL.DataContext;
using ECommerceCoza.DAL.DataContext.Entities;
using Microsoft.EntityFrameworkCore;

public class ReviewRepository : EFCoreRepository<Review>, IReviewRepository
{
    private readonly AppDbContext _context;

    public ReviewRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Review>> GetReviewsByProductIdAsync(int productId)
    {
        return await _context.Set<Review>()
            .Include(r => r.AppUser)
            .Where(r => r.ProductId == productId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Review>> GetReviewsByUserIdAsync(string userId)
    {
        return await _context.Set<Review>()
            .Include(r => r.Product)
            .Include(r => r.AppUser)
            .Where(r => r.AppUserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<Review?> GetReviewByUserAndProductAsync(string userId, int productId)
    {
        return await _context.Set<Review>()
            .FirstOrDefaultAsync(r => r.AppUserId == userId && r.ProductId == productId);
    }

    public async Task<double> GetAverageRatingByProductIdAsync(int productId)
    {
        var ratings = await _context.Set<Review>()
            .Where(r => r.ProductId == productId)
            .Select(r => r.Rating)
            .ToListAsync();
        return ratings.Any() ? ratings.Average() : 0;
    }

    public async Task<int> GetReviewCountByProductIdAsync(int productId)
    {
        return await _context.Set<Review>()
            .CountAsync(r => r.ProductId == productId);
    }
}