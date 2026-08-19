namespace Parking_project.Models.DTO
{
    public class CreditCardReadDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Brand { get; set; }
        public string Last4 { get; set; }
        public int ExpMonth { get; set; }
        public int ExpYear { get; set; }
    }
}
