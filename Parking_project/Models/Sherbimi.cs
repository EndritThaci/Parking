using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parking_project.Models
{
    public class Sherbimi
    {
        [Key]
        public int SherbimiId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Emri { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Cmimi { get; set; } = decimal.Zero;

        public bool active { get; set; } = true;

        [Required]
        public int BiznesId { get; set; }

        [ForeignKey(nameof(BiznesId))]
        public Organizata Organizata { get; set; }
    }
}
