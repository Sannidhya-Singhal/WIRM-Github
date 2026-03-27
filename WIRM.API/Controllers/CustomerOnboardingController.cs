using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WIRM.API.Interface;
using WIRM.API.Models.Response;

namespace WIRM.API.Controllers.CustomerOnboarding
{

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CustomerOnboardingController : ControllerBase
    {
        private readonly ISEService _seService;
        private readonly IArrayCustomerOnBoardingEmailService _customerOnBoardingEmailService;
        private readonly ILogger<CustomerOnboardingController> _logger;
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        public CustomerOnboardingController(ISEService seService, IArrayCustomerOnBoardingEmailService customerOnBoardingEmailService, ILogger<CustomerOnboardingController> logger)
        {
            _seService = seService;
            _customerOnBoardingEmailService = customerOnBoardingEmailService;
            _logger = logger;
        }

        /// <summary>
        /// Accepts multipart/form-data: <c>form</c> (JSON) + optional <c>otherUsersExcel</c>.
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "User.Read")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(WorkItemCreateResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> OnboardCustomer(
            [FromForm] CustomerOnboardingMultipartRequest request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Form))
                return BadRequest("Missing form payload.");

            CustomerOnboardingFormDto? dto;
            try
            {
                dto = JsonSerializer.Deserialize<CustomerOnboardingFormDto>(request.Form, JsonOptions);
            }
            catch (JsonException ex)
            {
                return BadRequest($"Invalid JSON in form field: {ex.Message}");
            }

            if (dto is null)
                return BadRequest("Could not parse form payload.");

            // Optional: save / parse Excel
            if (request.OtherUsersExcel is { Length: > 0 })
            {
                await using var stream = request.OtherUsersExcel.OpenReadStream();
                // TODO: validate content type / extension, parse rows, merge with dto.OtherUsers, etc.
            }

            // TODO: map dto to domain entity, persist, queue work item, etc.
            var result = await _seService.CreateCustomerOnboardingWorkItem(dto, request.OtherUsersExcel);
            try
            {
                dto.LoggedInUserEmail = HttpContext?.User?.Identity?.Name ?? string.Empty;
                await _customerOnBoardingEmailService.SendEmailAsync(dto, result);
            }
            catch (Exception ex)
            {
                // Log the email failure, but don't fail the entire request
                _logger.LogError(ex, "Failed to send onboarding email for customer {CustomerAccountName}", dto.CustomerAccountName);
            }
            return Ok(result);
        }
    }
}