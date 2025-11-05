using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.Security.Authentication;

namespace PRN232_SU25_SE183096.api.ExceptionHandler
{
    public sealed class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        private static readonly IReadOnlyDictionary<int, string> StatusToCode = new Dictionary<int, string>
        {
            [400] = "HB40001",
            [401] = "HB40101",
            [403] = "HB40301",
            [404] = "HB40401",
            [500] = "HB50001"
        };

        public ErrorHandlingMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext ctx)
        {
            try
            {
                await _next(ctx);
            }
            catch (Exception ex)
            {
                var status = ex switch
                {
                    ValidationException or ArgumentException or ArgumentNullException or FormatException or InvalidOperationException => 400,
                    AuthenticationException or SecurityTokenException => 401,
                    UnauthorizedAccessException => 403,
                    KeyNotFoundException or FileNotFoundException => 404,
                    _ => 500
                };

                if (!StatusToCode.TryGetValue(status, out var errorCode))
                {
                    status = 500;
                    errorCode = StatusToCode[500];
                }

                await WriteJson(ctx, status, errorCode, ex.Message);
            }
        }

        private static Task WriteJson(HttpContext ctx, int status, string errorCode, string? message)
        {
            if (!ctx.Response.HasStarted)
            {
                ctx.Response.StatusCode = status;
                ctx.Response.ContentType = "application/json";
            }
            return ctx.Response.WriteAsJsonAsync(new
            {
                errorCode,
                message = message ?? string.Empty
            });
        }
    }
}
