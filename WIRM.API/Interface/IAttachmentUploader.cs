namespace WIRM.API.Interface
{
    public interface IAttachmentUploader
    {
        Task<List<string>> UploadAttachmentsAsync(HttpClient httpClient, string accessToken, IEnumerable<IFormFile> files);    
    }
}
