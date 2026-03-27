namespace WIRM.API.Models
{
    public class WiqlResult
    {
        public IEnumerable<WorkItemReference> WorkItems { get; set; } = new List<WorkItemReference>();
    }

    public class WorkItemReference
    {
        public int Id { get; set; }
    }

    public class WorkItemsResult
    {
        public IEnumerable<WorkItemDetail> Value { get; set; } = new List<WorkItemDetail>();
    }

    public class WorkItemDetail
    {
        public int Id { get; set; }
        public Dictionary<string, object>? Fields { get; set; }
    }
}
