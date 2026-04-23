using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parking_project.Models.DTO
{
    public class TransaksionRead
    {

        public int TransaksioniId { get; set; }

        public decimal? Cmimi { get; set; }

        public DateTime KohaHyrjes { get; set; }

        public DateTime? KohaDaljes { get; set; }

        public string Statusi { get; set; }

        public NjesiOrg Njesia{ get; set; }
        public CilsimetParkimit Cilsimi { get; set; }
        public List<Sherbimi>? Sherbimi { get; set; }
        public Useri Useri { get; set; }

    }
}
