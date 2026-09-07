using Parking_project.Models.DTO;
using System.ComponentModel.DataAnnotations;


namespace Parking_project.Models
{
    public class Useri
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string Emri { get; set; }

        [Required]
        [StringLength(100)]
        public string Mbiemri { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; }

        [Required]
        [StringLength(255)]
        public string Passwordi { get; set; }

        
        [MaxLength(50)]
        public string Role { get; set; } = "Customer";

        public bool active { get; set; } = true;

        public List<UserOrg> UserOrgs { get; set; } = new();
    }
}
