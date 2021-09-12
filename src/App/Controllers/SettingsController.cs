using Library.Models.DataTransferObjects;
using Library.Models.Settings;
using Library.Services.Config.UserSettings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Library.Services.Auth;
using System.Threading.Tasks;

namespace App.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserSettingsController : ControllerBase
    {
        private readonly IUserSettingsService _userSettings;
        private readonly IAuthService _auth;

        public UserSettingsController(
            IUserSettingsService userSettings,
            IAuthService auth)
        {
            _userSettings = userSettings;
            _auth = auth;
        }

        // GET: api/usersettings/getusersettings
        [HttpGet]
        public async Task<IActionResult> GetUserSettings()
        {
            var settings = await _userSettings.GetUserSettings();
            return Ok(settings ?? new UserSettings());
        }

        // POST: api/usersettings/setusersettings
        [HttpPost]
        public IActionResult SetUserSettings(SetUserSettingsParams input)
        {
            var authResult = _auth.AuthenticateAndAuthorizeUser(input.userName, input.password);
            if (!authResult.IsAuthenticated)
                return StatusCode(StatusCodes.Status403Forbidden);

            _userSettings.SetUserSettings(input.settings);
            return Ok();
        }
    }
}
