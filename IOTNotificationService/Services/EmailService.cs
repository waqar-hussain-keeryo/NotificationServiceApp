using System.Threading.Tasks;

public static class EmailService
{
    public static Task SendEmailAsync(string to, string subject, string body)
    {
        // Replace this with actual email sending logic.
        // For now, it just prints to the console.
        Console.WriteLine($"Sending email to {to}");
        Console.WriteLine($"Subject: {subject}");
        Console.WriteLine($"Body: {body}");
        return Task.CompletedTask;
    }
}
