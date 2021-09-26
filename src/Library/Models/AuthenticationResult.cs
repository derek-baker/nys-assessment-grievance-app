using Library.Models.Entities;

namespace Library.Models
{
    public class AppAuthorization
    {
        public string UserName { get; }
        public AppUserType UserType { get; }

        public AppAuthorization(string userName, AppUserType userType)
        {
            UserName = userName;
            UserType = userType;
        }
    }

    public class AuthenticationResult
    {
        public bool IsAuthenticated { get; }
        public AppAuthorization Authorization { get; }
        public Session Session { get; set; }

        public AuthenticationResult(
            bool isAuthed,
            AppAuthorization authorization)
        {
            IsAuthenticated = isAuthed;
            Authorization = authorization;
        }

        public AuthenticationResult(
            bool isAuthed,
            AppAuthorization authorization,
            Session session)
        {
            IsAuthenticated = isAuthed;
            Authorization = authorization;
            Session = session;
        }
    }
}
