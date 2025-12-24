namespace ECommerceCoza.DAL.DataContext.Entities
{

    public class TimeStample : Entity
    {
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
    }
 
}
