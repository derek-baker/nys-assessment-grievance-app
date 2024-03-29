﻿using Library.Models;
using Library.Models.Entities;
using Library.Services.Clients.Database.Repositories;
using Library.Services.Crypto;
using Library.Services.Time;
using System;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Library.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _users;
        private readonly ISessionRepository _sessions;
        private readonly ITimeService _time;
        private readonly Contracts.Settings _settings;

        public AuthService(
            IUserRepository users, 
            ISessionRepository sessions,
            ITimeService time,
            Contracts.Settings settings)
        {
            _users = users;
            _sessions = sessions;
            _time = time;
            _settings = settings;
        }

        public async Task<(AuthenticationResult Result, User User)> AuthenticateAndAuthorizeUser(
            string userName, 
            string password)
        {
            var userNameClean = removeNonAsciiChars(userName.ToLower().Trim());
            var passwordClean = removeNonAsciiChars(password.Trim());

            var user = await _users.GetUser(userNameClean);
            if (user is null) 
                return (Result: buildNoAuthResult(userNameClean), User: null);

            if (HashService.HashData<string>(passwordClean, user.SaltBytes) == user.PasswordHash)
            {
                var session = await _sessions.CreateSession(user.UserId);

                await _users.RecordLogin(user.UserId);

                var result = new AuthenticationResult(
                    isAuthed: true, 
                    authorization: new AppAuthorization(userNameClean, AppUserType.AdvancedAdmin),
                    session);
                return (Result: result, User: user);
            }
            return (Result: buildNoAuthResult(userNameClean), User: user);
        }

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

        public async Task<(bool IsSuccess, int? Code, bool IsUserBuiltIn)> GenerateSecurityCode(string userEmail)
        {
            var user = await _users.GetUser(userEmail);
            if (user is null) return (IsSuccess: false, Code: null, IsUserBuiltIn: false);
            int? code = !user.IsBuiltIn ? (int?)GetSecurityCode(user, GetTime()) : null;
            return (IsSuccess: true, Code: code, IsUserBuiltIn: user.IsBuiltIn);
        }

        public bool ValidateSecurityCode(int code, User user)
        {
            if (!user.IsBuiltIn)
            {
                var time = GetTime();
                foreach (var minute in Enumerable.Range(0, 5))
                {
                    var delta = -1 * minute;
                    var candidateCode = GetSecurityCode(user, time.AddMinutes(delta));
                    if (candidateCode == code) return true;
                }
                return false;
            }
            else if (user.IsBuiltIn)
            {
                return code == _settings.Admin.DefaultSecurityCode;
            }
            else { return false; }
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

        private static bool IsInvalidSession(Session session)
            => session is null || session.ValidUntil <= DateTime.UtcNow;

        private DateTime GetTime() 
            => _time.GetTime();

        private static int GetSecurityCode(User user, DateTime time) 
            => HashService.GenerateSecurityCode(
                user.Salt, 
                time.ToString(format: "MM/dd/yyyy hh:mm"));
    }
}
