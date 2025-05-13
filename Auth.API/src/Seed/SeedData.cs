using Auth.API.Models;
using Microsoft.AspNetCore.Identity;

namespace Auth.API.Data.Seed
{
    public class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider, UserManager<User> userManager)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();


            var roleExist = await roleManager.RoleExistsAsync("Admin");
            if (!roleExist)
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            var user = await userManager.FindByEmailAsync("admin@domain.com");

            if (user == null)
            {
                user = new User
                {
                    Name = "Admin",
                    Email = "admin@gmail.com",
                    Role = "Admin",
                    Password = "admin"
                };

                var result = await userManager.CreateAsync(user, "Admin@123");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Admin");
                }
            }
        }
    }
}
