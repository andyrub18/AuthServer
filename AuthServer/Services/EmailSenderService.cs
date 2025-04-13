using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace AuthServer.Services;

public class EmailSenderService : IEmailSender
{
    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var client = new SmtpClient("sandbox.smtp.mailtrap.io", 2525)
        {
            Credentials = new NetworkCredential("c18ba624921280", "4247259d89485c"),
            EnableSsl = true,
        };
        client.Send("from@example.com", email, subject, htmlMessage);
    }
}