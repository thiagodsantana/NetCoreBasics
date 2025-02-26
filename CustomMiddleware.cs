
namespace NetCoreBasics
{

    public class CustomMiddleware(RequestDelegate next, ILogger logger)
    {
        private readonly RequestDelegate _next = next;
        private readonly ILogger _logger = logger;
        public async Task Invoke(HttpContext context)
        {
            _logger.LogInformation($"Request: {context.GetEndpoint()!.DisplayName}");
            await _next(context); // Chama o próximo middleware
            _logger.LogInformation($"Response: Status Code {context.Response.StatusCode}");
        }
    }
}
