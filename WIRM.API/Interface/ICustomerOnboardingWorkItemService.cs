using WIRM.API.Controllers.CustomerOnboarding;
using WIRM.API.Models.Request;
using WIRM.API.Models.Response;

namespace WIRM.API.Interface
{
    public interface ICustomerOnboardingWorkItemService
    {
        Task<WorkItemCreateResponseDto> CreateWorkItem(CustomerOnboardingFormDto customerOnboardingFormDto, IEnumerable<string> attachmentUrls, HttpClient httpClient, string accessToken);
    }
}


