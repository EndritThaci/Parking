using System.ComponentModel.DataAnnotations;

namespace Parking_web.Models.DTO
{
    public class VendiCreateDTO
    {
        [Required]
        public string VendiEmri { get; set; }

        [Required]
        public int LokacioniId { get; set; }
    }
}
