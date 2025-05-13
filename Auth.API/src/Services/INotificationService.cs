namespace Auth.API.Services;
using Auth.API.Models;


public interface INotificationService
{
    Task Send(Message notification);
}
