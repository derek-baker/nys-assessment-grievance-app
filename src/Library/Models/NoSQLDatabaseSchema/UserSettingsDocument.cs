using Library.Models.Settings;
using MongoDB.Bson.Serialization.Attributes;

namespace Library.Models
{
    [BsonIgnoreExtraElements]
    public class UserSettingsDocument
    {
        public UserSettings UserSettings { get; set; }
    }
}
