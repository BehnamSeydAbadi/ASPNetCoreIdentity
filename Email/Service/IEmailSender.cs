namespace Email.Service;

public interface IEmailSender
{
    Task SendEmailAsync(string receiverName, string to, string subject, string content);
}
