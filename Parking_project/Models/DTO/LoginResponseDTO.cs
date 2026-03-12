namespace Parking_project.Models.DTO
{
    public class LoginResponseDTO
    {
        public string? Token { get; set; }
        public UserReadDTO? UserReadDTO { get; set; }
    }
}
