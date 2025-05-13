using Auth.API.DTO;
using System.Threading.Tasks;

namespace Auth.API.Services;


public interface IPasswordService
{
    Task<UserRecoveryPasswordResponseDto> ProcessPasswordRecovery(UserRecoveryPasswordDto recoveryDto);
    Task<UserResetPasswordResponseDto> ProcessPasswordReset(UserResetPasswordDto resetDto, string token);
}