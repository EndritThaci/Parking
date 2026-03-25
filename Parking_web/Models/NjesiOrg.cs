using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parking_web.Models
{
    public class NjesiOrg
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

        public bool active { get; set; } = true;

        [Required]
        public int BiznesId { get; set; }

        [ForeignKey(nameof(BiznesId))]
        public Organizata Organizata { get; set; }
    }
}
