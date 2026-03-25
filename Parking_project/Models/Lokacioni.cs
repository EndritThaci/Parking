using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parking_project.Models
{
    public class Lokacioni
    {
        [Key]
        public int LokacioniId { get; set; }

        [Required]
        public int Kati { get; set; }

        public bool active { get; set; } = true;

        [Required]
        public int NjesiteId { get; set; }

        [ForeignKey(nameof(NjesiteId))]
        public NjesiOrg NjesiOrg { get; set; }
    }
}
