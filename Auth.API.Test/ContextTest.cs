using Microsoft.EntityFrameworkCore;
using Auth.API.Models;
using Auth.API.Repository;

namespace AuthApi.Test;

public class ContextTest : DbContext, IDatabaseContext
{
    public ContextTest(DbContextOptions<ContextTest> options) : base(options)
    { }

    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

    public Task<int> SaveChangesAsync()
    {
        return base.SaveChangesAsync(CancellationToken.None);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return base.SaveChangesAsync(cancellationToken);
    }
}
