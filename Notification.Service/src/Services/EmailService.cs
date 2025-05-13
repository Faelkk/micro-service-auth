namespace Notification.Service.Service;
using System.Text;
using System.Net;
using System.Net.Mail;
using Notification.Service.Models;

public class EmailService : MailMessage
{
    private static readonly string EMAIL_HOST = Environment.GetEnvironmentVariable("EMAIL_HOST");
    private static readonly string EMAIL_FROM = Environment.GetEnvironmentVariable("EMAIL_FROM");
    private static readonly string EMAIL_PASSWORD = Environment.GetEnvironmentVariable("EMAIL_PASSWORD");

    public static void Send(Message message)
    {
        try
        {
            using (var msgEmail = new MailMessage())
            {
                msgEmail.From = new MailAddress(EMAIL_FROM);
                msgEmail.To.Add(message.MailTo);
                msgEmail.Subject = message.Title;
                msgEmail.Body = message.Text;
                msgEmail.IsBodyHtml = true;


                using (var smtpClient = new SmtpClient(EMAIL_HOST, 587))
                {
                    smtpClient.Credentials = new NetworkCredential(EMAIL_FROM, EMAIL_PASSWORD);
                    smtpClient.EnableSsl = true;
                    smtpClient.Send(msgEmail);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao enviar email: {ex.Message}");
        }
    }

}