using Microsoft.EntityFrameworkCore;
using Auth.API.Models;
using System.Threading.Tasks;

namespace Auth.API.Context;

public class DatabaseContext : DbContext, IDatabaseContext
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { } // <--- Updated constructor

    public int SaveChanges()
    {
        return base.SaveChanges();
    }

    public Task<int> SaveChangesAsync()
    {
        return base.SaveChangesAsync();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var server = Environment.GetEnvironmentVariable("DBSERVER");
        var database = Environment.GetEnvironmentVariable("DBNAME");
        var dbuser = Environment.GetEnvironmentVariable("DBUSER");
        var dbpass = Environment.GetEnvironmentVariable("DBPASSWORD");
        var connectionString = $"Server={server};Database={database};User={dbuser};Password={dbpass};TrustServerCertificate=True";

        optionsBuilder.UseSqlServer(connectionString);

    }
}