namespace EcommerceCoza.BLL.ViewModels
{
    public class ReviewViewModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? AppUserId { get; set; }
        public string? UserName { get; set; }
        public string? Comment { get; set; }
        public double Rating { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class ReviewCreateViewModel
    {
        public int ProductId { get; set; }
        public string? Comment { get; set; }
        public double Rating { get; set; }
    }


    public class ReviewUpdateViewModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? Comment { get; set; }
        public double Rating { get; set; }
    }
}