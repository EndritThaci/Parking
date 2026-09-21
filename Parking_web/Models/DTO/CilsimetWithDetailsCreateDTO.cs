using System.ComponentModel.DataAnnotations;

namespace Parking_web.Models.DTO
{
    public class CilsimetWithDetailsCreateDTO
    {
        public string Emri { get; set; }
        public int NjesiteId { get; set; }
        public int SherbimiId { get; set; }
        public List<DetajetCreateDto> Detajet { get; set; } = new List<DetajetCreateDto>();
    }
}
