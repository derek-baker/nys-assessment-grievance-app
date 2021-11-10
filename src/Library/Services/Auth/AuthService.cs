using Library.Models;
using Library.Models.Entities;
using Library.Services.Clients.Database.Repositories;
using Library.Services.Crypto;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Library.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly UserRepository _users;
        private readonly SessionRepository _sessions;

        public AuthService(UserRepository users, SessionRepository sessions)
        {
            _users = users;
            _sessions = sessions;
        }

        public async Task<AuthenticationResult> AuthenticateAndAuthorizeUser(string userName, string password)
        {
            var userNameClean = removeNonAsciiChars(userName.ToLower().Trim());
            var passwordClean = removeNonAsciiChars(password.Trim());

            // TODO: Brute force mitigation based on IP?

            var user = await _users.GetUser(userName);

            if (user is null) return buildNoAuthResult(userNameClean);

            if (HashService.HashData(passwordClean, user.SaltBytes) == user.PasswordHash)
            {
                var session = await _sessions.CreateSession(user.UserId);

                await _users.RecordLogin(user.UserId);

                return new AuthenticationResult(
                    isAuthed: true, 
                    authorization: new AppAuthorization(userNameClean, AppUserType.AdvancedAdmin),
                    session);
            }
            return buildNoAuthResult(userNameClean);
        }

        private static bool IsInvalidSession(Session session) 
            => session is null || session.ValidUntil <= DateTime.UtcNow;

        public async Task<(bool IsValidSession, string UserName)> ValidateSession(Session sessionFromCookie)
        {
            var sessionFromDb = await _sessions.GetUserSession(
                sessionFromCookie.UserId,
                sessionFromCookie.SessionId);

            var userName = (await _users.GetUserById(sessionFromCookie.UserId))?.UserName;
            return (IsValidSession: !IsInvalidSession(sessionFromDb), UserName: userName);
        }

        public async Task<bool> ValidateSession(string cookieData)
        {
            var sessionFromCookie = JsonSerializer.Deserialize<Session>(cookieData);
            var sessionFromDb = await _sessions.GetUserSession(
                sessionFromCookie.UserId,
                sessionFromCookie.SessionId);

            return !IsInvalidSession(sessionFromDb);
        }

        public Task<bool> ValidateSecurityCode(string code, Session session)
        {
            var intervals = new List<int> { 0, 1, 2, 3, 4 };

            // hash codes from last five minutes

            return Task.Run(() => true);
        }

        private static AuthenticationResult buildNoAuthResult(string user)
        {
            Thread.Sleep(500); // <== Hacky attempt at rate-limiting
            return new AuthenticationResult(
                isAuthed: false,
                authorization: new AppAuthorization(user, AppUserType.NoAuthorization)
            );
        }

        // https://stackoverflow.com/questions/15259275/removing-hidden-characters-from-within-strings#answer-21821412
        private static string removeNonAsciiChars(string stringToClean)
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
