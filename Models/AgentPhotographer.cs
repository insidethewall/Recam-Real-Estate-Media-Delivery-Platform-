public class AgentPhotographer
{
    public required string AgentId { get; set; }
    public required Agent Agent { get; set; }

    public required string PhotographerId { get; set; }
    
    public required Photographer Photographer { get; set; }
}