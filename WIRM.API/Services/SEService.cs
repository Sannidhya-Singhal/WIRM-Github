using System.Diagnostics.Eventing.Reader;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using WIRM.API.Controllers.CustomerOnboarding;
using WIRM.API.Extensions;
using WIRM.API.Interface;
using WIRM.API.Models;
using WIRM.API.Models.Request;
using WIRM.API.Models.Response;

namespace WIRM.API.Services
{
    public class SEService : ISEService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthService _authService;
        private readonly IAttachmentUploader _attachmentUploader;
        private readonly IWorkItemCreatorService _workItemCreatorService;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ICustomerOnboardingWorkItemService _customerOnboardingWorkItemService;

        public SEService(HttpClient httpClient, AuthService authService, IAttachmentUploader attachmentUploader,
            IHttpContextAccessor contextAccessor, IWorkItemCreatorService workItemCreatorService, ICustomerOnboardingWorkItemService customerOnboardingWorkItemService)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://liox-teams.visualstudio.com/Solutions%20Engineering/");
            _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            _authService = authService;
            _attachmentUploader = attachmentUploader;
            _contextAccessor = contextAccessor;
            _workItemCreatorService = workItemCreatorService;
            _customerOnboardingWorkItemService = customerOnboardingWorkItemService;
        }

        public async Task<WorkItemCreateResponseDto> CreateWorkItem(TicketForm ticketForm, IEnumerable<IFormFile> attachments)
        {
            var accessToken = await _authService.GetAccessToken(_contextAccessor);
            var attachmentUrls = await _attachmentUploader.UploadAttachmentsAsync(_httpClient, accessToken, attachments);
            ticketForm.Attachments = attachmentUrls;
            return await _workItemCreatorService.CreateWorkItem(ticketForm, attachmentUrls, _httpClient, accessToken);
        }

        public async Task<WorkItemCreateResponseDto> CreateCustomerOnboardingWorkItem(CustomerOnboardingFormDto customerOnboardingFormDto, IFormFile attachment)
        {
            List<IFormFile> formFiles = new List<IFormFile>();
            if (attachment != null)
            {
                formFiles.Add(attachment);
            }
            
            var accessToken = await _authService.GetAccessToken(_contextAccessor);
            var attachmentUrls = await _attachmentUploader.UploadAttachmentsAsync(_httpClient, accessToken, formFiles);
            return await _customerOnboardingWorkItemService.CreateWorkItem(customerOnboardingFormDto, attachmentUrls, _httpClient, accessToken);
        }
    }
}

