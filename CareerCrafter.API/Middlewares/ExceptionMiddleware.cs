using CareerCrafter.Core.Exceptions;
using log4net;
using System.Net;
using System.Text.Json;

namespace CareerCrafter.API.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private static readonly ILog _log = LogManager.GetLogger(typeof(ExceptionMiddleware));

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (NotFoundException ex)
            {
                _log.Warn($"Not found: {ex.Message}");
                await WriteResponse(context, HttpStatusCode.NotFound, ex.Message);
            }
            catch (UnauthorizedException ex)
            {
                _log.Warn($"Unauthorized: {ex.Message}");
                await WriteResponse(context, HttpStatusCode.Forbidden, ex.Message);
            }
            catch (ValidationException ex)
            {
                _log.Warn($"Validation error: {ex.Message}");
                await WriteResponse(context, HttpStatusCode.BadRequest, ex.Message);
            }
            catch (DuplicateException ex)
            {
                _log.Warn($"Duplicate error: {ex.Message}");
                await WriteResponse(context, HttpStatusCode.Conflict, ex.Message);
            }
            catch (Exception ex)
            {
                _log.Error($"Unhandled exception at {context.Request.Path}", ex);
                await WriteResponse(context, HttpStatusCode.InternalServerError, "An unexpected error occurred. Please try again later.");
            }
        }

        private static async Task WriteResponse(HttpContext context, HttpStatusCode statusCode, string message)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;
            var json = JsonSerializer.Serialize(new { message });
            await context.Response.WriteAsync(json);
        }
    }
}
