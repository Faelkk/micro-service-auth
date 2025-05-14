
using Auth.API.Context;
using Auth.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Auth.API.Data
{
    public static class DatabaseSeeder
    {
        public static void ApplyMigrationsAndSeed(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();


            db.Database.Migrate();


            if (!db.Users.Any(u => u.Email == "admin@example.com"))
            {
                var admin = new User
                {
                    Name = "Admin",
                    Email = "admin@example.com",
                    Role = "Admin"
                };

                var hasher = new PasswordHasher<User>();
                admin.Password = hasher.HashPassword(admin, "admin123");

                db.Users.Add(admin);
                db.SaveChanges();
            }
        }
    }
}
