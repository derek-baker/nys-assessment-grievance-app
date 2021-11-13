using Library.Models;
using Library.Models.Entities;
using System.Threading.Tasks;

namespace Library.Services.Auth
{
    public interface IAuthService
    {
        Task<(AuthenticationResult Result, User User)> AuthenticateAndAuthorizeUser(string userName, string password);
        
        Task<(bool IsValidSession, string UserName)> ValidateSession(Session session);
        
        Task<bool> ValidateSession(string cookieData);

        Task<(bool IsSuccess, int? Code)> GenerateSecurityCode(string userEmail);

        bool ValidateSecurityCode(int code, User user);
    }
}