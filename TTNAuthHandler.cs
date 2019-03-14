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
            const int MAX_RETRIES = 3;
            HttpResponseMessage response = null;

            for (var retries = 0; retries < MAX_RETRIES; retries++)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", TTNConfigProvider.GetTTNBearerToken());
                response = await base.SendAsync(request, cancellationToken);

                if (response.Equals(HttpStatusCode.Unauthorized))
                {
                    TTNConfigProvider.RenewTTNBearerToken();
                }
                else
                {
                    break;
                }
            }

            return response;
        }
    }
}