namespace Library.Models.DataTransferObjects.Output
{
    public class AuthResponse
    {
        public string UserName { get; }
        public AuthenticationResult AuthResult { get; }

        public AuthResponse(
            string userName, 
            AuthenticationResult result,
            bool isCodeValid)
        {
            UserName = userName;
            AuthResult = new AuthenticationResult(result, isCodeValid);            
        }
    }
}
