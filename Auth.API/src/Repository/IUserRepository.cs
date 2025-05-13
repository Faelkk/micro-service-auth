namespace Auth.API.Repository;
using Auth.API.DTO;

using Auth.API.Models;

public interface IUserRepository
{
    IEnumerable<UserResponseDto> GetAll();
    UserResponseDto GetById(int userId);
    UserResponseDto Create(UserInsertDto userData);
    UserResponseDto Login(LoginDto userLoginData);

    Task Remove(int userId);

    Task<PasswordResetToken> CreatePasswordResetToken(User user);
    Task<PasswordResetToken> GetPasswordResetToken(string token);
    Task RemovePasswordResetToken(PasswordResetToken token);
    Task UpdateUserPassword(User user, string newPassword);
    Task<User> GetUserByEmail(string email);
}