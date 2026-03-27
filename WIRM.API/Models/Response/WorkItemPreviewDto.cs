namespace WIRM.API.Models.Response
{
    public class WorkItemPreviewDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? State { get; set; }
        public string? AssignedTo { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastUpdate { get; set; }
        public string? TeamProject { get; set; }
    }
}
