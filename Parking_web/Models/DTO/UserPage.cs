namespace Parking_web.Models.DTO
{
    public class UserPage
    {
        public List<Useri> Data { get; set; } = new();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
    }
}
