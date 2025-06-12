using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecamSystemApi.Models
{
    public class CaseContact
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public required string FullName { get; set; }

        [Required]
        [Phone]
        public required string PhoneNumber { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        public string? Company { get; set; }

        public string? ProfileUrl { get; set; }

        // Foreign Key to ListingCase
        [Required]
        public required string ListingCaseId { get; set; }

        [ForeignKey(nameof(ListingCaseId))]
        public required ListingCase ListingCase { get; set; }
    }
}