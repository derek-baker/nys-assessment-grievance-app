using Library.Models.DataTransferObjects;
using Library.Models.DataTransferObjects.Output;
using Microsoft.AspNetCore.Mvc;
using Library.Services.Auth;
using System.Diagnostics.Contracts;

namespace App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        // POST: api/Auth
        [HttpPost]
        //[AutoValidateAntiforgeryToken]
        public IActionResult Post([FromBody] UserAuthInfo userInfo)
        {
            Contract.Requires(userInfo != null);

            // TODO: Dependency Injection
            var authSvc = new AuthService();
            var authResult = authSvc.AuthenticateAndAuthorizeUser(userInfo.userName, userInfo.password);
            var authAttemptResponse = new AuthResponse(userInfo.userName, authResult);
            
            return Ok(authAttemptResponse);
        }
    }
}
