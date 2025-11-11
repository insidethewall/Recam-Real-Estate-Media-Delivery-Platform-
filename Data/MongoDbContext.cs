using Microsoft.Extensions.Options;
using MongoDB.Driver;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IOptions<MongoDbSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionStrings);
        _database = client.GetDatabase(settings.Value.DatabaseName);
    }

    public IMongoCollection<ListingCaseLog> ListingCaseLogs =>
        _database.GetCollection<ListingCaseLog>("ListingCaseLogs");

    public IMongoCollection<UserLog> UserLogs =>
        _database.GetCollection<UserLog>("UserLogs");
}