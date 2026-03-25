using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parking_project.Models.DTO
{
    public class CilsimetReadDto
    {
        public int CilsimetiId { get; set; }
        public string Emri { get; set; }
        public int NjesiteId { get; set; }
        public int SherbimiId { get; set; }
        public bool Selected { get; set; }
        public NjesiOrg NjesiOrg { get; set; }
        public Sherbimi Sherbimi { get; set; }
    }
}
