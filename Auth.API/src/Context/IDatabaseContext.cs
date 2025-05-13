using Microsoft.EntityFrameworkCore;
using Auth.API.Models;
using System.Threading.Tasks;

public interface IDatabaseContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
    public int SaveChanges();
    public Task<int> SaveChangesAsync(); // Add this line
}
