namespace Parking_project.Models.DTO
{
    public class UserReadDTO
    {
        public int UserId { get; set; }
        public string Emri { get; set; }
        public string Mbiemri { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public int BiznesId { get; set; }
    }
}
