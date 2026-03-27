using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;

namespace WIRM.API.Models.Request
{
    public class CreateWorkItemRequestDto
    {
        [FromForm(Name = "form")]
        public string Form { get; set; } = string.Empty;
        [FromForm(Name = "files")]
        public List<IFormFile> Attachments { get; set; } = [];
        
    }
}
