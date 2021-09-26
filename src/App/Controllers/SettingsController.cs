using Library.Models.DataTransferObjects;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Library.Models.Entities;
using Library.Services.Clients.Database.Repositories;

namespace App.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserSettingsController : ControllerBase
    {
        private readonly UserSettingsRepository _userSettings;
        
        public UserSettingsController(UserSettingsRepository userSettings)
        {
            _userSettings = userSettings;
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
        [CustomAuth]
        public async Task<IActionResult> SetUserSettings(SetUserSettingsParams input)
        {
            var settings = input.Settings;
            if (settings.LevelOfAssessment is null || settings.Year is null)
                return BadRequest();

            await _userSettings.SetUserSettings(input.Settings);
            return Ok();
        }
    }
}
