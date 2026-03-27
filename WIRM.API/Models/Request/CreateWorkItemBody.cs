namespace WIRM.API.Models.Request
{
    public class CreateWorkItemBody
    {
        public string? OP { get; set; }
        public string? Path { get; set; }
        public string? From { get; set; }
        public object? Value { get; set; }
    }
}
