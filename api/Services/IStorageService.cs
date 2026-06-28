namespace api.Services;

public interface IStorageService
{
    public Task<string> UploadFileAsync(Stream fileStream, string fileName, string requestHash);
}
