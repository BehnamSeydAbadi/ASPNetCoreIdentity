namespace Email.Models;

public record EmailOptions
{
    public string From { get; init; }
    public string SmtpServer { get; init; }
    public int Port { get; init; }
    public string UserName { get; init; }
    public string Password { get; init; }
}
