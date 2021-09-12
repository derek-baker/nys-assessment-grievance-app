using Library.Models;

namespace Library.Services.Auth
{
    public interface IAuthService
    {
        AuthenticationResult AuthenticateAndAuthorizeUser(string userName, string password);
    }
}