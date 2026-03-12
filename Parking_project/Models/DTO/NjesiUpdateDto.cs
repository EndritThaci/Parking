using System.ComponentModel.DataAnnotations;

namespace Parking_project.Models.DTO
{
    public class NjesiUpdateDto
    {
        [Key]
        public int NjesiteId { get; set; }
        [Required]
        [StringLength(150)]
        public string Emri { get; set; }

        [Required]
        [StringLength(50)]
        public string Kodi { get; set; }

        [Required]
        [StringLength(200)]
        public string Adresa { get; set; }

        [Required]
        public int BiznesId { get; set; }
    }
}
