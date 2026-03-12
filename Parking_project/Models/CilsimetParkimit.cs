using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parking_project.Models
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

        public bool Active { get; set; } = false;

        [ForeignKey(nameof(SherbimiId))]
        public Sherbimi Sherbimi { get; set; }

        [ForeignKey(nameof(NjesiteId))]
        public NjesiOrg NjesiOrg { get; set; }
    }
}
