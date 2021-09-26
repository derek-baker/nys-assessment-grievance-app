using System;

namespace Library.Services
{
    public static class HostService
    {
        public static string GetHostFromAmbientInfo(Uri encodedUrl)
        {
            string host = "https" + Uri.SchemeDelimiter + encodedUrl.Host;
            return host;
        }
    }
}
