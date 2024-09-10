using MongoDB.Bson;

public class Program
{
    public static async Task Main(string[] args)
    {
        // Replace with your actual database and collection names
        var context = new MongoDBContext("IOTDB", "Customers", "DigitalServices");

        // Test MongoDB connection
        bool connectionTest = await context.TestConnectionAsync();
        if (!connectionTest)
        {
            Console.WriteLine("Failed to connect to MongoDB.");
            return;
        }

        // Fetch and process all customers
        await FetchAndProcessCustomers(context);
    }

    public static async Task FetchAndProcessCustomers(MongoDBContext context)
    {
        var allCustomers = await context.GetAllCustomersAsync();

        if (allCustomers.Count == 0)
        {
            Console.WriteLine("No customers found.");
            return;
        }

        Console.WriteLine($"Total customers retrieved: {allCustomers.Count}");

        foreach (var customer in allCustomers)
        {
            // Extract digital services for each customer
            var digitalServices = customer.GetValue("DigitalServices", new BsonArray()).AsBsonArray;

            foreach (var service in digitalServices)
            {
                // Assuming DigitalServiceID is stored as a string in the MongoDB document
                var digitalServiceIdString = service.AsBsonDocument.GetValue("DigitalServiceID").AsString;
                if (!Guid.TryParse(digitalServiceIdString, out Guid digitalServiceId))
                {
                    Console.WriteLine($"Invalid GUID format: {digitalServiceIdString}");
                    continue;
                }

                // Fetch notification users for this digital service
                var notificationUsers = await context.GetNotificationUsersAsync(digitalServiceId);

                // Send emails to notification users
                foreach (var user in notificationUsers)
                {
                    var email = user.GetValue("Email").AsString;
                    var name = user.GetValue("Name").AsString;
                    var subject = "Notification Subject";
                    var body = $"Hello {name}, this is a notification email.";
                    await EmailService.SendEmailAsync(email, subject, body);
                }
            }
        }
    }
}
