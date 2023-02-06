using Microsoft.EntityFrameworkCore;

namespace Aixasz.OpenIddict.Server.Models;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
}