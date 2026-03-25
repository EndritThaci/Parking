using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parking_web.Models
{
    public class Vendi
    {
        [Key]
        public int VendiId { get; set; }

        [Required]
        public string VendiEmri { get; set; }

        public bool IsFree { get; set; }

        public bool active { get; set; } = true;

        public int LokacioniId { get; set; }

        [ForeignKey(nameof(LokacioniId))]
        public Lokacioni Lokacioni { get; set; }
    }
}