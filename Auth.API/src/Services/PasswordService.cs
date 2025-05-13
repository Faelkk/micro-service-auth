using Auth.API.DTO;
using Auth.API.Models;
using Auth.API.Repository;
using System;
using System.Threading.Tasks;

namespace Auth.API.Services;


public class PasswordService : IPasswordService
{
    private readonly IUserRepository userRepository;
    private readonly INotificationService notificationService;

    public PasswordService(IUserRepository userRepository, INotificationService notificationService)
    {
        this.userRepository = userRepository;
        this.notificationService = notificationService;
    }

    public async Task<UserRecoveryPasswordResponseDto> ProcessPasswordRecovery(UserRecoveryPasswordDto recoveryDto)
    {
        var user = await userRepository.GetUserByEmail(recoveryDto.email);
        if (user == null)
        {
            return new UserRecoveryPasswordResponseDto { Message = "If an account with that email exists, we've sent a password reset link." };
        }
        var token = await userRepository.CreatePasswordResetToken(user);
        var resetLink = $"SEU_FRONTEND_URL/reset-password?token={token.Token}";

        string messageText = $"<h4>Recuperação de Senha</h4><p>Olá, {user.Name},</p><p>Você solicitou a recuperação da sua senha. Clique no link abaixo para redefinir sua senha:</p><p><a href='{resetLink}'>{resetLink}</a></p><p>Este link é válido por 24 horas.</p>";

        Message message = new Message
        {
            Title = "Recuperação de Senha",
            Text = messageText,
            MailTo = user.Email
        };

        await notificationService.Send(message);

        return new UserRecoveryPasswordResponseDto { Message = "If an account with that email exists, we've sent a password reset link." };
    }

    public async Task<UserResetPasswordResponseDto> ProcessPasswordReset(UserResetPasswordDto resetDto, string token)
    {
        var resetToken = await userRepository.GetPasswordResetToken(token);
        if (resetToken == null)
        {
            return new UserResetPasswordResponseDto { Message = "Invalid or expired reset token." };
        }

        var user = resetToken.User;
        await userRepository.UpdateUserPassword(user, resetDto.password);
        await userRepository.RemovePasswordResetToken(resetToken);

        return new UserResetPasswordResponseDto { Message = "Password reset successfully." };
    }
}
