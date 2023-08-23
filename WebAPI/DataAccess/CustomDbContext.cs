using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace WebAPI.DataAccess;

public class CustomDbContext : IdentityDbContext
{
    public CustomDbContext(DbContextOptions options) : base(options) { }
}
