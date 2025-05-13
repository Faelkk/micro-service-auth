

using Microsoft.AspNetCore.Mvc;
using Auth.API.Repository;
using Auth.API.DTO;
using Auth.API.Models;
using Auth.API.Services;


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
    public IActionResult Login([FromBody] LoginDto userLoginData)
    {

        try
        {
            var userLogged = userRepository.Login(userLoginData);

            var UserAgent = HttpContext.Request.Headers.UserAgent;
            var dateTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            var messageText = "<h4>Novo login realizado no ShopTrybe </h4>";
            messageText += "<p> Origem: " + UserAgent;
            messageText += "<br /> Data: " + dateTime + "</p>";
            messageText += "<p> Caso não reconheça este login, revise seus dados de autenticação.</p>";

            Message message = new Message
            {
                Title = "Shop Trybe - Novo login",
                Text = messageText,
                MailTo = userLoginData.email
            };
            notificationService.Send(message);

            var token = _tokenGenerator.Generate(userLogged);

            return Ok(new { token });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message.ToString() });
        }
    }

}