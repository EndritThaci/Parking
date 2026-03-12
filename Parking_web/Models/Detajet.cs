using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parking_web.Models
{
    public class Detajet
    {
        [Key]
        public int DetajetId { get; set; }

        public int FromHour { get; set; }

        public int? ToHour { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Cmimi { get; set; }

        [Required]
        public int CilsimetiId { get; set; }

        [ForeignKey(nameof(CilsimetiId))]
        public CilsimetParkimit CilsimetParkimit { get; set; }
    }
}
