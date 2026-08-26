namespace Parking_project.Models.DTO
{
    public class TransaksionPage
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal MonthlyAmount { get; set; }
        public decimal YearlyAmount { get; set; }
        public List<NjesiOrg> Njesite { get; set; }
        public List<TransaksionRead> Data { get; set; }
    }
}
