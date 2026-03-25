using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parking_web.Models
{
    public class CilsimetParkimit
    {
        [Key]
        public int CilsimetiId { get; set; }

        [Required]
        public string Emri { get; set; }
        
        [Required]
        public int NjesiteId { get; set; }
        
        [Required]
        public int SherbimiId { get; set; }

        public bool Selected { get; set; } = false;
        public bool active { get; set; } = true;

        [ForeignKey(nameof(SherbimiId))]
        public Sherbimi Sherbimi { get; set; }

        [ForeignKey(nameof(NjesiteId))]
        public NjesiOrg NjesiOrg { get; set; }
    }
}
