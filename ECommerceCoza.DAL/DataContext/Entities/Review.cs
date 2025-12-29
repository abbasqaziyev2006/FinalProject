using ECommerceCoza.DAL.DataContext.Entities;

public class Review : TimeStample
{
    public string? Comment { get; set; }
    public double Rating { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;


    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public string? AppUserId { get; set; }
    public AppUser? AppUser { get; set; }
}