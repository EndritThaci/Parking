using System.ComponentModel.DataAnnotations;

namespace Parking_web.Models.DTO
{
    public class LokacioniUpdateDTO
    {
        [Required]
        public int LokacioniId { get; set; }

        [Required]
        public int Kati { get; set; }

        [Required]
        public int NjesiteId { get; set; }
    }
}
