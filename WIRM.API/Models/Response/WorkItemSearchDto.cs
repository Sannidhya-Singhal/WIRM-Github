namespace WIRM.API.Models.Response
{
    public class WorkItemSearchDto
    {
        public int Id { get; set; }
        public string? TeamProject { get; set; }
        public string? BusinessDriver { get; set; }
        public string? MonthlyUSDRevenue { get; set; }
        public string? CustomerName { get; set; }
        public string? OtherClientsAffected { get; set; }
        public string? RegionVertical { get; set; }
        public string? ReportingCustomer { get; set; }
        public string? ProductName { get; set; }
    }
}
