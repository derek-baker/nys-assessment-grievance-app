namespace Library.Models.NoSQLDatabaseSchema
{
    public class UserDocument
    {
        public static UserDocumentFields Fields { get; } = new UserDocumentFields();
    }

    public class UserDocumentFields
    {
        public string UserId { get; } = "UserId";
        public string UserName { get; } = "UserName";
        public string PasswordHash { get; } = "PasswordHash";
        public string Salt { get; } = "Salt";
        public string HasNeverLoggedIn { get; } = "HasNeverLoggedIn";
    }
}
