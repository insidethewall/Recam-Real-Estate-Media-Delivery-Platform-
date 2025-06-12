using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RecamSystemApi.Models;

public class Photographer
{
    [Key, ForeignKey("User")]
    public required string Id { get; set; }

    public required User User { get; set; }

    [Required]
    public required string CompanyName { get; set; }

    [Required]
    public required string PhotographerFirstName { get; set; }

    [Required]
    public required string PhotographerLastName { get; set; }

    public string? AvatarUrl { get; set; }
    public ICollection<AgentPhotographer> AgentPhotographer = new List<AgentPhotographer>();
}