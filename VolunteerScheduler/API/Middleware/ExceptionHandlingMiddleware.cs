using System.Net;

namespace VolunteerScheduler.API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ArgumentException ex)
            {
                await ProblemDetailsHelper.WriteProblemDetailsAsync(context, HttpStatusCode.BadRequest, "Bad Request", ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                await ProblemDetailsHelper.WriteProblemDetailsAsync(context, HttpStatusCode.NotFound, "Not Found", ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                await ProblemDetailsHelper.WriteProblemDetailsAsync(context, HttpStatusCode.Conflict, "Conflict", ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                await ProblemDetailsHelper.WriteProblemDetailsAsync(context, HttpStatusCode.Unauthorized, "Unauthorized", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred");
                await ProblemDetailsHelper.WriteProblemDetailsAsync(context, HttpStatusCode.InternalServerError, "Internal Server Error", "An unexpected error occurred.");
            }
        }
    }
}
