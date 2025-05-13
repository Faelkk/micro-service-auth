// Auth.API.Repository/UserRepository.cs
using Auth.API.DTO;
using Auth.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


namespace Auth.API.Repository;

public class UserRepository : IUserRepository
{
    private readonly IDatabaseContext databaseContext;
    private readonly PasswordHasher<User> passwordHasher;

    public UserRepository(IDatabaseContext databaseContext)
    {
        this.databaseContext = databaseContext;
        this.passwordHasher = new PasswordHasher<User>();
    }

    public IEnumerable<UserResponseDto> GetAll()
    {
        var users = databaseContext.Users
       .Select(user => new UserResponseDto { UserId = user.UserId, Email = user.Email, Name = user.Name })
       .ToList();

        if (!users.Any())
        {
            throw new Exception("No users found");
        }

        return users;

    }

    public UserResponseDto GetById(int userId)
    {
        var user = databaseContext.Users
            .Where(user => user.UserId == userId)
            .Select(user => new UserResponseDto { UserId = user.UserId, Email = user.Email, Name = user.Name })
            .FirstOrDefault();

        if (user == null)
        {
            throw new Exception("User not found");
        }

        return user;
    }

    public UserResponseDto Create(UserInsertDto userData)
    {
        var existingUserWithEmail = databaseContext.Users.Any(user => user.Email == userData.email);
        if (existingUserWithEmail)
        {
            throw new Exception("Email already in use");
        }

        var newUser = new User { Email = userData.email, Name = userData.name, Role = "Client" };
        newUser.Password = passwordHasher.HashPassword(newUser, userData.password);

        databaseContext.Users.Add(newUser);
        databaseContext.SaveChanges();

        return new UserResponseDto { UserId = newUser.UserId, Name = newUser.Name, Email = newUser.Email, Role = newUser.Role };
    }

    public UserResponseDto Login(LoginDto userLoginData)
    {
        var user = databaseContext.Users.FirstOrDefault(u => u.Email == userLoginData.email);
        if (user == null)
        {
            throw new Exception("Not found any user for this E-Mail");
        }

        var result = passwordHasher.VerifyHashedPassword(user, user.Password, userLoginData.password);
        if (result == PasswordVerificationResult.Failed)
        {
            throw new Exception("Invalid password");
        }

        return new UserResponseDto { UserId = user.UserId, Name = user.Name, Email = user.Email, Role = user.Role };
    }

    public async Task Remove(int id)
    {
        var user = await databaseContext.Users.FirstOrDefaultAsync(u => u.UserId == id);
        if (user == null)
        {
            throw new Exception("User not found");
        }

        databaseContext.Users.Remove(user);
        await databaseContext.SaveChangesAsync();
    }


    public async Task<User> GetUserByEmail(string email)
    {
        return await databaseContext.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<PasswordResetToken> CreatePasswordResetToken(User user)
    {
        var token = Guid.NewGuid().ToString();
        var expirationDate = DateTime.UtcNow.AddHours(24);

        var resetToken = new PasswordResetToken
        {
            UserId = user.UserId,
            Token = token,
            ExpirationDate = expirationDate
        };

        databaseContext.PasswordResetTokens.Add(resetToken);
        await databaseContext.SaveChangesAsync();

        return resetToken;
    }

    public async Task<PasswordResetToken> GetPasswordResetToken(string token)
    {
        return await databaseContext.PasswordResetTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == token && t.ExpirationDate > DateTime.UtcNow);
    }

    public async Task RemovePasswordResetToken(PasswordResetToken token)
    {
        databaseContext.PasswordResetTokens.Remove(token);
        await databaseContext.SaveChangesAsync();
    }

    public async Task UpdateUserPassword(User user, string newPassword)
    {
        user.Password = passwordHasher.HashPassword(user, newPassword);
        databaseContext.Users.Update(user);
        await databaseContext.SaveChangesAsync();
    }


}