using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parking_project.Models
{
    public class UserOrg
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int BiznesId { get; set; }
        public int? NjesiaId { get; set; }


        [ForeignKey(nameof(UserId))]
        public Useri User { get; set; } = null!;

        [ForeignKey(nameof(BiznesId))]
        public Organizata Organizata { get; set; } = null!;

        [ForeignKey(nameof(NjesiaId))]
        public NjesiOrg? Njesi { get; set; }
    }
}
