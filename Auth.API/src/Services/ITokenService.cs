
using System.Security.Claims;

namespace Auth.API.Services;

public interface ITokenService
{
    ClaimsPrincipal? GetPrincipalFromToken(string token);
}