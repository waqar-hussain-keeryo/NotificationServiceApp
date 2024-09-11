using MongoDB.Bson;
using MongoDB.Driver;

public class MongoDBContext
{
    private readonly IMongoDatabase _database;
    private readonly IMongoCollection<BsonDocument> _customersCollection;
    private readonly IMongoCollection<BsonDocument> _digitalServicesCollection;

    public MongoDBContext(string databaseName, string customersCollectionName, string digitalServicesCollectionName)
    {
        var client = new MongoClient("mongodb://localhost:27017");
        _database = client.GetDatabase(databaseName);
        _customersCollection = _database.GetCollection<BsonDocument>(customersCollectionName);
        _digitalServicesCollection = _database.GetCollection<BsonDocument>(digitalServicesCollectionName);
    }

    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            var command = new BsonDocument("ping", 1);
            await _database.RunCommandAsync<BsonDocument>(command);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Connection failed: {ex.Message}");
            return false;
        }
    }

    public async Task<List<BsonDocument>> GetAllCustomersAsync()
    {
        try
        {
            return await _customersCollection.Find(new BsonDocument()).ToListAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching customers: {ex.Message}");
            return new List<BsonDocument>();
        }
    }

    public async Task<List<BsonDocument>> GetAllDigitalServicesAsync()
    {
        try
        {
            var pipeline = new[]
            {
                // Unwind the DigitalServices array
                new BsonDocument("$unwind", "$DigitalServices"),

                // Project the required fields
                new BsonDocument("$project", new BsonDocument
                {
                    { "_id", 0 },
                    { "DigitalServiceID", "$DigitalServices.DigitalServiceID" },
                    { "ServiceStartDate", "$DigitalServices.ServiceStartDate" },
                    { "ServiceEndDate", "$DigitalServices.ServiceEndDate" },
                    { "NotificationUsers", "$DigitalServices.NotificationUsers" }
                })
            };

            var results = await _customersCollection.Aggregate<BsonDocument>(pipeline).ToListAsync();
            return results;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching digital services: {ex.Message}");
            return new List<BsonDocument>();
        }
    }
}
