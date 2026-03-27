using System.Linq;
using System.Text;
using System.Text.Json;
using WIRM.API.Extensions;
using WIRM.API.Interface;
using WIRM.API.Models;

namespace WIRM.API.Services
{
    public class AttachmentUploader : IAttachmentUploader
    {
        private readonly IConfiguration _config;
        public AttachmentUploader(IConfiguration config)
        {
            _config = config;
        }

        public async Task<List<string>> UploadAttachmentsAsync(HttpClient httpClient, string accessToken, IEnumerable<IFormFile> files)
        {
            if (files?.Count() == 0) return new List<string>();

            var tasks = files.Select(async file =>
            {
                var endpoint = $"_apis/wit/attachments?fileName={Uri.EscapeDataString(file.FileName)}&{_config["AdoApiSettings:Version"]}";
                using var requestMessage = new HttpRequestMessage(HttpMethod.Post, endpoint);
                using var stream = file.OpenReadStream();
                using var content = new StreamContent(stream);
                requestMessage.Content = content;
                requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await httpClient.SendAsync(requestMessage);  
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ADOAttachmentUploadResult>();
                    return result.Url;
                }
                else
                    throw await response.HandleUnsuccessfulResponse("Upload Attachments").ConfigureAwait(false);                
            });

            return (await Task.WhenAll(tasks)).ToList();
        }
    }
}