public interface IAzureBlobStorageService
{
    Task<string> UploadFileAsync(IFormFile file, string folderName);
    Task<List<string>> UploadFileBulkAsync(IEnumerable<(IFormFile file, string pageName)> files);
    Task<(Stream Content, string ContentType, string FileName)> DownloadFileAsync(string blobUrl);
    Task<bool> DeleteFileAsync(string blobUrl);
}