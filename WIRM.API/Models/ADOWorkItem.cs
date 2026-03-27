using System.Text.Json.Serialization;

namespace WIRM.API.Models
{
    public class ADOWorkItem
    {
        public int Id { get; set; }
        public Dictionary<string, object> Fields { get; set; }
    }
}
