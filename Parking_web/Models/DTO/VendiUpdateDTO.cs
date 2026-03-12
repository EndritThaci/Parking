using System.ComponentModel.DataAnnotations;

namespace Parking_web.Models.DTO
{
    public class VendiUpdateDTO
    {
        [Required]
        public int VendiId { get; set; }

        [Required]
        public string VendiEmri { get; set; }

        [Required]
        public int LokacioniId { get; set; }
    }
}
