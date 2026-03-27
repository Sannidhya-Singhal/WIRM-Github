using WIRM.API.Models.Request;
using WIRM.API.Models.Response;

namespace WIRM.API.Interface
{
    public interface ITicketEmailService
    {
        Task SendEmailAsync(TicketForm ticketForm, WorkItemCreateResponseDto workItemCreateResponseDto);
    }
}
