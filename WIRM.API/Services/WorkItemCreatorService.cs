using Microsoft.AspNetCore.Components;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using WIRM.API.Extensions;
using WIRM.API.Interface;
using WIRM.API.Models;
using WIRM.API.Models.Request;
using WIRM.API.Models.Response;

namespace WIRM.API.Services
{
    public class WorkItemCreatorService : IWorkItemCreatorService
    {
        private readonly IWeightingCalculator _weightingCalculator;
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _contextAccessor;

        public WorkItemCreatorService(IWeightingCalculator weightingCalculator, IConfiguration config, IHttpContextAccessor contextAccessor)
        {
            _weightingCalculator = weightingCalculator;
            _config = config;
            _contextAccessor = contextAccessor;
        }

        public async Task<WorkItemCreateResponseDto> CreateWorkItem(TicketForm ticketForm, IEnumerable<string> attachmentUrls, HttpClient httpClient, string accessToken)
        {
            var ticketType = ticketForm.TicketType.Contains("bug") ? "$Bug" : "$User%20Story";
            var body = BuildWorkItemCreateRequestBody(ticketForm);
            AddAttachmentsToRequestBody(body, attachmentUrls);
            var endpoint = $"_apis/wit/workitems/{ticketType}?{_config["AdoApiSettings:Version"]}";

            using var requestMessage = new HttpRequestMessage(HttpMethod.Patch, endpoint);
            requestMessage.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json-patch+json");
            requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await httpClient.SendAsync(requestMessage);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ADOWorkItem>().ConfigureAwait(false);
                if (result == null)
                    throw new HttpRequestException("Deserialization error. ADO response's result format might've been changed.");

                return new WorkItemCreateResponseDto
                {
                    Id = result.Id,
                    Weighting = result.Fields["Custom.Weighting"].ToString().ToInt(),
                    TeamProject = result.Fields["System.TeamProject"].ToString(),
                    Title = result.Fields["System.Title"].ToString(),
                };
            }
            else
                throw await response.HandleUnsuccessfulResponse("Create Work Item").ConfigureAwait(false);
        }

        private List<CreateWorkItemBody> BuildWorkItemCreateRequestBody(TicketForm ticketForm)
        {
            var list = new List<CreateWorkItemBody>();
            string areaPath = string.Empty;
            var addWorkItemBody = (string propertyName, object value) =>
            {
                list.Add(new CreateWorkItemBody
                {
                    OP = "add",
                    Path = string.Format("/fields/{0}", propertyName),
                    Value = value
                });
            };

            int weighting = _weightingCalculator.Calculate(ticketForm);


            //used for setting value in mail html body.
            ticketForm.Weighing = weighting;


            //For staging environment comment below conditions. No areapath needed.
            areaPath = GetAreaPath(ticketForm.SubProductType);

            //For staging environment comment below conditions. No areapath needed.
            areaPath = GetAreaPath(ticketForm.SubProductType);
            if (!ticketForm.ProductType.Equals("Lionbridge Core Technology"))
            {
                areaPath = "SE";
            }
            if (string.IsNullOrEmpty(areaPath))
            {
                addWorkItemBody("System.AreaPath", $@"Operations Product Requests");
            }
            else
            {
                addWorkItemBody("System.AreaPath", $@"Operations Product Requests\{areaPath}");
            }

            addWorkItemBody("Custom.GeminiNumber", ticketForm.GeminiNumber);

            addWorkItemBody("Custom.BusinessDriver", ticketForm.BusinessDriver);
            addWorkItemBody("Custom.OriginalRequestor", $"{ticketForm.OriginalRequestorSide} - {ticketForm.RequestEmailAddress}");
            addWorkItemBody("Custom.ReportingCustomer", _contextAccessor.HttpContext?.User?.Identity?.Name ?? string.Empty);
            addWorkItemBody("System.Description", ticketForm.ReflectedWorkItemDescription);
            addWorkItemBody("Microsoft.VSTS.Common.AcceptanceCriteria", ticketForm.AcceptanceCriteria.ToHtmlString());
            addWorkItemBody("Custom.MonthlyUSDRevenue", ticketForm.Revenue);
            addWorkItemBody("Custom.CustomerName", ticketForm.CustomerName);
            addWorkItemBody("Custom.OtherClientsEffected", ticketForm.OtherClientsAffected);
            addWorkItemBody("Custom.RegionVertical", !string.IsNullOrEmpty(ticketForm.OtherVerticalType) ? ticketForm.OtherVerticalType : ticketForm.VerticalType);
            addWorkItemBody("Custom.Weighting", weighting);
            addWorkItemBody("System.Title", ticketForm.ReflectedWorkItemTitle);

            if (ticketForm.HasFirmDeadline)
            {
                addWorkItemBody("Custom.Deadline", ticketForm.Deadline.ToDateTime());
                addWorkItemBody("Custom.DeadlineType", "Firm Deadline");
            }
            else
                addWorkItemBody("Custom.DeadlineType", "No Deadline");

            if (!string.IsNullOrEmpty(ticketForm.ReflectedProductName))
                addWorkItemBody("Custom.Product", ticketForm.ReflectedProductName);

            if (!string.IsNullOrEmpty(ticketForm.ReferenceWorkItemId))
            {
                var jsonObject = new JsonObject
                {
                    ["rel"] = "System.LinkTypes.Related",
                    ["url"] = $"https://liox-teams.visualstudio.com/{ticketForm.ReferenceWorkItemTeamProject}/_apis/wit/workItems/{ticketForm.ReferenceWorkItemId}",
                    ["attributes"] = new JsonObject
                    {
                        ["comment"] = "Related Work Items"
                    }
                };
                list.Add(new CreateWorkItemBody
                {
                    OP = "add",
                    Path = "/relations/-",
                    Value = jsonObject,
                });
            }

            return list;
        }

        private void AddAttachmentsToRequestBody(List<CreateWorkItemBody> workItemBody, IEnumerable<string> attachmentUrls)
        {
            foreach (var url in attachmentUrls)
            {
                workItemBody.Add(new CreateWorkItemBody
                {
                    OP = "add",
                    Path = "/relations/-",
                    Value = new
                    {
                        rel = "AttachedFile",
                        url,
                        attributes = new
                        {
                            comment = "Uploaded from OPR Form Web App"
                        },
                    },
                });
            }
        }

        private static string GetAreaPath(string subProductType)
        {
            switch (subProductType)
            {
                case "Aurora Studio":
                    return "Aurora Studio";
                case "Aurora Array":
                    return "Aurora Array";
                case "Aurora AI":
                    return "Aurora AI";
                case "Connectivity":
                    return "Connectors";
                case "JTS":
                    return "JTS";
                case "LangAI System":
                    return "LangAI System";
                case "LCX":
                    return "LCX";
                case "LTB":
                    return "LTB";
                case "ORT":
                    return "TW\\ORT (Online Review Tool)";
                case "Language Cloud (LLC)":
                    return "LLC (Lionbridge Language Cloud)";
                case "QA App":
                    return "QA App";
                case "CPQ":
                    return "CPQ";
                case "Gemini":
                    return "Gemini";
                case "PowerBI and reporting":
                    return "BI Dashboards";
                case "Content Remix App":
                    return "Content Remix App";
                case "TMS":
                    return "TMS";
                case "TW":
                    return "TW";
                case "Content Remix Generator Template":
                    return "Content Remix Generator Template";
                case "Other":
                    return null;
                default:
                    return null;
            }
        }
    }
}
