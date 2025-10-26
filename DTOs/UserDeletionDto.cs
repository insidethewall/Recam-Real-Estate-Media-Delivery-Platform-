using RecamSystemApi.Models;

public class UserDeletionDto
{
    public required User user { get; set; }

    public Photographer? photographer { get; set; }
    public Agent? agent { get; set; }

}