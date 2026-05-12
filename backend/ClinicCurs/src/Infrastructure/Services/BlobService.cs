using Application.Interfaces;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services;
public class BlobService : IBlobService
{
    private readonly BlobServiceClient _blobServiceClient;

    public BlobService(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Azurite");
        _blobServiceClient = new BlobServiceClient(connectionString);
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string originalFileName, string contentType, string containerName = "images")
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

        // Генерируем уникальное имя файла: GUID + расширение оригинального файла
        var extension = Path.GetExtension(originalFileName);
        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        
        var blobClient = containerClient.GetBlobClient(uniqueFileName);

        var options = new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
        };

        await blobClient.UploadAsync(fileStream, options);

        // ВОЗВРАЩАЕМ ТОЛЬКО ИМЯ ФАЙЛА (например: 3f8a...9c.jpg)
        return uniqueFileName;
    }

    public async Task DeleteFileAsync(string fileName, string containerName = "images")
    {
        if (string.IsNullOrWhiteSpace(fileName)) return;

        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(fileName);
            await blobClient.DeleteIfExistsAsync();
        }
        catch { /* Игнорируем ошибки, если файла уже нет */ }
    }

    public async Task<(Stream Stream, string ContentType, string FileName)> DownloadFileAsync(string fileName, string containerName = "images")
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("Имя файла не может быть пустым");

        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(fileName);

        if (!await blobClient.ExistsAsync())
        {
            throw new FileNotFoundException("Файл не найден в хранилище.");
        }

        var downloadInfo = await blobClient.DownloadAsync();
        return (downloadInfo.Value.Content, downloadInfo.Value.ContentType, fileName);
    }
}
