namespace Library.Models
{
    public enum AppUserType
    {
        NoAuthorizations = 0,
        BasicAdminUserWithGroup1Password = 1,
        BasicAdminUserWithGroup2Password = 2,
        AdvancedAdminUserWithUniquePasswords = 3,
        BasicAdminUserBarGroup = 4
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
