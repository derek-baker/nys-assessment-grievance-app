using Microsoft.AspNetCore.Http;
using System;

namespace App.Services.Auth
{
    public static class CookieFactoryService
    {
        public static CookieOptions BuildCookieOptions(DateTime expiresAt)
        {
            var cookieOptions = new CookieOptions
            {
                Secure = false,
                HttpOnly = false,
                SameSite = SameSiteMode.Strict,
                Expires = expiresAt
            };
            return cookieOptions;
        }
    }
}
