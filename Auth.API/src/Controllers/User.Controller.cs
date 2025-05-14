// Auth.API.Controllers/User/UserController.cs
using Auth.API.DTO;
using Auth.API.Models;
using Auth.API.Repository;
using Microsoft.AspNetCore.Mvc;
using Auth.API.Services;
using Microsoft.AspNetCore.Authorization;

namespace Auth.API.Controllers;


[ApiController]
[Route("user")]
public class UserController : Controller
{
    private readonly IUserRepository userRepository;
    private readonly INotificationService notificationService;
    private readonly IPasswordService passwordService;
    private readonly TokenGenerator _tokenGenerator;

    public UserController(IUserRepository userRepository, INotificationService notificationService, IPasswordService passwordService, TokenGenerator tokenGenerator)
    {
        this.userRepository = userRepository;
        this.notificationService = notificationService;
        this.passwordService = passwordService;
        this._tokenGenerator = tokenGenerator;
    }

    [HttpGet("teste")]

    public IActionResult Teste()
    {
        return Ok(new { message = "Teste" });
    }


    [HttpGet("")]
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