using WIRM.API.Models;
using WIRM.API.Models.Request;
using WIRM.API.Models.Response;

namespace WIRM.API.Interface
{
    public interface IOPRService
    {
        Task<PaginatedResponse<WorkItemPreviewDto>> GetWorkItemsAsync(PaginationRequest request);
        Task<WorkItemSearchDto?> GetWorkItemDetailsAsync(string id);

        Task<WorkItemCreateResponseDto> CreateWorkItem(TicketForm ticketForm, IEnumerable<IFormFile> attachments);
    }
}
