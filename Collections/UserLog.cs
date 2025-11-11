using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using RecamSystemApi.DTOs;

public class UserLog
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public required UserDetailDto UserDetail { get; set; }

    public required UserAction Action { get; set; }

    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;

    public UserDetailDto? TargetedUser { get; set; }
    public string AdditionalInfo { get; set; } = string.Empty;
}