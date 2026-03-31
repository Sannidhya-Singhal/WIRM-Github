using Azure;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Net;
using System.Net.Mail;
using System.Xml.Linq;
using WIRM.API.Controllers.CustomerOnboarding;
using WIRM.API.Interface;
using WIRM.API.Models.Request;
using WIRM.API.Models.Response;

namespace WIRM.API.Services
{
    public class ArrayCustomerEmailOnBoardingService : IArrayCustomerOnBoardingEmailService

    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;

        public ArrayCustomerEmailOnBoardingService(IConfiguration configuration, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _env = env;
        }

        public async Task SendEmailAsync(CustomerOnboardingFormDto customerOnboardingForm, WorkItemCreateResponseDto workItemCreateResponseDto)
        {
            string apiKey = _configuration["EmailDetails:ApiKey"];
            SendGridClient sendGridClient = new SendGridClient(apiKey);
            string finalHtml = GenerateEmailBodyForCustomerOnboarding(customerOnboardingForm.LoggedInUserEmail, customerOnboardingForm.CustomerAccountName,workItemCreateResponseDto.Id,workItemCreateResponseDto.TeamProject);
            SendGridMessage emailMessage = new SendGridMessage()
            {
                From = new EmailAddress(_configuration["EmailDetails:SolutionEngineeringEmail"]),
                Subject = customerOnboardingForm.TicketTitle,
                HtmlContent = finalHtml
            };
            #region "To" and "Cc" Emails
            var toEmails = _configuration.GetSection("EmailDetailsOnBoarding:To").Get<List<string>>();
            toEmails?.Add(customerOnboardingForm.LoggedInUserEmail);
            var ccEmails = _configuration.GetSection("EmailDetailsOnBoarding:Cc").Get<List<string>>();
            
            foreach (var to in toEmails)
            {
                emailMessage.AddTo(to);
            }
            foreach (var cc in ccEmails)
            {
                emailMessage.AddCc(cc);
            }
            #endregion

            SendGrid.Response response = await sendGridClient.SendEmailAsync(emailMessage);
            if (!(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Accepted))
            {
                string errorMessage = await response.Body.ReadAsStringAsync();
                throw new Exception(errorMessage);
            }
        }
        public string GenerateEmailBodyForCustomerOnboarding(
    string requestorEmail,
    string customerName,
    int userStoryId,
    string board) // team project name
        {
            string adoBaseUri = @"https://liox-teams.visualstudio.com/";
            string requestorName = ExtractFullName(requestorEmail);
            // Construct the clickable URL to the work item
            string userStoryUrl = $"{adoBaseUri}{board}/_workitems/edit/{userStoryId}";
            return $@"
<p>Hello Array team,</p>
<p>
    {requestorName} ({requestorEmail}) has submitted a ticket to have {customerName} added to Array.
    <a href='{userStoryUrl}' target='_blank'>User story #{userStoryId}</a> has been created and added to the backlog.
</p>
";
        }
        public string ExtractFullName(string email)
        {
            // Remove 'v-' prefix if it exists
            if (email.StartsWith("v-", StringComparison.OrdinalIgnoreCase))
            {
                email = email.Substring(2);
            }

            // Get substring before '@'
            int atIndex = email.IndexOf('@');
            string namePart = atIndex > 0 ? email.Substring(0, atIndex) : email;

            // Replace dots with spaces
            string fullName = namePart.Replace('.', ' ');

            return fullName;
        }
    }
}

