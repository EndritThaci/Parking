using System.ComponentModel.DataAnnotations;

namespace Parking_project.Models.DTO
{
    public class TransaksionetCreateDto
    {
        [Required]
        public int NjesiaId { get; set; }

        [Required]
        public int CilsimiId { get; set; }

        public int? UserId { get; set; }
    }
}
