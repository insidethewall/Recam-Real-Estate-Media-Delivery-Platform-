using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RecamSystemApi.Models;

public class Agent
{
    [Key, ForeignKey("User")]
    public required string Id { get; set; }

    public required User User { get; set; }

    [Required]
    public required string CompanyName { get; set; }


    [Required]
    public required string AgentFirstName { get; set; }

    [Required]
    public required string AgentLastName { get; set; }

    public string? AvatarUrl { get; set; }

    public ICollection<AgentListingCase> AgentListingCases = new List<AgentListingCase>();
    public ICollection<AgentPhotographer> AgentPhotographer = new List<AgentPhotographer>();

}