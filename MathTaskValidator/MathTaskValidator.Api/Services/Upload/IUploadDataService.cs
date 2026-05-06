namespace MathTaskValidator.Api.Services
{
    public interface IUploadDataService
    {
        Task<bool> UploadDataAsync(IFormFile file);
    }
}
