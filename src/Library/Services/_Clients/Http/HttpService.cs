using System.Net.Http;

namespace App.Services.Clients.Http
{
    public static class HttpService
    {
        /// <summary>
        /// HttpClient is intended to be instantiated once and re-used throughout the life of 
        /// an application. Instantiating an HttpClient class for every request will exhaust 
        /// the number of sockets available under heavy loads. 
        /// This will result in SocketException errors.
        /// DOCS: https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclient?view=netcore-3.1
        /// </summary>
        public static readonly HttpClient Client = new HttpClient();
    }
}
