using RecamSystemApi.Models;

public class AgentListingCase
{
    public required string AgentId { get; set; }
    public required Agent Agent { get; set; }

    public required string ListingCaseId { get; set; }
    
    public required ListingCase ListingCase { get; set; }
}