using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WIRM.API.Extensions;
using WIRM.API.Interface;
using WIRM.API.Models.Request;
using WIRM.API.Models.Response;

namespace WIRM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WorkItemsController : ControllerBase
    {
        private readonly ILogger<WorkItemsController> _logger;
        private readonly IOPRService _oprService;
        private readonly ISEService _seService;
        private readonly ITicketEmailService _emailService;

        public WorkItemsController(ILogger<WorkItemsController> logger, IOPRService oprService, ISEService seService,ITicketEmailService emailService)
        {
            _logger = logger;
            _oprService = oprService;
            _seService = seService;
            _emailService = emailService;
        }

        [HttpGet]
        [Authorize(Policy = "User.Read")]
        public async Task<IActionResult> GetWorkItems([FromQuery] PaginationRequest request)
        {
            if (request.CurrentPage < 1) request.CurrentPage = 1;
            if (request.PageSize < 1 || request.PageSize > 100) request.PageSize = 10;

            try
            {                
                var result = await _oprService.GetWorkItemsAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while fetching work items.\nMessage {ex.GetInnerMostExceptionMessage()}");
            }
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "User.Read")]
        public async Task<IActionResult> GetWorkItemDetails(string id)
        {
            try
            {
                var result = await _oprService.GetWorkItemDetailsAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occured while fetching work item details.\nMessage {ex.GetInnerMostExceptionMessage()}");
            }
        }

        [HttpPost]
        [Authorize(Policy = "User.Read")]
        public async Task<IActionResult> CreateTicket([FromForm] CreateWorkItemRequestDto request)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var option = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    };
                    var form = JsonSerializer.Deserialize<TicketForm>(request.Form, option);
                    if (form == null)
                        return BadRequest("Empty form is submitted.");
                    var result = new WorkItemCreateResponseDto();
                
                        //For Production
                        result = await _oprService.CreateWorkItem(form, request.Attachments);
                   
                        //For Stage testing
                       // result = await _seService.CreateWorkItem(form, request.Attachments);
                    
                        await _emailService.SendEmailAsync(form, result);
                    return Ok(result);
                }

                return BadRequest(ModelState);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occured on processing the submitted ticket form.\nMessage: {ex.GetInnerMostExceptionMessage()}");
            }
        }
    }
}
