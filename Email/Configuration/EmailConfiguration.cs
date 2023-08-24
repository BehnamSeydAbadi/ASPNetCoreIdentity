using Email.Models;
using Email.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Email.Configuration;

public static class EmailConfiguration
{
    public static void ConfigureEmail(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        var emailOptions = configuration.GetSection("EmailConfiguration")
            .Get<EmailOptions>();

        serviceCollection.AddSingleton(emailOptions);
        serviceCollection.AddScoped<IEmailSender, EmailSender>();
    }
}
