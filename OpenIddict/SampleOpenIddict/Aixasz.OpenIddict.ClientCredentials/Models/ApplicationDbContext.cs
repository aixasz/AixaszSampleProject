using Microsoft.EntityFrameworkCore;

namespace Aixasz.OpenIddict.ClientCredentials.Models;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
}