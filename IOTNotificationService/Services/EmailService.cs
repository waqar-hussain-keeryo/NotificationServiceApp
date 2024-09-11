using System.Net.Mail;
using System.Net.Sockets;
using System.Net;

public static class EmailService
{
    private static readonly string smtpServer = "smtp.gmail.com";
    private static readonly int smtpPort = 587;
    private static readonly string smtpUser = "dotnettesting60@gmail.com";
    private static readonly string smtpPassword = "pyzvbigfwuhkyxek";

    public static async Task SendEmail(string email, string subject, string body)
    {
        try
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress(smtpUser),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(email);

            using (var smtpClient = new SmtpClient(smtpServer, smtpPort))
            {
                smtpClient.Credentials = new NetworkCredential(smtpUser, smtpPassword);
                smtpClient.EnableSsl = true;

                await smtpClient.SendMailAsync(mailMessage);
                Console.WriteLine($"Email sent to {email} successfully.");
            }
        }
        catch (SmtpException ex)
        {
            Console.WriteLine($"SMTP error: {ex.Message}");
        }
        catch (SocketException ex)
        {
            Console.WriteLine($"Socket error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to send email to {email}. Error: {ex.Message}");
        }
    }
}
