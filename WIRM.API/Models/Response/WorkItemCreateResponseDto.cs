namespace WIRM.API.Models.Response
{
    public class WorkItemCreateResponseDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public int Weighting { get; set; } = 1;
        public string? TeamProject { get; set; } = string.Empty;
    }
}
