namespace Parking_project.Models.DTO
{
    public class CilsimetWithDetailsUpdateDTO
    {
        public string Emri { get; set; } = string.Empty;
        public int NjesiteId { get; set; }
        public int SherbimiId { get; set; }

        public List<DetajetUpdateDto> Detajet { get; set; } = new();
    }
}
