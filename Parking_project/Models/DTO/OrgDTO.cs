using System.ComponentModel.DataAnnotations;

namespace Parking_project.Models.DTO
{
    public class OrgDTO
    {

        [Key]
        public int BiznesId { get; set; }

        [Required]
        [StringLength(200)]
        public string EmriBiznesit { get; set; }

        [Required]
        [StringLength(50)]
        public string NumriUnikIdentifikues { get; set; }

        [Required]
        [StringLength(200)]
        public string Adresa { get; set; }

        [Required]
        [StringLength(50)]
        public string NumriBiznesit { get; set; }

        [Required]
        [StringLength(50)]
        public string NumriFiskal { get; set; }

        public int NumriPunetoreve { get; set; }

        [Required]
        public DateTime DataRegjistrimit { get; set; }

        [Required]
        [StringLength(100)]
        public string Komuna { get; set; }

        [Phone]
        [StringLength(30)]
        public string Telefoni { get; set; }

        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; }
    }
}
