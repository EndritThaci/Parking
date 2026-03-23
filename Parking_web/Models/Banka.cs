using System.ComponentModel.DataAnnotations;

namespace Parking_web.Models
{
    public class Banka
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

    }
}
