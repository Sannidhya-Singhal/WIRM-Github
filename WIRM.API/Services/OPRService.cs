using Microsoft.AspNetCore.Authentication;
using System.Text;
using System.Text.Json;
using WIRM.API.Extensions;
using WIRM.API.Interface;
using WIRM.API.Models;
using WIRM.API.Models.Request;
using WIRM.API.Models.Response;

namespace WIRM.API.Services
{
    public class OPRService : IOPRService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthService _authService;
        private readonly IAttachmentUploader _attachmentUploader;
        private readonly IWorkItemCreatorService _workItemCreatorService;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly string? _adoRestApiVersion;

        public OPRService(HttpClient httpClient, AuthService authService, IAttachmentUploader attachmentUploader, IHttpContextAccessor contextAccessor, IConfiguration config, IWorkItemCreatorService workItemCreatorService)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://liox-teams.visualstudio.com/Operations%20Product%20Requests/");
            _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            _authService = authService;
            _attachmentUploader = attachmentUploader;
            _contextAccessor = contextAccessor;
            _adoRestApiVersion = config["AdoApiSettings:Version"];
            _workItemCreatorService = workItemCreatorService;
        }        

        /// <summary>
        /// Get Work Items for the assigned user. Number of fetched work items is currently maxed at 5000.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<PaginatedResponse<WorkItemPreviewDto>> GetWorkItemsAsync(PaginationRequest request)
        {
            var skip = (request.CurrentPage - 1) * request.PageSize;
            request.SearchQuery = $"[System.CreatedBy] = '{_contextAccessor.HttpContext.User.Identity.Name}' And ([System.TeamProject] = 'Operations Product Requests' OR [System.TeamProject] = 'Solutions Engineering')";
            var wiql = BuildFinalWIQL(request);

            var accessToken = await _authService.GetAccessToken(_contextAccessor);

            var endpoint = $"_apis/wit/wiql?$top=5000&{_adoRestApiVersion}";
            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, endpoint);
            requestMessage.Content = new StringContent(JsonSerializer.Serialize(new { query = wiql }), System.Text.Encoding.UTF8, "application/json");
            requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            var response = await _httpClient.SendAsync(requestMessage).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                var wiqlResult = await response.Content.ReadFromJsonAsync<WiqlResult>();
                if (wiqlResult?.WorkItems == null || !wiqlResult.WorkItems.Any())
                {
                    return new PaginatedResponse<WorkItemPreviewDto>
                    {
                        Data = new List<WorkItemPreviewDto>(),
                        TotalCount = 0,
                        CurrentPage = 1,
                        PageSize = request.PageSize
                    };
                }

                var totalCount = wiqlResult.WorkItems.Count();
                var result = await GetWorkItemDetails(wiqlResult.WorkItems.Select(i => i.Id).Skip(skip).Take(request.PageSize), accessToken);
                var workItems = result?.Value?.Select(i => new WorkItemPreviewDto
                {
                    Id = i.Id,
                    Title = i.Fields?.GetValueOrDefault("System.Title")?.ToString(),
                    State = i.Fields?.GetValueOrDefault("System.State")?.ToString(),
                    AssignedTo = i.Fields?.GetValueOrDefault("System.AssignedTo")?.ToString(),
                    CreatedDate = i.Fields?.GetValueOrDefault("System.AssignedTo")?.ToDateTime() ?? DateTime.MinValue,
                    LastUpdate = i.Fields?.GetValueOrDefault("System.ChangedDate")?.ToDateTime() ?? DateTime.MinValue,
                    TeamProject = i.Fields?.GetValueOrDefault("System.TeamProject")?.ToString(),
                }) ?? new List<WorkItemPreviewDto>();
                return new PaginatedResponse<WorkItemPreviewDto>
                {
                    Data = workItems,
                    TotalCount = totalCount,
                    CurrentPage = request.CurrentPage,
                    PageSize = request.PageSize
                };
            }
            else
                throw await response.HandleUnsuccessfulResponse("Get Work Item List").ConfigureAwait(false);                            
        }
        private string BuildFinalWIQL(PaginationRequest request)
        {
            var query = "Select [System.Id] From WorkItems";
            if (!string.IsNullOrEmpty(request.SearchQuery))
                query += $" Where {request.SearchQuery}";

            if (!string.IsNullOrEmpty(request.SortBy))
            {
                var order = request.SortOrder?.ToLower() == "asc" ? "asc" : "desc";
                query += $" Order By [{request.SortBy}] {order}";
            }
            else
            {
                query += " Order By [System.CreatedDate] Desc";
            }
            return query;
        }

        /// <summary>
        /// Get Work Item Details for the given work item id / ticket no.
        /// </summary>
        /// <param name="id">Work Item Id</param>
        /// <returns></returns>
        public async Task<WorkItemSearchDto?> GetWorkItemDetailsAsync(string id)
        {
            var accessToken = await _authService.GetAccessToken(_contextAccessor);         
            var result = await GetWorkItemDetails(new List<int> { Convert.ToInt32(id) }, accessToken);
            var workItem = result?.Value?.Select(i => new WorkItemSearchDto
            {
                Id = i.Id,
                TeamProject = i.Fields?.GetStringValue("System.TeamProject"),
                BusinessDriver = i.Fields?.GetStringValue("Custom.Businessdriver"),
                MonthlyUSDRevenue = i.Fields?.GetStringValue("Custom.MonthlyUSDRevenue"),
                CustomerName = i.Fields?.GetStringValue("Custom.CustomerName"),
                OtherClientsAffected = i.Fields?.GetStringValue("Custom.OtherClientsEffected"),
                RegionVertical = i.Fields?.GetStringValue("Custom.RegionVertical"),
                ReportingCustomer = i.Fields?.GetStringValue("Custom.ReportingCustomer"),
                ProductName = i.Fields?.GetStringValue("Custom.Product"), 
            }).FirstOrDefault();
            return workItem;
        }

        private async Task<WorkItemsResult?> GetWorkItemDetails(IEnumerable<int> workItemIds, string accessToken)
        {
            var endpoint = $"_apis/wit/workitemsbatch?{_adoRestApiVersion}";
            var batchRequest = new
            {
                Ids = workItemIds,
                Expand = "All"
            };
            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, endpoint);
            requestMessage.Content = new StringContent(JsonSerializer.Serialize(batchRequest), System.Text.Encoding.UTF8, "application/json");
            requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            
            var response = await _httpClient.SendAsync(requestMessage).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<WorkItemsResult>().ConfigureAwait(false);
                return result;
            }
            else
                throw await response.HandleUnsuccessfulResponse("Get Work Item Details").ConfigureAwait(false);
        }

        public async Task<WorkItemCreateResponseDto> CreateWorkItem(TicketForm ticketForm, IEnumerable<IFormFile> attachments)
        {
            var accessToken = await _authService.GetAccessToken(_contextAccessor);
            var attachmentUrls = await _attachmentUploader.UploadAttachmentsAsync(_httpClient, accessToken, attachments);
            ticketForm.Attachments = attachmentUrls;
            return await _workItemCreatorService.CreateWorkItem(ticketForm, attachmentUrls, _httpClient, accessToken);            
        }

    }
}
