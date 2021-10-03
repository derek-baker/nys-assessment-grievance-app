using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Library.Models.Entities
{
    [BsonIgnoreExtraElements]
    public class User
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string PasswordHash { get; set; }
        public string Salt { get; set; }
        public byte[] SaltBytes => Convert.FromBase64String(Salt);
        public bool HasNeverLoggedIn { get; set; }
    }

    public enum AppUserType
    {
        NoAuthorization,
        BasicAdmin,
        AdvancedAdmin,
        BasicAdminBar
    }

    public class UserDetails
    {
        public AppUserType UserType { get; }
        public string Password { get; }

        public UserDetails(
            AppUserType type,
            string password
        )
        {
            UserType = type;
            Password = password;
        }
    }
}
