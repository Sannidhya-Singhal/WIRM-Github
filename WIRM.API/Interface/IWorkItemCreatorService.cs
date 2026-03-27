using WIRM.API.Models.Request;
using WIRM.API.Models.Response;

namespace WIRM.API.Interface
{
    public interface IWorkItemCreatorService 
    {
        Task<WorkItemCreateResponseDto> CreateWorkItem(TicketForm ticketForm, IEnumerable<string> attachmentUrls, HttpClient httpClient, string accessToken);
    }
}
