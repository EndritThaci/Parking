using System.ComponentModel.DataAnnotations;

namespace Parking_web.Models.DTO
{
    public class UserCreateDTO
    {
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

        
        public int? BiznesId { get; set; }
        public int? NjesiaId { get; set; }
    }
}
