using System;

namespace Library.Services
{
    // TODO: Non-static interface implementation
    public static class HostService
    {
        public static string GetHostFromAmbientInfo(Uri encodedUrl)
        {
            var port = encodedUrl.Host.Contains("localhost") ? $":{encodedUrl.Port}" : "";
            string host = "https" + Uri.SchemeDelimiter + encodedUrl.Host + port;
            return host;
        }
    }
}
