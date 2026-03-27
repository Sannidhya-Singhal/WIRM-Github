namespace WIRM.API.Models
{
    public class ADORequestException : Exception
    {
        public string? ResponseContent { get; set; }

        public int? GetMentionedWorkitemId(string message)
        {
            string key = "work item";
            message = message.ToLower();
            if (message.Contains(key))
            {
                var workItemId = message.Substring(message.IndexOf(key) + key.Length + 1).Split(' ').First();
                if (int.TryParse(workItemId, out int value))
                    return value;
            }
            return null;
        }
    }
}
