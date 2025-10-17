namespace RecamSystemApi.Models
{
    public class MediaAsset
    {
        public required string Id { get; set; }

        public required string FileName { get; set; }

        public required string FilePath { get; set; } // e.g., Azure Blob URL or local path

        public MediaType MediaType { get; set; } // Enum: Photo, Video, FloorPlan, VR

        public bool IsHeroMedia { get; set; } = false;

        public bool IsDisplaySelected { get; set; } = false;

        public bool IsDeleted { get; set; } = false;

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

 
        public required string UserId { get; set; }
        public required User User { get; set; }
        public required string ListingCaseId { get; set; }

        public required ListingCase ListingCase { get; set; }
    }
}