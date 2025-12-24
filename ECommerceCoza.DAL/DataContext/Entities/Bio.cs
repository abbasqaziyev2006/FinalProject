namespace ECommerceCoza.DAL.DataContext.Entities
{
    public class Bio : TimeStample
    {
        public string Address { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string LocationUrl { get; set; } = null!;
    }

 
}
