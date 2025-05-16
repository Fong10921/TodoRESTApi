using System.Net;

namespace TodoRESTApi.WebAPI.StartupExtensions
{
    public class RateLimitRedirectHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RateLimitRedirectHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                var context = _httpContextAccessor.HttpContext;
                
                if (context != null)
                {
                    // Trigger a redirect immediately
                    context.Response.Redirect("/RateLimitExceeded");
                }
                // Throw an exception so that the calling code does not try to read the response body as JSON
                throw new HttpRequestException("Rate limit exceeded. Redirecting...");
            }

            return response;
        }
    }
}