using Library.Models;
using Library.Services.Crypto;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace Library.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly ImmutableDictionary<string, UserDetails> _logins;

        public AuthService()
        {
            //throw new NotImplementedException();
        }

        public AuthenticationResult AuthenticateAndAuthorizeUser(string userName, string password)
        {
            var userNameClean = removeNonAsciiChars(userName.ToLower().Trim());
            var passwordClean = removeNonAsciiChars(password.Trim());

            // TODO: Brute force mitigation based on IP?
            
            if (!_logins.Keys.Select(name => name.ToLower()).Contains(userNameClean)) 
            {
                return buildNoAuthResult(userNameClean);
            }
            UserDetails user = _logins[userNameClean];

            if (HashService.HashData(passwordClean) == user.Password)
            {
                return new AuthenticationResult(
                    isAuthed: true, 
                    authorization: new AppAuthorization(userNameClean, user.UserType)
                );
            }
            return buildNoAuthResult(userNameClean);

            static AuthenticationResult buildNoAuthResult(string user)
            {
                Thread.Sleep(500); // <== Hacky attempt at rate-limiting
                return new AuthenticationResult(
                    isAuthed: false, 
                    authorization: new AppAuthorization(user, AppUserType.NoAuthorizations)
                );
            }

            // https://stackoverflow.com/questions/15259275/removing-hidden-characters-from-within-strings#answer-21821412
            static string removeNonAsciiChars(string stringToClean)
            {
                var userNameClean = Regex.Replace(
                    stringToClean,
                    @"[^\u0009\u000A\u000D\u0020-\u007E]",
                    ""
                );
                return userNameClean;
            }
        }
    }
}
