using Library.Models;
using Library.Models.Entities;
using System.Threading.Tasks;

namespace Library.Services.Auth
{
    public interface IAuthService
    {
        Task<AuthenticationResult> AuthenticateAndAuthorizeUser(string userName, string password);
        Task<bool> ValidateSession(Session session);
        Task<bool> ValidateSession(string sessionCookieValue);
    }
}