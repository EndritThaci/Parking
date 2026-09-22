namespace Parking_web.Models.DTO
{
    public class CilsimetWithDetailsUpdateDTO
    {
        public int CilsimetiId { get; set; }
        public string Emri { get; set; } = string.Empty;
        public int NjesiteId { get; set; }
        public int SherbimiId { get; set; }

        public List<DetajetUpdateDto> Detajet { get; set; } = new();
    }
}
