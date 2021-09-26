using System;
using System.Diagnostics.Contracts;
using System.Text.Json;
using App.Services.Auth;
using Library.Models.DataTransferObjects;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Library.Services;

namespace App.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PrefillRp524Controller : ControllerBase
    {
        private readonly ILogger<PrefillRp524Controller> _logger;

        public PrefillRp524Controller(ILogger<PrefillRp524Controller> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Redirect to the front-end app, and send data in a cookie.
        /// COOKIE DOCS: https://docs.microsoft.com/en-us/aspnet/core/security/samesite?view=aspnetcore-3.1#net-core-support-for-the-samesite-attribute
        /// CACHE DOCS: https://docs.microsoft.com/en-us/aspnet/core/performance/caching/memory?view=aspnetcore-3.1
        /// </summary>   
        [EnableCors("ApiPolicy")]
        [HttpPost]
        public RedirectResult Post([FromBody] PrefillDataParams data)
        {
            Contract.Requires(data != null);
            var cookieOptions = CookieFactoryService.BuildCookieOptions(DateTime.Now.AddMinutes(5));
            base.Response.Cookies.Append(
                data.CacheKey, 
                JsonSerializer.Serialize(data.PrefillData), 
                cookieOptions
            );
            _logger.LogInformation(JsonSerializer.Serialize(data.PrefillData));

            Uri encodedUrl = new Uri(Request.GetEncodedUrl());
            string host = HostService.GetHostFromAmbientInfo(encodedUrl);

            return Redirect($"{host}/{data.ClientCallbackRoute}");
        }
    }
}
