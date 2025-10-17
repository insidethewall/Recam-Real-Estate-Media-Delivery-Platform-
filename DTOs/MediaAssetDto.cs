namespace RecamSystemApi.Models
{
    public class MediaAssetDto
    {
        public required string FileName { get; set; }

        public required string FilePath { get; set; } // e.g., Azure Blob URL or local path

        public MediaType MediaType { get; set; } // Enum: Photo, Video, FloorPlan, VR

        public bool IsHeroMedia { get; set; } = false;

        public bool IsDisplaySelected { get; set; } = false;

        public bool IsDeleted { get; set; } = false;
        public required string UserId { get; set; }
        public required string ListingCaseId { get; set; }
    }
}