using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace TTNAzureBridge
{
    public class TTNAuthHandler : DelegatingHandler
    {
        public TTNAuthHandler(HttpMessageHandler innerHandler) : base(innerHandler) { }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", TTNConfigProvider.GetTTNBearerToken());
            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            if (response.Equals(HttpStatusCode.Unauthorized))
            {
                // TODO: Get bearer token
            }

            return response;
        }
    }
}