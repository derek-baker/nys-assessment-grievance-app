using Library.Models.DataTransferObjects;
using Library.Models.DataTransferObjects.Output;
using Microsoft.AspNetCore.Mvc;
using Library.Services.Auth;
using System.Threading.Tasks;
using System.Text.Json;
using App.Services.Auth;
using Library.Models.Entities;
using Library.Email;

namespace App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;
        private readonly IEmailClient _email;

        public AuthController(
            IAuthService auth,
            IEmailClient email)
        {
            _auth = auth;
            _email = email;
        }

        // POST: api/Auth
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] UserAuthInfo userInfo)
        {
            var (Result, User) = await _auth.AuthenticateAndAuthorizeUser(userInfo.UserName, userInfo.Password);
            var isCodeValid = Result.IsAuthenticated
                && _auth.ValidateSecurityCode(userInfo.SecurityCode, User);

            var authAttemptResponse = new AuthResponse(userInfo.UserName, Result, isCodeValid);            
            if (authAttemptResponse.AuthResult.IsAuthenticated)
            {
                var cookieOptions = CookieFactoryService.BuildCookieOptions(Result.Session.ValidUntil);
                base.Response.Cookies.Append(
                    nameof(Session),
                    JsonSerializer.Serialize(Result.Session),
                    cookieOptions
                );
            }
            return Ok(authAttemptResponse);
        }

        [HttpPost("ValidateSession")]
        public async Task<IActionResult> ValidateSession([FromBody] Session session)
        {
            var (IsValidSession, UserName) = await _auth.ValidateSession(session);
            return Ok(new { IsValidSession, UserName });
        }

        [HttpPost("SendSecurityCode")]
        public async Task<IActionResult> SendSecurityCode([FromQuery] string userEmail)
        {
            var (IsSuccess, Code) = await _auth.GenerateSecurityCode(userEmail);
            
            if (IsSuccess)
                await _email.SendSecurityCodeEmail(userEmail, (int)Code);

            return Ok();
        }
    }
}
