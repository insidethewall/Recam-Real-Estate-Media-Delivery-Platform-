using RecamSystemApi.Data;

public static class FileValidator
{
    // Removed instance fields and constructor because static classes cannot have them.
    public static bool IsFileExtensionAllowed(IFormFile file, string[] allowedExtensions)
    {
        var extension = Path.GetExtension(file.FileName);
        return allowedExtensions.Contains(extension);
    }

    public static bool IsFileSizeWithinLimit(IFormFile file, long maxSizeInBytes)
    {
        return file.Length <= maxSizeInBytes;
    }

    public static bool FileWithSameNameExists(IFormFile file, ReacmDbContext dbContext)
    {
        // Implement logic to check if a file with the same name exists in the system
        string fileName = file.FileName;
        return dbContext.MediaAssets.Any(m => m.FileName == fileName);
    }
    

}