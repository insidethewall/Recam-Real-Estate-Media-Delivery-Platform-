using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace CourseManagementAPI.Services
{

    public class AzureBlobStorageService : IAzureBlobStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName;

        public AzureBlobStorageService(BlobServiceClient blobServiceClient, IConfiguration configuration)
        {
            _blobServiceClient = blobServiceClient;
            _containerName = configuration["AzureBlobStorage:ContainerName"] 
                ?? throw new ArgumentNullException("AzureBlobStorage:ContainerName configuration is missing.");
        }

        private void ValidateFile(IFormFile file)
        {
            var allowedContentTypes = new[] { "image/jpeg", "image/png", "image/gif", "application/pdf", "audio/mpeg" };
            const long maxFileSize = 10 * 1024 * 1024;

            if (!allowedContentTypes.Contains(file.ContentType.ToLower()))
                throw new InvalidOperationException($"File type '{file.ContentType}' is not allowed.");

            if (file.Length > maxFileSize)
                throw new InvalidOperationException("File size exceeds the 10 MB limit.");
        }

        public async Task<List<string>> UploadFileBulkAsync(IEnumerable<(IFormFile file, string pageName)> files)
        {
            var uploadedUrls = new List<string>();

            foreach (var (file, pageName) in files)
            {
                try
                {
                    var uploadedUrl = await UploadFileAsync(file, pageName);
                    uploadedUrls.Add(uploadedUrl);

                    Console.WriteLine($"Uploaded file for page: {pageName}, URL: {uploadedUrl}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to upload file for page: {pageName}. Error: {ex.Message}");
                }
            }

            return uploadedUrls;
        }


        public async Task<string> UploadFileAsync(IFormFile file, string folderName)
        {
            ValidateFile(file);
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);

                await containerClient.CreateIfNotExistsAsync();

                var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                var blobName = $"{folderName}/{timestamp}_{file.FileName}";
                var blobClient = containerClient.GetBlobClient(blobName);

                await using var stream = file.OpenReadStream();
                await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });

                return blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw;
            }
        }


        public async Task<(Stream Content, string ContentType, string FileName)> DownloadFileAsync(string blobUrl)
        {
            try
            {
                blobUrl = blobUrl.Replace(" ", "%20");

                Uri uri = new Uri(blobUrl);
                string hostName = uri.Host;
                string accountName = hostName.Split('.')[0];

                string[] segments = uri.AbsolutePath.TrimStart('/').Split('/');
                string containerName = segments[0];

                //handle Unicode characters of blob name
                string blobName = string.Join("/", segments.Skip(1).Select(s => Uri.UnescapeDataString(s)));

                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                var blobClient = containerClient.GetBlobClient(blobName);

                var exists = await blobClient.ExistsAsync();
                if (!exists)
                {
                    throw new FileNotFoundException($"The file at {blobUrl} was not found.");
                }

                BlobDownloadInfo download = await blobClient.DownloadAsync();

                string fileName = Uri.UnescapeDataString(Path.GetFileName(blobClient.Name));

                return (download.Content, download.ContentType, fileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while downloading: {ex.Message}");
                throw;
            }
        }

        //Not Tested
        public async Task<bool> DeleteFileAsync(string blobUrl)
        {
            try
            {
                Uri uri = new Uri(blobUrl);
                string hostName = uri.Host;
                string accountName = hostName.Split('.')[0];
                string[] segments = uri.AbsolutePath.TrimStart('/').Split('/');
                string containerName = segments[0];
                string blobName = string.Join("/", segments.Skip(1));

                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                var blobClient = containerClient.GetBlobClient(blobName);

                var exists = await blobClient.ExistsAsync();
                if (!exists)
                {
                    throw new FileNotFoundException($"The file at {blobUrl} was not found.");
                }

                var response = await blobClient.DeleteAsync();
                return response.Status == 202;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while deleting: {ex.Message}");
                throw;
            }
        }

    }

}