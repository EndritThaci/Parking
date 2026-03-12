using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parking_project.Models.DTO
{
    public class DetajetCreateDto
    {
        public int FromHour { get; set; }

        public int? ToHour { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Cmimi { get; set; }

        [Required]
        public int CilsimetiId { get; set; }
    }
}
