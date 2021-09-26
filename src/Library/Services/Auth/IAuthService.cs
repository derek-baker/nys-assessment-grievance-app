using Library.Models;
using System.Threading.Tasks;

namespace Library.Services.Auth
{
    public interface IAuthService
    {
        Task<AuthenticationResult> AuthenticateAndAuthorizeUser(string userName, string password);
        Task<bool> ValidateSession(string cookieData);
    }
}