using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parking_web.Models.DTO
{
    public class DetajetUpdateDto
    {
        public int FromHour { get; set; }
        public int? ToHour { get; set; }
        public decimal Cmimi { get; set; }
        //public int CilsimetiId { get; set; }
    }
}
