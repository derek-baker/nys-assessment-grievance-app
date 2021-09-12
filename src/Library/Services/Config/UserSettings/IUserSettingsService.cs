using System.Threading.Tasks;

namespace Library.Services.Config.UserSettings
{
    public interface IUserSettingsService
    {
        Task<Models.Settings.UserSettings> GetUserSettings();

        Task SetUserSettings(Models.Settings.UserSettings settings);
    }
}