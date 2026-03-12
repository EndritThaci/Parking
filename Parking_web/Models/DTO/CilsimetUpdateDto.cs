using System.ComponentModel.DataAnnotations;

namespace Parking_web.Models.DTO
{
    public class CilsimetUpdateDto
    {
        [Key]
        public int CilsimetiId { get; set; }

        [Required]
        public string Emri { get; set; }

        [Required]
        public int NjesiteId { get; set; }
        
        [Required]
        public int SherbimiId { get; set; }
    }
}
