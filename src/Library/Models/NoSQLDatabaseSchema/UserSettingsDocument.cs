using Library.Models.Entities;
using Library.Models.Settings;
using MongoDB.Bson.Serialization.Attributes;

namespace Library.Models
{
    [BsonIgnoreExtraElements]
    public class UserSettingsDocument
    {
        public static UserSettingsDocumentFields Fields = new UserSettingsDocumentFields();
        public UserSettings UserSettings { get; set; }
    }

    public class UserSettingsDocumentFields
    {
        public string UserSettings { get; } = "UserSettings";
    }
}
