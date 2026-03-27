using Azure;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Net;
using System.Net.Mail;
using System.Xml.Linq;
using WIRM.API.Interface;
using WIRM.API.Models.Request;
using WIRM.API.Models.Response;

namespace WIRM.API.Services
{
    public class TicketEmailService : ITicketEmailService

    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;

        public TicketEmailService(IConfiguration configuration, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _env = env;
        }

        public async Task SendEmailAsync(TicketForm ticketForm, WorkItemCreateResponseDto workItemCreateResponseDto)
        {
            string apiKey = _configuration["EmailDetails:ApiKey"];
            SendGridClient sendGridClient = new SendGridClient(apiKey);
            string finalHtml = GenerateEmailBody(ticketForm, workItemCreateResponseDto);
            SendGridMessage emailMessage = new SendGridMessage()
            {
                From = new EmailAddress(_configuration["EmailDetails:SolutionEngineeringEmail"]),
                Subject = ticketForm.ReflectedWorkItemTitle,
                HtmlContent = finalHtml
            };

            emailMessage.AddTo(ticketForm.RequestEmailAddress);

            SendGrid.Response response = await sendGridClient.SendEmailAsync(emailMessage);
            if (!(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Accepted))
            {
                string errorMessage = await response.Body.ReadAsStringAsync();
                throw new Exception(errorMessage);
            }
        }

        private string GenerateEmailBody(TicketForm ticketForm, WorkItemCreateResponseDto workItemCreateResponseDto)
        {
            ticketForm.Attachments = getAttachedFilenames(ticketForm.Attachments);
            ticketForm.OriginalRequestorSide = $"{ticketForm.OriginalRequestorSide} - {ticketForm.RequestEmailAddress}";
            ticketForm.ChangedBy = GetChangedBy(ticketForm.RequestEmailAddress);
            ticketForm.TicketId = workItemCreateResponseDto.Id;
            string htmlTemplate = GetTemplate();
            string finalHtml = PopulateTemplate(htmlTemplate, ticketForm);
            return finalHtml;
        }

        public string GetTemplate()
        {
            var path = Path.Combine(_env.ContentRootPath, "Templates", "TicketEmailBody.html");
            return File.ReadAllText(path);
        }

        public static string PopulateTemplate<T>(string template, T data)
        {
            var properties = typeof(T).GetProperties();
            foreach (var prop in properties)
            {
                string placeholder = $"{{{{{prop.Name}}}}}";
                var value = prop.GetValue(data);

                if (value is IEnumerable<string> list) // handle collections
                {
                    var sb = new System.Text.StringBuilder();
                    foreach (var item in list)
                    {
                        sb.AppendLine(
                    $@"<li><span class='tag'>📎 {item}</span></li>"
                );

                    }
                    template = template.Replace(placeholder, sb.ToString());
                }
                else
                {
                    template = template.Replace(placeholder, value?.ToString() ?? string.Empty);
                }
            }
            return template;
        }

        private string GetChangedBy(string requestEmailAddress)
        {
            var result = requestEmailAddress.Split('@')[0];
            var changedBy = result.Replace(".", " "); 
            return changedBy;
        }

        private List<string> getAttachedFilenames( List<string> attachments)
        {
            var newAttachments = new List<string>();
            foreach (var url in attachments)
            {
                var uri = new Uri(url);
                var queryParams = QueryHelpers.ParseQuery(uri.Query);
                string fileName = queryParams["fileName"];
                newAttachments.Add(fileName);
            }

            return newAttachments;
        }
    }
}

