using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using WIRM.API.Controllers.CustomerOnboarding;
using WIRM.API.Extensions;
using WIRM.API.Interface;
using WIRM.API.Models;
using WIRM.API.Models.Response;

namespace WIRM.API.Models.Request
{
    public class CustomerOnboardingWorkItemService : ICustomerOnboardingWorkItemService
    {
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _contextAccessor;
        public CustomerOnboardingWorkItemService(IConfiguration config, IHttpContextAccessor contextAccessor)
        {
            _config = config;
            _contextAccessor = contextAccessor;
        }

        public async Task<WorkItemCreateResponseDto> CreateWorkItem(CustomerOnboardingFormDto customerOnboardingFormDto, IEnumerable<string> attachmentUrls, HttpClient httpClient, string accessToken)
        {
            try
            {
                var body = BuildWorkItemCreateRequestBody(customerOnboardingFormDto);
                AddAttachmentsToRequestBody(body, attachmentUrls);
                var endpoint = $"_apis/wit/workitems/$User%20Story?{_config["AdoApiSettings:Version"]}";

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
                        //Weighting = result.Fields["Custom.Weighting"].ToString().ToInt(),
                        TeamProject = result.Fields["System.TeamProject"].ToString(),
                        Title = result.Fields["System.Title"].ToString(),
                    };
                }
                else
                    throw await response.HandleUnsuccessfulResponse("Create Work Item").ConfigureAwait(false);
            }
            catch (Exception ex)
            { 
            
            }
            return null;
        }

        private List<CreateWorkItemBody> BuildWorkItemCreateRequestBody(CustomerOnboardingFormDto customerOnboardingFormDto)
        {
            string description = BuildDescription(customerOnboardingFormDto,true);
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

                areaPath = "SE";
               // addWorkItemBody("System.AreaPath", $@"Operations Product Requests\{areaPath}");
            
            addWorkItemBody("System.Description", description);
            addWorkItemBody("System.Title", customerOnboardingFormDto.TicketTitle);
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

        private string CreateAndGetDescription(CustomerOnboardingFormDto dto) 
        {
            var sb = new StringBuilder();

            var separator = string.Empty;
            for (int i = 0; i < 40; i++)
                separator += "=";

            // Step 1 - Account Details
            sb.AppendLine("Step 1 - Account Details");
            sb.AppendLine($"- Customer Group Name: {dto.CustomerGroupName}");
            sb.AppendLine($"- Customer Account Name: {dto.CustomerAccountName}");
            sb.AppendLine($"- Coda Code: {dto.CodaCode}");
            sb.AppendLine();

            // Step 2 - Optional Module Settings
            sb.AppendLine(separator);
            sb.AppendLine("Step 2 - Optional Module Settings");
            sb.AppendLine($"- Validate: {dto.Validate}");
            sb.AppendLine($"- Engage: {dto.Engage}");
            sb.AppendLine($"- Insights: {dto.Insights}");
            sb.AppendLine($"- Apps: {dto.Apps}");
            sb.AppendLine($"- MSP: {dto.Msp}");
            sb.AppendLine();

            // Step 3 - User and Roles
            
            sb.AppendLine("Step 3 - User and Roles");
            sb.AppendLine($"- Primary User First Name: {dto.PrimaryUserFirstName}");
            sb.AppendLine($"- Primary User Last Name: {dto.PrimaryUserLastName}");
            sb.AppendLine($"- Primary User Email Address: {dto.PrimaryUserEmailAddress}");
            if (dto.OtherUsers?.Count > 0)
            {
                sb.AppendLine("- Other Users:");
                foreach (var user in dto.OtherUsers)
                {
                    sb.AppendLine($"    • First Name: {user.FirstName}");
                    sb.AppendLine($"    • Last Name: {user.LastName}");
                    sb.AppendLine($"    • Email: {user.Email}");
                    sb.AppendLine($"    • Role: {user.Role}");
                    sb.AppendLine();
                }
            }
            sb.AppendLine();

            // Step 4 - Process Specifics
            sb.AppendLine(separator);
            sb.AppendLine("Step 4 - Process Specifics");
            if (dto.CustomizedTemplates?.Count > 0)
            {
                sb.AppendLine("- Customized Templates:");
                foreach (var template in dto.CustomizedTemplates)
                {
                    sb.AppendLine($"    • Desired Name: {template.DesiredName}");
                    sb.AppendLine($"    • Existing Aurora Process: {template.ExistingAuroraProcess}");
                    sb.AppendLine($"    • Available AddOns: {template.AvailableAddOns}");
                    sb.AppendLine($"    • Work Type: {template.WorkType}");
                    sb.AppendLine($"    • Service: {template.Service}");
                    sb.AppendLine();
                }
            }
            sb.AppendLine();

            // Step 5 - Modifiers
            sb.AppendLine(separator);
            sb.AppendLine("Step 5 - Modifiers");

            void AppendModifiers(string title, List<ModifierRowDto> modifiers)
            {
                if (modifiers?.Count > 0)
                {
                    sb.AppendLine($"- {title}:");
                    foreach (var mod in modifiers)
                    {
                        sb.AppendLine($"    • Name: {mod.Name}");
                        sb.AppendLine($"    • Values: {mod.Values}");
                        sb.AppendLine($"    • Details Purpose: {mod.DetailsPurpose}");
                        sb.AppendLine($"    • Expected Behavior: {mod.ExpectedBehaviorWhenSelected}");
                        sb.AppendLine();
                    }
                }
            }

            AppendModifiers("Business Modifiers", dto.BusinessModifiers);
            AppendModifiers("Finance Modifiers", dto.FinanceModifiers);
            AppendModifiers("Process Modifiers", dto.ProcessModifiers);
            sb.AppendLine();

            // Step 6 - Considerations
            sb.AppendLine(separator);
            sb.AppendLine("Step 6 - Considerations");
            sb.AppendLine($"- Anything Else: {dto.ConsiderAnythingElse}");
            sb.AppendLine($"- Urgent Deployment Date: {dto.ConsiderUrgentDeployment}");
            sb.AppendLine($"- Operational Owner Account: {dto.ConsiderOperationalOwnerAccount}");
            sb.AppendLine($"- Account Manager: {dto.ConsiderAccountManager}");
            sb.AppendLine($"- Migration POC: {dto.ConsiderMigrationPoc}");
            sb.AppendLine();

            return sb.ToString();
        }

        private static string BuildDescription(CustomerOnboardingFormDto dto, bool asHtml = false)
        {
            var sb = new StringBuilder();

            void AddSeparator()
            {
                if (asHtml)
                    sb.AppendLine("<hr />");
                else
                {
                    sb.AppendLine();
                    sb.AppendLine("**************************************************************************************");
                    sb.AppendLine("**************************************************************************************");
                    sb.AppendLine();
                }
            }

            // Step 1 - Account Details
            AddSeparator();
            if (asHtml) sb.AppendLine("<h2>Step 1 - Account Details</h2><ul>");
            else sb.AppendLine("Step 1 - Account Details");

            if (asHtml)
            {
                sb.AppendLine($"<li>Customer Group Name: {dto.CustomerGroupName}</li>");
                sb.AppendLine($"<li>Customer Account Name: {dto.CustomerAccountName}</li>");
                sb.AppendLine($"<li>Coda Code: {dto.CodaCode}</li>");
                sb.AppendLine("</ul>");
            }
            else
            {
                sb.AppendLine($"- Customer Group Name: {dto.CustomerGroupName}");
                sb.AppendLine($"- Customer Account Name: {dto.CustomerAccountName}");
                sb.AppendLine($"- Coda Code: {dto.CodaCode}");
            }

            // Step 2 - Optional Module Settings
            AddSeparator();
            if (asHtml) sb.AppendLine("<h2>Step 2 - Optional Module Settings</h2><ul>");
            else sb.AppendLine("Step 2 - Optional Module Settings");

            string[] modules = {
    $"Validate: {(dto.Validate ? "On" : "Off")}",
    $"Engage: {(dto.Engage ? "On" : "Off")}",
    $"Insights: {(dto.Insights ? "On" : "Off")}",
    $"Apps: {(dto.Apps ? "On" : "Off")}",
    $"MSP: {(dto.Msp ? "On" : "Off")}"
};
            foreach (var m in modules)
            {
                if (asHtml) sb.AppendLine($"<li>{m}</li>");
                else sb.AppendLine($"- {m}");
            }
            if (asHtml) sb.AppendLine("</ul>");

            // Step 3 - User and Roles
            AddSeparator();
            if (asHtml) sb.AppendLine("<h2>Step 3 - User and Roles</h2><ul>");
            else sb.AppendLine("Step 3 - User and Roles");

            if (asHtml)
            {
                sb.AppendLine($"<li>Primary User First Name: {dto.PrimaryUserFirstName}</li>");
                sb.AppendLine($"<li>Primary User Last Name: {dto.PrimaryUserLastName}</li>");
                sb.AppendLine($"<li>Primary User Email Address: {dto.PrimaryUserEmailAddress}</li>");
                if (dto.OtherUsers != null && dto.OtherUsers.Any())
                {
                    sb.AppendLine("<li>Other Users:<ul>");
                    foreach (var u in dto.OtherUsers)
                    {
                        sb.AppendLine(
    $"<li>First Name: {(string.IsNullOrWhiteSpace(u.FirstName) ? "Not provided" : u.FirstName)} | " +
    $"Last Name: {(string.IsNullOrWhiteSpace(u.LastName) ? "Not provided" : u.LastName)} | " +
    $"Email: {(string.IsNullOrWhiteSpace(u.Email) ? "Not provided" : u.Email)} | " +
    $"Role: {(string.IsNullOrWhiteSpace(u.Role) ? "Not provided" : u.Role)}</li>"
);
                    }
                    sb.AppendLine("</ul></li>");
                }
                else
                {
                    sb.AppendLine("<li>Other Users: Not Provided</li>");
                }

                sb.AppendLine("</ul>");
            }
            else
            {
                sb.AppendLine($"- Primary User First Name: {dto.PrimaryUserFirstName}");
                sb.AppendLine($"- Primary User Last Name: {dto.PrimaryUserLastName}");
                sb.AppendLine($"- Primary User Email Address: {dto.PrimaryUserEmailAddress}");
                if (dto.OtherUsers != null && dto.OtherUsers.Any())
                {
                    sb.AppendLine("- Other Users:");
                    foreach (var u in dto.OtherUsers)
                    {
                        sb.AppendLine(
    $"  - First Name: {(string.IsNullOrWhiteSpace(u.FirstName) ? "Not provided" : u.FirstName)} | " +
    $"Last Name: {(string.IsNullOrWhiteSpace(u.LastName) ? "Not provided" : u.LastName)} | " +
    $"Email: {(string.IsNullOrWhiteSpace(u.Email) ? "Not provided" : u.Email)} | " +
    $"Role: {(string.IsNullOrWhiteSpace(u.Role) ? "Not provided" : u.Role)}"
);
                    }
                }
                else
                {
                    sb.AppendLine($"- Other Users: Not Provided");
                }
            }

            // Step 4 - Process Specifics
            AddSeparator();
            if (asHtml) sb.AppendLine("<h2>Step 4 - Process Specifics</h2><ul>");
            else sb.AppendLine("Step 4 - Process Specifics");

            if (dto.CustomizedTemplates.Any())
            {
                foreach (var t in dto.CustomizedTemplates)
                {
                    string NP(string? v) => string.IsNullOrWhiteSpace(v) ? "Not provided" : v;
                    string NPList(IEnumerable<string>? v) =>
                        (v == null || !v.Any()) ? "Not provided" : string.Join(", ", v);

                    if (asHtml)
                    {
                        var addOns = NPList(t.AvailableAddOns);
                        var workType = NPList(t.WorkType);
                        var service = NPList(t.Service);

                        sb.AppendLine($"<li>Desired Name: {NP(t.DesiredName)}</li>");
                        sb.AppendLine($"<li>Existing Aurora Process: {NP(t.ExistingAuroraProcess)}</li>");
                        sb.AppendLine($"<li>Available AddOns: {addOns}</li>");
                        sb.AppendLine($"<li>Work Type: {workType}</li>");
                        sb.AppendLine($"<li>Service: {service}</li>");
                    }
                    else
                    {
                        var addOns = NPList(t.AvailableAddOns);
                        var workType = NPList(t.WorkType);
                        var service = NPList(t.Service);

                        sb.AppendLine($"- Desired Name: {NP(t.DesiredName)}");
                        sb.AppendLine($"- Existing Aurora Process: {NP(t.ExistingAuroraProcess)}");
                        sb.AppendLine($"- Available AddOns: {addOns}");
                        sb.AppendLine($"- Work Type: {workType}");
                        sb.AppendLine($"- Service: {service}");
                    }
                }
            }
            else
            {
                if (asHtml) sb.AppendLine("<li>No customized templates provided</li>");
                else sb.AppendLine("- No customized templates provided");
            }
            if (asHtml) sb.AppendLine("</ul>");

            // Step 5 - Modifiers
            AddSeparator();
            if (asHtml) sb.AppendLine("<h2>Step 5 - Modifiers</h2><ul>");
            else sb.AppendLine("Step 5 - Modifiers");

            void AppendModifiers(string title, List<ModifierRowDto>? mods)
            {
                string NP(string? v) => string.IsNullOrWhiteSpace(v) ? "Not provided" : v;

                if (mods != null && mods.Any())
                {
                    if (asHtml)
                    {
                        sb.AppendLine($"<li>{title}:<ul>");
                        foreach (var m in mods)
                        {
                            var formattedValues = m.Values?
                                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(v => v.Trim())
                                .DefaultIfEmpty("Not provided");

                            sb.AppendLine(
                                $"<li>" +
                                $"Name: {NP(m.Name)} | " +
                                $"Values: {string.Join(", ", formattedValues ?? ["Not provided"])} | " +
                                $"Details/Purpose: {NP(m.DetailsPurpose)} | " +
                                $"Expected Behavior When Selected: {NP(m.ExpectedBehaviorWhenSelected)}" +
                                $"</li>"
                            );
                        }
                        sb.AppendLine("</ul></li>");
                    }
                    else
                    {
                        sb.AppendLine($"- {title}:");
                        foreach (var m in mods)
                        {
                            var formattedValues = m.Values?
                                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(v => v.Trim())
                                .DefaultIfEmpty("Not provided");

                            sb.AppendLine(
                                $"   • Name: {NP(m.Name)} | " +
                                $"Values: {string.Join(", ", formattedValues ?? ["Not provided"])} | " +
                                $"Details/Purpose: {NP(m.DetailsPurpose)} | " +
                                $"Expected Behavior When Selected: {NP(m.ExpectedBehaviorWhenSelected)}"
                            );
                        }
                    }
                }
                else
                {
                    if (asHtml) sb.AppendLine($"<li>No {title.ToLower()} provided</li>");
                    else sb.AppendLine($"- No {title.ToLower()} provided");
                }
            }

            AppendModifiers("Business Modifiers", dto.BusinessModifiers);
            AppendModifiers("Finance Modifiers", dto.FinanceModifiers);
            AppendModifiers("Process Modifiers", dto.ProcessModifiers);

            if (asHtml) sb.AppendLine("</ul>");

            // Step 6 - Considerations
            AddSeparator();
            if (asHtml) sb.AppendLine("<h2>Step 6 - Considerations</h2><ul>");
            else sb.AppendLine("Step 6 - Considerations");

            string[] considerations = {
            $"Anything Else: {(string.IsNullOrWhiteSpace(dto.ConsiderAnythingElse) ? "Not provided" : dto.ConsiderAnythingElse)}",
            $"Urgent Deployment Date: {dto.ConsiderUrgentDeployment}",
            $"Operational Owner Account: {dto.ConsiderOperationalOwnerAccount}",
            $"Account Manager: {dto.ConsiderAccountManager}",
            $"Migration POC: {dto.ConsiderMigrationPoc}"
        };

            foreach (var c in considerations)
            {
                if (asHtml) sb.AppendLine($"<li>{c}</li>");
                else sb.AppendLine($"- {c}");
            }
            if (asHtml) sb.AppendLine("</ul>");

            return sb.ToString();
        }
    }
}



