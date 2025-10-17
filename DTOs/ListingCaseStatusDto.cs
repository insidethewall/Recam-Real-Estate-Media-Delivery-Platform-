namespace RecamSystemApi.DTOs;

public class ListingCaseStatusDto
{
    public required string Id { get; set; }
    public required string title { get; set; }
    public required ListcaseStatus Status { get; set; }
}