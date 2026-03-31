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
            _httpClient.BaseAddress = new Uri("https://liox-teams.visualstudio.com/Aurora%20CX/");
            var accessToken = await _authService.GetAccessToken(_contextAccessor);
            accessToken = "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6IlFaZ045SHFOa0dORU00R2VLY3pEMDJQY1Z2NCIsImtpZCI6IlFaZ045SHFOa0dORU00R2VLY3pEMDJQY1Z2NCJ9.eyJhdWQiOiJhcGk6Ly9hYmJiZDBmNi1kYWQ5LTQ5ZWQtOGJmMS0zMzhiOWE4NzAxNDEiLCJpc3MiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC80MmRjOGIwZi00NzU5LTRhZmUtOTM0OC00MTk1MmVlYWY5OGIvIiwiaWF0IjoxNzc0OTMyOTg4LCJuYmYiOjE3NzQ5MzI5ODgsImV4cCI6MTc3NDkzNzUzMywiYWNyIjoiMSIsImFpbyI6IkFYUUFpLzhiQUFBQTNKdjNIWkRseU5LVCtKcFpvUTNTcExWVW10TG8xVXpKQnNvTzl0K0huRzh2ZFl4b25IZ2JvNTZBOXNMUkdBZU9CaHVlUnc2QWo0bmUweHJzd1ZNdWY1NGJ1eDBXTUI2SFR2UHI3bWRkTzRJZ1VMNmZ6bU01bVZaL09WV05tbGZpd2E3T0txN1VHTFFSa1czeWlqYURjZz09IiwiYW1yIjpbInB3ZCIsInJzYSIsIm1mYSJdLCJhcHBpZCI6IjQ4ZjQ4YjRjLTg2MDktNDg1NC05OTFlLTUyOGI2MWY3YWRiNSIsImFwcGlkYWNyIjoiMCIsImRldmljZWlkIjoiOTk5MGQ5NTctMTVhOS00YTE2LWE3YjYtZTA5ZGU4YTk3ZjFkIiwiZmFtaWx5X25hbWUiOiJOaWNoYW50ZSIsImdpdmVuX25hbWUiOiJSdXBlc2giLCJpcGFkZHIiOiIxMjUuMTguMjI3Ljk0IiwibmFtZSI6Ik5pY2hhbnRlLCBSdXBlc2ggKENvbnRyYWN0b3IpIiwib2lkIjoiOThhM2RmMjQtM2M2Ny00ZWI2LWFjZDItMWE3YzUxOWU5MTA5Iiwib25wcmVtX3NpZCI6IlMtMS01LTIxLTIzOTI3NTI1NDYtMzk1MjAyOTE3Mi0zNzQyNTE5OTc4LTEyNDUzMjgiLCJyaCI6IjEuQVRnQUQ0dmNRbGxIX2txVFNFR1ZMdXI1aV9iUXU2dloydTFKaV9Femk1cUhBVUhOQWFVNEFBLiIsInNjcCI6IlVzZXIuUmVhZCIsInNpZCI6IjAwMjI1Y2ZhLTQ0YWItZDQ0Yi1mYzI2LTE4YjQyYjFjMmM1MyIsInN1YiI6IjZBRzNvc1Rka2YxWHM2a2FlcEpYbS1QMUN0STZMUktqc0N6LXNZU2U4VnciLCJ0aWQiOiI0MmRjOGIwZi00NzU5LTRhZmUtOTM0OC00MTk1MmVlYWY5OGIiLCJ1bmlxdWVfbmFtZSI6InYtUnVwZXNoLk5pY2hhbnRlQGxpb25icmlkZ2UuY29tIiwidXBuIjoidi1SdXBlc2guTmljaGFudGVAbGlvbmJyaWRnZS5jb20iLCJ1dGkiOiIzR1JIdnd1YjdFTzEzLU53VjJkOEFBIiwidmVyIjoiMS4wIiwieG1zX2Z0ZCI6IkFrelJTem5hYU5Tam9CR2JNMWpwMGJMZURlUGlvX2FCelc5c0JKZVM4ZWdCZFhObFlYTjBMV1J6YlhNIn0.ec6qYh9PIAZJRil6jGEazeQxUYAeAsepYNQRdBo7rU_ICu3QFE-1oO3Rf3FB-lLmpKLsknMMEO_5osgEb6m7fdydf3MIqw82T1opyU2_nrG8g6hTVuo0-iLNC4IxBhADqE_QdQNXJi7pOoecwHfQkP9BCgkGEWJhS7DekBzsyqKmLlKdoSuOAr6tednuPr5wW6_Z27bEPkq66RLDV8ZS2fZJ2dJsFMBK6Ir2VvS7GJRryidEdimlGMeDWJWFg2oXPj-PRIQ0Lo6OIyzP3JELlTgHy9e1OcCgYXDNTYOGuv-wWkfi-Oan4FqGgIG0qy2U-Q8nyaRJtP0VH07HhfzjbQ";
            var attachmentUrls = await _attachmentUploader.UploadAttachmentsAsync(_httpClient, accessToken, formFiles);
            return await _customerOnboardingWorkItemService.CreateWorkItem(customerOnboardingFormDto, attachmentUrls, _httpClient, accessToken);
        }
    }
}

