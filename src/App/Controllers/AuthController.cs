using Library.Models.DataTransferObjects;
using Library.Models.DataTransferObjects.Output;
using Microsoft.AspNetCore.Mvc;
using Library.Services.Auth;
using System.Threading.Tasks;
using System.Text.Json;
using App.Services.Auth;
using Library.Models.Entities;

namespace App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;

        public AuthController(IAuthService auth)
        {
            _auth = auth;
        }

        // POST: api/Auth
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] UserAuthInfo userInfo)
        {
            var authResult = await _auth.AuthenticateAndAuthorizeUser(userInfo.userName, userInfo.password);
            
            var authAttemptResponse = new AuthResponse(userInfo.userName, authResult);            
            if (authAttemptResponse.AuthResult.IsAuthenticated)
            {
                var cookieOptions = CookieFactoryService.BuildCookieOptions(authResult.Session.ValidUntil);
                base.Response.Cookies.Append(
                    nameof(Session),
                    JsonSerializer.Serialize(authResult.Session),
                    cookieOptions
                );
            }
            return Ok(authAttemptResponse);
        }

        [HttpPost("ValidateSession")]
        public async Task<IActionResult> ValidateSession([FromBody] Session session)
        {
            var isValidSession = await _auth.ValidateSession(session);
            return Ok(new { IsValid = isValidSession });
        }
    }
}
