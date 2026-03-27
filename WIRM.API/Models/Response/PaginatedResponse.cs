namespace WIRM.API.Models.Response
{
    public class PaginatedResponse<T>
    {
        public IEnumerable<T> Data { get; set; }
        public int TotalCount { get; set; } 
        public int CurrentPage {  get; set; }
        public int PageSize { get; set; }
        public int PageCount => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasNextPage => CurrentPage < PageCount;
        public bool HasPreviousPage => CurrentPage > 1; 
    }
}
