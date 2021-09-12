namespace Library.Models.DataTransferObjects
{
    public class UserAuthInfo : IAuthorizable
    {
        public string userName { get; set; }
        public string password { get; set; }
    }
}
