using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parking_web.Models.DTO
{
    public class TransaksionetCreateDto
    {
        [Required]
        public int NjesiaId { get; set; }

        [Required]
        public int CilsimiId { get; set; }

    }
}
