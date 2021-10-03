using Library.Models.Entities;
using Library.Services.Auth;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace App
{
    public class CustomAuthAttribute : Attribute, IAsyncAuthorizationFilter
    {
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var auth = context.HttpContext.RequestServices.GetService<IAuthService>();

            context.HttpContext.Request.Cookies.TryGetValue(nameof(Session), out var sessionCookie); 

            if (!(await auth.ValidateSession(sessionCookie)))
                context.Result = new ChallengeResult(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}
