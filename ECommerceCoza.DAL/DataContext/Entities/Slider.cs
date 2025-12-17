namespace ECommerceCoza.DAL.DataContext.Entities
{
    public class Slider : TimeStample
    {
        public string Title { get; set; } = null!;
        public string Line1 { get; set; } = null!; // Tagline
        public string Line2 { get; set; } = null!; // Subtitle
        public string ImageName { get; set; } = null!;
        public string? Link { get; set; } // URL
        public string? CategoryIcon { get; set; } // Optional category icon
        public bool IsActive { get; set; } = true;
    }
}


