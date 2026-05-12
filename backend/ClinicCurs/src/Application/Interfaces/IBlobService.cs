namespace Application.Interfaces;

public interface IBlobService
{
    Task<string> UploadFileAsync(Stream fileStream, string originalFileName, string contentType, string containerName = "images");
    Task DeleteFileAsync(string fileName, string containerName = "images");
    Task<(Stream Stream, string ContentType, string FileName)> DownloadFileAsync(string fileName, string containerName = "images");
}
