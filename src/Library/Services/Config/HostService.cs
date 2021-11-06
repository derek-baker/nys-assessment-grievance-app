using System;

namespace Library.Services
{
    public static class HostService
    {
        public static string GetAppUrlFromAmbientInfo(Uri encodedUrl)
        {
            var port = encodedUrl.Host.Contains("localhost") ? $":{encodedUrl.Port}" : "";
            string appUrl = "https" + Uri.SchemeDelimiter + encodedUrl.Host + port;
            return appUrl;
        }
    }
}
