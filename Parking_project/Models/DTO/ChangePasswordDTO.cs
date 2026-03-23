namespace Parking_project.Models.DTO
{
    public class ChangePasswordDTO
    {
        public required string OldPassword { get; set; }
        public required string NewPassword { get; set; }
        public required string ConfirmPassword { get; set; }
    }
}
