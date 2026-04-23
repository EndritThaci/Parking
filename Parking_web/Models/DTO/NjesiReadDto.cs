using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parking_web.Models.DTO
{
    public class NjesiReadDto
    {
        public int NjesiteId { get; set; }

        public string Emri { get; set; }

        public string Kodi { get; set; }

        public string Adresa { get; set; }

        public int VendeTeLira { get; set; }

        public int BiznesId { get; set; }

        public Organizata Organizata { get; set; }
    }
}
