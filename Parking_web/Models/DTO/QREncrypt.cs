namespace Parking_web.Models.DTO
{
    public class QREncrypt
    {
        public int? ID { get; set; }
        public string? Timestamp { get; set; }
        public int? CardID { get; set; }
        public int? NjesiaID { get; set; }
        public int? UserID { get; set; }
        public string? Identifikues { get; set; }
        public string? Signature { get; set; }
        public string? Type { get; set; }
    }
}
