namespace Parking_web.Models.DTO
{
    public class OrgPage
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
        public List<OrgDTO> Data { get; set; }
    }
}
