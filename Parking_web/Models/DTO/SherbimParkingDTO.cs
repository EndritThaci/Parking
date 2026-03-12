using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parking_web.Models.DTO
{
    public class SherbimParkingDTO
    {
        public int BiznesId { get; set; }

        public string EmriCilsimit { get; set; }

        public int NjesiteId { get; set; }

        public int FromHour { get; set; }

        public int? ToHour { get; set; }

        public decimal Cmimi { get; set; }

    }
}
