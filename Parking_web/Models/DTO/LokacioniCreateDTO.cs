using System.ComponentModel.DataAnnotations;

namespace Parking_web.Models.DTO
{
    public class LokacioniCreateDTO
    {
        [Required]
        public int Kati { get; set; }

        [Required]
        public int NjesiteId { get; set; }
    }
}
