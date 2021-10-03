namespace Library.Models.NoSQLDatabaseSchema
{
    public class SessionDocument
    {
        public static SessionDocumentFields Fields => new SessionDocumentFields();
    }

    public class SessionDocumentFields
    {
        public string SessionId { get; } = "SessionId";
        public string UserId { get; } = "UserId";
        public string CreatedAt { get; } = "CreatedAt";
        public string ValidUntil { get; } = "ValidUntil";
    }
}
