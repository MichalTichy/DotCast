namespace DotCast.App.Middlewares
{
    public class LegacyUrlRedirectMiddleware
    {
        private readonly RequestDelegate _next;

        public LegacyUrlRedirectMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var request = context.Request;
            var response = context.Response;

            if (request.Path.Value.Contains("podcast", StringComparison.OrdinalIgnoreCase))
            {
                var newUrl = request.Path.Value.Replace("podcast", "audiobook", StringComparison.OrdinalIgnoreCase) + request.QueryString;
                response.Redirect(newUrl, true);
            }
            else
            {
                await _next(context);
            }
        }
    }
}