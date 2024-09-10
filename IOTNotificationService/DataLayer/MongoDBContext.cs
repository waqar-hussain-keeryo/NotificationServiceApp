using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

    public async Task<List<BsonDocument>> GetNotificationUsersAsync(Guid digitalServiceId)
    {
        try
        {
            var filter = Builders<BsonDocument>.Filter.Eq("DigitalServiceID", digitalServiceId);
            var digitalService = await _digitalServicesCollection.Find(filter).FirstOrDefaultAsync();

            if (digitalService != null)
            {
                var notificationUsersArray = digitalService.GetValue("NotificationUsers", new BsonArray()).AsBsonArray;
                return notificationUsersArray.Select(doc => doc.AsBsonDocument).ToList();
            }

            return new List<BsonDocument>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching notification users: {ex.Message}");
            return new List<BsonDocument>();
        }
    }
}
