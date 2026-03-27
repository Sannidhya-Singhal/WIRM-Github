using WIRM.API.Controllers.CustomerOnboarding;
using WIRM.API.Models.Response;

namespace WIRM.API.Interface
{
    public interface IArrayCustomerOnBoardingEmailService
    {
        Task SendEmailAsync(CustomerOnboardingFormDto dto, WorkItemCreateResponseDto workItemCreateResponseDto);
    }
}
