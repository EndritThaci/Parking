using System.ComponentModel.DataAnnotations;

namespace Parking_project.Models.DTO
{
    public class VendiUpdateDTO
    {
        [Required]
        public int VendiId { get; set; }

        [Required]
        public string VendiEmri { get; set; }

        public bool IsFree { get; set; }

        [Required]
        public int LokacioniId { get; set; }
    }
}
