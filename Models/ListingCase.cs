using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecamSystemApi.Models;

    public class ListingCase
    {
        [Key]
        public required string Id { get; set; }

        [Required]
        [MaxLength(200)]
        public required string Title { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        [MaxLength(255)]
        public string? Street { get; set; }

        [Required]
        [MaxLength(100)]
        public string? City { get; set; }

        [Required]
        [MaxLength(100)]
        public string? State { get; set; }
        [Required]
        public int Postcode { get; set; }

        [Column(TypeName = "decimal(9,6)")]
        public decimal Longitude { get; set; }

        [Column(TypeName = "decimal(9,6)")]
        public decimal Latitude { get; set; }

        public double Price { get; set; }

        public int Bedrooms { get; set; }

        public int Bathrooms { get; set; }

        public int Garages { get; set; }

        public double FloorArea { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; } = false;

        // Enums
        [Required]
        public PropertyType PropertyType { get; set; }

        [Required]
        public SaleCategory SaleCategory { get; set; }

        public ListcaseStatus ListcaseStatus { get; set; } = ListcaseStatus.Created;

        // Foreign Key (User)
        public required string UserId { get; set; }
        public required User User { get; set; }
        
        public ICollection<AgentListingCase> AgentListingCases = new List<AgentListingCase>();


        public ICollection<MediaAsset> MediaAssets = new List<MediaAsset>();
        public ICollection<CaseContact> CaseContacts =  new List<CaseContact>();
    }
