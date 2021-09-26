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
        /// <summary>
        /// Redirect to the front-end app, and send data in a cookie.
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

            Uri encodedUrl = new Uri(Request.GetEncodedUrl());
            string host = HostService.GetHostFromAmbientInfo(encodedUrl);

            return Redirect($"{host}/{data.ClientCallbackRoute}");
        }
    }
}
