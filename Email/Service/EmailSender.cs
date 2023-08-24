using Email.Models;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;

namespace Email.Service;

internal class EmailSender : IEmailSender
{
    private readonly EmailOptions _emailOptions;
    public EmailSender(EmailOptions emailOptions) => _emailOptions = emailOptions;

    public async Task SendEmailAsync(string receiverName, string to, string subject, string content)
    {
        var emailMessage = CreateEmailMessage(receiverName, to, subject, content);

        await SendAsync(emailMessage);
    }

    private MimeMessage CreateEmailMessage(string receiverName, string to, string subject, string content)
    {
        var emailMessage = new MimeMessage();

        emailMessage.From.Add(new MailboxAddress("Sender", _emailOptions.From));
        emailMessage.To.Add(new MailboxAddress(receiverName, to));
        emailMessage.Subject = subject;
        emailMessage.Body = new TextPart(TextFormat.Text) { Text = content };

        return emailMessage;
    }

    private async Task SendAsync(MimeMessage mailMessage)
    {
        using var client = new SmtpClient();
        try
        {
            client.Connect(_emailOptions.SmtpServer, _emailOptions.Port, true);
            client.AuthenticationMechanisms.Remove("XOAUTH2");
            client.Authenticate(_emailOptions.UserName, _emailOptions.Password);
            await client.SendAsync(mailMessage);
        }
        catch
        {
            throw;
        }
        finally
        {
            client.Disconnect(true);
            client.Dispose();
        }
    }
}
