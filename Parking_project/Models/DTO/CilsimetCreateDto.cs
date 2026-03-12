using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parking_project.Models.DTO
{
    public class CilsimetCreateDto
    {

        [Required]
        public string Emri { get; set; }

        [Required]
        public int NjesiteId { get; set; }

        [Required]
        public int SherbimiId { get; set; }
    }
}
