using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using RecamSystemApi.DTOs;


public record FieldChange(string Field, string? OldValue, string? NewValue);
public class ListingCaseLog
{

  [BsonId]
  [BsonRepresentation(BsonType.ObjectId)]
  public string? Id { get; set; }  // Unique identifier for the log entry
  public required string ListingCaseId { get; set; }  // ID of the associated listing case
                                                      // Timestamp of the log entry
  public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
  public required UserDetailDto CreatedBy { get; set; }

  public UserDetailDto? ChangedBy { get; set; }

  public ChangeType ChangeType { get; set; }
  public string AdditionalInfo { get; set; } = string.Empty;
   public List<FieldChange> Changes { get; set; } = new();

}