using System.ComponentModel.DataAnnotations;

namespace Parking_project.Models.DTO
{
    public class UserUpdateDTO
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string Emri { get; set; }

        [Required]
        [StringLength(100)]
        public string Mbiemri { get; set; }

        [Required]
        [StringLength(150)]
        public string Username { get; set; }

        public List<UserOrgCreateDto>? UserOrgs { get; set; }
    }
}
