using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parking_project.Models.DTO
{
    public class NjesiOrgDto
    {
        [Required]
        [StringLength(150)]
        public string Emri { get; set; }

        [Required]
        [StringLength(50)]
        public string Kodi { get; set; }

        [Required]
        [StringLength(200)]
        public string Adresa { get; set; }

        public int VendeTeLira { get; set; }

        [Required]
        public int BiznesId { get; set; }
    }
}
