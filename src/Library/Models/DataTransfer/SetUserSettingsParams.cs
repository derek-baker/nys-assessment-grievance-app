using Library.Models.Settings;

namespace Library.Models.DataTransferObjects
{
    public class SetUserSettingsParams : UserAuthInfo
    {
        public UserSettings settings { get; set; }
    }
}
