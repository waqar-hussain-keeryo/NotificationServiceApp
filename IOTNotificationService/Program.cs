using MongoDB.Bson;

public class Program
{
    public static async Task Main(string[] args)
    {
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

    private static async Task FetchAndProcessDigitalServices(MongoDBContext context)
    {
        var allDigitalServices = await context.GetAllDigitalServicesAsync();

        if (allDigitalServices.Count == 0)
        {
            Console.WriteLine("No digital services found.");
            return;
        }

        var today = DateTime.UtcNow;

        // Group digital services by month and year of their ServiceEndDate
        var groupedByMonth = allDigitalServices
            .GroupBy(service => service.GetValue("ServiceEndDate", DateTime.MinValue).ToUniversalTime().ToString("yyyy-MM"))
            .OrderByDescending(g => g.Key)  // Sort by month-year descending
            .ToList();

        if (!groupedByMonth.Any())
        {
            Console.WriteLine("No digital services with valid end dates found.");
            return;
        }

        // Select the most recent group (latest month)
        var latestGroup = groupedByMonth.First();

        // Flag to check if any notifications are sent
        bool notificationsSent = false;

        // Process each digital service in the latest group
        foreach (var service in latestGroup)
        {
            var serviceId = service.GetValue("DigitalServiceID", Guid.Empty);
            var serviceStartDate = service.GetValue("ServiceStartDate", DateTime.MinValue).ToUniversalTime();
            var serviceEndDate = service.GetValue("ServiceEndDate", DateTime.MinValue).ToUniversalTime();
            var notificationUsers = service.GetValue("NotificationUsers", new BsonArray()).AsBsonArray.Select(u => u.AsString).ToList();

            // Check if there are no users to notify
            if (!notificationUsers.Any())
            {
                Console.WriteLine($"Digital Service {serviceId} has no users to notify.");
                continue;  // Skip to the next service
            }

            // Check if the service is expiring within the next 10 days
            var daysUntilExpiration = (serviceEndDate - today).TotalDays;
            var daysSinceExpiration = (today - serviceEndDate).TotalDays;

            if (daysUntilExpiration <= 10 && daysUntilExpiration >= 0)
            {
                Console.WriteLine($"Digital Service {serviceId} is expiring soon!");

                foreach (var userEmail in notificationUsers)
                {
                    var subject = "Upcoming Expiration Notification";
                    var body = $"Hello,\n\nYour digital service expiring on {serviceEndDate.ToShortDateString()}. Please take necessary actions.\n\nThank you.";

                    // Send email notification
                    await EmailService.SendEmail(userEmail, subject, body);
                }

                notificationsSent = true;
            }
            else if (daysSinceExpiration > 0 && daysSinceExpiration <= 10)
            {
                Console.WriteLine($"Digital Service {serviceId} has expired!");

                foreach (var userEmail in notificationUsers)
                {
                    var subject = "Service Expired Notification";
                    var body = $"Hello,\n\nYour digital service expired on {serviceEndDate.ToShortDateString()}. Please review your service status.\n\nThank you.";

                    // Send email notification
                    await EmailService.SendEmail(userEmail, subject, body);
                }

                notificationsSent = true;
            }
        }

        // Inform if no expiring or expired services were found
        if (!notificationsSent)
        {
            Console.WriteLine("No expiring or expired digital services found.");
        }
    }
}