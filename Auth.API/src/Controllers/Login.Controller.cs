using Microsoft.AspNetCore.Mvc;
using Auth.API.Repository;
using Auth.API.DTO;
using Auth.API.Models;
using Auth.API.Services;
using Google.Apis.Auth;
using Auth.API.Context;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Auth.API.Controllers;

[ApiController]
[Route("login")]
public class LoginController : Controller
{
    private readonly IUserRepository userRepository;
    private readonly INotificationService notificationService;
    private readonly TokenGenerator _tokenGenerator;

    public LoginController(IUserRepository userRepository, INotificationService notificationService, TokenGenerator tokenGenerator)
    {
        this.userRepository = userRepository;
        this.notificationService = notificationService;
        this._tokenGenerator = tokenGenerator;
    }

    [HttpPost]
    public async Task<IActionResult> Login([FromBody] LoginDto userLoginData)
    {
        try
        {
            var userLogged = userRepository.Login(userLoginData);


            SendLoginNotification(userLoginData.email);

            if (userLogged.IsTwoFactorEnabled)
            {
                var code = await userRepository.GenerateAndSaveTwoFactorCode(userLogged.Email);

                await notificationService.Send(new Message
                {
                    Title = "Código de verificação",
                    Text = $"Seu código de verificação é: <b>{code}</b>",
                    MailTo = userLogged.Email
                });

                return Ok(new
                {
                    message = "Autenticação de dois fatores habilitada. Código enviado.",
                    email = userLogged.Email
                });
            }


            var token = _tokenGenerator.Generate(userLogged);

            return Ok(new { token });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("login-google")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDto googleLoginDto)
    {
        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(googleLoginDto.IdToken);

            var user = await userRepository.GetUserByEmail(payload.Email);

            if (user == null)
            {
                var userName = string.IsNullOrEmpty(payload.Name) ? "Unknown" : payload.Name;

                var newUser = new UserInsertDto
                {
                    email = payload.Email,
                    name = userName,
                    password = Guid.NewGuid().ToString(),
                };

                var createdUser = userRepository.Create(newUser);
                var tokenNew = _tokenGenerator.Generate(createdUser);

                SendLoginNotification(payload.Email);

                return Ok(new { token = tokenNew });
            }

            var userDto = new UserResponseDto
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Role = "Client"
            };

            var token = _tokenGenerator.Generate(userDto);

            SendLoginNotification(payload.Email);

            return Ok(new { token });
        }
        catch (Exception ex)
        {
            var innerException = ex.InnerException != null ? ex.InnerException.Message : "No inner exception";

            return BadRequest(new { message = "Erro ao autenticar com o Google: " + ex.Message, innerException });
        }
    }



    [HttpPatch("enable-2fa")]
    [Authorize(Policy = "Authenticated")]
    public async Task<IActionResult> EnableTwoFa([FromBody] EnableTwoFaDto enableTwoFaDto)
    {
        try
        {
            var updatedUser = await userRepository.EnableTwoFactorCode(enableTwoFaDto.Email, enableTwoFaDto.IsTwoFactorEnabled);

            return Ok(new
            {
                message = "Autenticação de dois fatores atualizada",
                user = updatedUser
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }




    [HttpPost("verify-2fa")]
    public async Task<IActionResult> VerifyTwoFactorCode([FromBody] TwoFactorDto dto)
    {
        try
        {
            var user = await userRepository.VerifyTwoFactorCode(dto.Email, dto.Code);
            var token = _tokenGenerator.Generate(user);
            return Ok(new { token });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }



    private void SendLoginNotification(string email)
    {
        var userAgent = HttpContext.Request.Headers.UserAgent;
        var dateTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

        var messageText = "<h4>Novo login realizado no ShopTrybe</h4>";
        messageText += $"<p>Origem: {userAgent}<br />Data: {dateTime}</p>";
        messageText += "<p>Caso não reconheça este login, revise seus dados de autenticação.</p>";

        Message message = new Message
        {
            Title = "Shop Trybe - Novo login",
            Text = messageText,
            MailTo = email
        };

        notificationService.Send(message);
    }
}