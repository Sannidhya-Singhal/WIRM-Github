using WIRM.API.Controllers.CustomerOnboarding;
using WIRM.API.Models.Request;
using WIRM.API.Models.Response;

namespace WIRM.API.Interface
{
    public interface ISEService
    {
        Task<WorkItemCreateResponseDto> CreateWorkItem(TicketForm ticketForm, IEnumerable<IFormFile> attachments);
        Task<WorkItemCreateResponseDto> CreateCustomerOnboardingWorkItem(CustomerOnboardingFormDto customerOnboardingFormDto, IFormFile attachment);
    }
}
