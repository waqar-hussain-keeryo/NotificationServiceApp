using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        // Fetch and process all digital services
        await FetchAndProcessDigitalServices(context);
    }

    public static async Task FetchAndProcessDigitalServices(MongoDBContext context)
    {
        var allDigitalServices = await context.GetAllDigitalServicesAsync();

        if (allDigitalServices.Count == 0)
        {
            Console.WriteLine("No digital services found.");
            return;
        }

        Console.WriteLine($"Total digital services retrieved: {allDigitalServices.Count}");

        var today = DateTime.UtcNow;

        foreach (var service in allDigitalServices)
        {
            var serviceStartDate = service.GetValue("ServiceStartDate", DateTime.MinValue).ToUniversalTime();
            var serviceEndDate = service.GetValue("ServiceEndDate", DateTime.MinValue).ToUniversalTime();
            var notificationUsers = service.GetValue("NotificationUsers", new BsonArray()).AsBsonArray.Select(u => u.AsString).ToList();

            // Check if the service is expiring within the next 10 days
            var daysUntilExpiration = (serviceEndDate - today).TotalDays;

            if (daysUntilExpiration <= 10 && daysUntilExpiration >= 0)
            {
                Console.WriteLine($"Digital Service {service.GetValue("DigitalServiceID")} is expiring soon!");

                foreach (var userEmail in notificationUsers)
                {
                    var subject = "Upcoming Expiration Notification";
                    var body = $"Hello,\n\nYour digital service with ID {service.GetValue("DigitalServiceID")} is expiring on {serviceEndDate.ToShortDateString()}. Please take necessary actions.\n\nThank you.";

                    // Send email notification
                    await EmailService.SendEmailAsync(userEmail, subject, body);
                }
            }
        }
    }

}
