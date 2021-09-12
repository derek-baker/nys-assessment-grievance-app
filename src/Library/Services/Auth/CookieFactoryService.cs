using Microsoft.AspNetCore.Http;

namespace App.Services.Auth
{
    public static class CookieFactoryService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns>Options that should not be used with cookies containing sensitive data.</returns>
        public static CookieOptions BuildCookieOptions()
        {
            var cookieOptions = new CookieOptions
            {
                // Set the secure flag, which Chrome's changes will require for SameSite none.
                // Note this will also require you to be running on HTTPS.
                Secure = false,
                // Set the cookie to HTTP only which is good practice unless you really do need
                // to access it client side in scripts.
                HttpOnly = false,
                // Add the SameSite attribute, this will emit the attribute with a value of none.
                // To not emit the attribute at all set
                // SameSite = (SameSiteMode)(-1)
                SameSite = SameSiteMode.Strict
            };
            return cookieOptions;
        }
    }
}
