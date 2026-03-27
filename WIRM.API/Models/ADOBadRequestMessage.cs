namespace WIRM.API.Models
{
    public class ADOBadRequestMessage
    {
        public string Id { get; set; }
        public int ErrorCode { get; set; }
        public int EventId { get; set; }
        public string Message { get; set; }
        public string TypeKey { get; set; }
        public string TypeName { get; set; }
    }
}
