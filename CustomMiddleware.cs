using Microsoft.Extensions.Logging;

namespace NetCoreBasics
{
    public class CustomMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next;        
        public async Task Invoke(HttpContext context)
        {
            Console.WriteLine($"Request: {context.GetEndpoint()!.DisplayName}");
            await _next(context); // Chama o próximo middleware
            Console.WriteLine($"Response: Status Code {context.Response.StatusCode}");
        }
    }
}
