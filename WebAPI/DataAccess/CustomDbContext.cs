using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace WebAPI.DataAccess;

public class CustomDbContext : IdentityDbContext<User, Role, Guid>
{
    public CustomDbContext(DbContextOptions<CustomDbContext> options) : base(options) { }
}
