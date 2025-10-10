public interface IAzureBlobStorageService
{
    Task<string> UploadFileAsync(IFormFile file, string folderName);
    Task<IDictionary<string, string>> UploadFileBulkAsync(IEnumerable<(IFormFile file, string mediaType)> files);
    Task<(Stream Content, string ContentType, string FileName)> DownloadFileAsync(string blobUrl);
    Task<bool> DeleteFileAsync(string blobUrl);
}