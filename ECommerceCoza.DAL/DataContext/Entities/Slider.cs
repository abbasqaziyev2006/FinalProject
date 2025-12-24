namespace ECommerceCoza.DAL.DataContext.Entities
{
    public class Slider : TimeStample
    {
        public string Title { get; set; } = null!;
        public string Line1 { get; set; } = null!; 
        public string Line2 { get; set; } = null!; 
        public string ImageName { get; set; } = null!;
        public string? Link { get; set; } 
        public string? CategoryIcon { get; set; } 
        public bool IsActive { get; set; } = true;
    }
}


