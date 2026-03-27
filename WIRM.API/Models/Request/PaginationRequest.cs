namespace WIRM.API.Models.Request
{
    public class PaginationRequest
    {
        public int CurrentPage { get; set; } = 1;   
        public int PageSize { get; set; }
        public string? SearchQuery { get; set; }  
        public string? SortBy { get; set; }
        public string? SortOrder { get; set; }
    }
}
