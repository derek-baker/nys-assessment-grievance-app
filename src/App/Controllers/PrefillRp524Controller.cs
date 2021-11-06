using System;
using System.Diagnostics.Contracts;
using System.Text.Json;
using App.Services.Auth;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Library.Services;
using Library.Models;

namespace App.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PrefillRp524Controller : ControllerBase
    {
        /// <summary>
        /// Redirect to the front-end app, and includes prefill data in a cookie.
        /// </summary>   
        [EnableCors("PublicApiOpenCorsPolicy")]
        [HttpPost]
        public RedirectResult Post([FromForm] Rp524PrefillData data)
        {
            var cacheKey = "Rp524PrefillData";
            var clientCallbackRoute = "rp524";

            Contract.Requires(data != null);
            var cookieOptions = CookieFactoryService.BuildCookieOptions(DateTime.UtcNow.AddSeconds(20));
            base.Response.Cookies.Append(
                cacheKey, 
                JsonSerializer.Serialize(data), 
                cookieOptions
            );

            Uri encodedUrl = new Uri(Request.GetEncodedUrl());
            string host = HostService.GetAppUrlFromAmbientInfo(encodedUrl);

            return Redirect($"{host}/{clientCallbackRoute}");
        }
    }
}
