using System.Net;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace VolunteerScheduler.API.Middleware
{
    public static class ProblemDetailsHelper
    {
        public static async Task WriteProblemDetailsAsync(HttpContext context, HttpStatusCode statusCode, string title, string detail)
        {
            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = (int)statusCode;

            var traceId = context.TraceIdentifier ?? Guid.NewGuid().ToString();

            var problemDetails = new
            {
                type = $"https://httpstatuses.io/{(int)statusCode}",
                title,
                status = (int)statusCode,
                detail,
                instance = context.Request.Path,
                traceId
            };

            var json = JsonSerializer.Serialize(problemDetails);
            await context.Response.WriteAsync(json);
        }
    }
}
