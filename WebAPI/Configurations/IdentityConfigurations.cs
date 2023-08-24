using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebAPI.DataAccess;

namespace WebAPI.Configurations;

internal static class IdentityConfigurations
{
    internal static void ConfigureIdentity(this IServiceCollection servicesCollection)
    {
        servicesCollection.AddDbContext<CustomDbContext>(options => options.UseInMemoryDatabase("Identity_Db"));
        servicesCollection.AddIdentity<User, Role>(options =>
        {
            options.Password.RequiredLength = 4;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;

            //options.Lockout.MaxFailedAccessAttempts = 5;
            //options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);

            options.User.RequireUniqueEmail = true;

            options.SignIn.RequireConfirmedEmail = true;
        }).AddEntityFrameworkStores<CustomDbContext>()
          .AddDefaultTokenProviders();
    }
}
