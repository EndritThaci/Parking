namespace Parking_web.Models.DTO
{
    public class LoginResponseDTO
    {
        public string? Token { get; set; }
        public UserReadDTO? UserReadDTO { get; set; }
    }
}
