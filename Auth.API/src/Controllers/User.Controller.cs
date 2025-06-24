using Auth.API.DTO;
using Auth.API.Models;
using Auth.API.Repository;
using Microsoft.AspNetCore.Mvc;
using Auth.API.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Auth.API.Controllers;


[ApiController]
[Route("user")]
public class UserController : Controller
{
    private readonly IUserRepository userRepository;
    private readonly INotificationService notificationService;
    private readonly IPasswordService passwordService;
    private readonly TokenGenerator _tokenGenerator;

    private readonly ITokenService _tokenService;

    public UserController(IUserRepository userRepository, INotificationService notificationService, IPasswordService passwordService, TokenGenerator tokenGenerator, ITokenService tokenService)
    {
        this.userRepository = userRepository;
        this.notificationService = notificationService;
        this.passwordService = passwordService;
        this._tokenGenerator = tokenGenerator;
        this._tokenService = tokenService;
    }


    [HttpGet("")]
    [Authorize(Policy = "Authenticated")]
    [Authorize(Policy = "Admin")]
    public IActionResult Get()
    {
        try
        {
            var users = userRepository.GetAll();
            return Ok(users);
        }
        catch (Exception Err)
        {
            return BadRequest(new { message = Err.Message.ToString() });
        }
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "Authenticated")]
    [Authorize(Policy = "Admin")]
    public IActionResult GetById(int id)
    {
        try
        {
            var users = userRepository.GetById(id);
            return Ok(users);
        }
        catch (Exception Err)
        {
            return BadRequest(new { message = Err.Message.ToString() });
        }
    }

    [Authorize]
    [HttpGet("validate-token")]
    public IActionResult ValidateToken()
    {
        return Ok(new { message = "Token válido." });
    }



    [Authorize]
    [HttpGet("get-user-by-token")]
    public IActionResult GetUserByToken()
    {
        var user = HttpContext.User;

        var name = user.FindFirst(ClaimTypes.Name)?.Value;
        var email = user.FindFirst(ClaimTypes.Email)?.Value;
        var role = user.FindFirst(ClaimTypes.Role)?.Value;

        return Ok(new
        {
            name,
            email,
            role
        });
    }



    [HttpPost]
    public IActionResult Create([FromBody] UserInsertDto userData)
    {
        try
        {

            var userCreated = userRepository.Create(userData);

            var messageText = "<h4>Cadastro realizado no AuthApi </h4>";
            messageText += "<p> Olá: " + userData.name;
            messageText += "<p> Boas vindas ao Micro-Service-Auth</p>";

            Message message = new Message
            {
                Title = "Shop Trybe - Cadastro realizado",
                Text = messageText,
                MailTo = userData.email
            };
            notificationService.Send(message);


            var token = _tokenGenerator.Generate(userCreated);

            return Created("", new { token });
        }
        catch (Exception Err)
        {
            Console.WriteLine(Err.Message);
            return BadRequest(new { message = Err.Message.ToString() });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "Admin")]
    [Authorize(Policy = "Authenticated")]
    public async Task<IActionResult> Remove(int id)
    {
        try
        {
            await userRepository.Remove(id);
            return NoContent();
        }
        catch (Exception Err)
        {
            return BadRequest(new { message = Err.Message });
        }
    }


    [HttpPatch("recover-password")]
    public async Task<IActionResult> RecoverPassword([FromBody] UserRecoveryPasswordDto userData)
    {
        try
        {
            var response = await passwordService.ProcessPasswordRecovery(userData);
            return Ok(response);
        }
        catch (Exception Err)
        {
            return BadRequest(new { message = Err.Message.ToString() });
        }
    }

    [HttpPatch("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] UserResetPasswordDto userData, [FromQuery] string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return BadRequest(new { message = "Token de redefinição de senha é obrigatório." });
        }

        try
        {
            var response = await passwordService.ProcessPasswordReset(userData, token);
            return Ok(response);
        }
        catch (Exception Err)
        {
            return BadRequest(new { message = Err.Message.ToString() });
        }
    }


}