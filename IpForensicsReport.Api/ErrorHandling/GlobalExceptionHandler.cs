using IpForensicsReport.Api.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text.Json;

namespace IpForensicsReport.Api.ErrorHandling
{
    public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) 
        : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            // Do not convert a client-disconnected request into a 500 response.
            if (exception is OperationCanceledException
                && httpContext.RequestAborted.IsCancellationRequested)
            {
                return false;
            }

            var problemDetails = exception switch
            {
                InvalidIpAddressException => new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Invalid IP address",
                    Detail = exception.Message
                },

                HttpRequestException => new ProblemDetails
                {
                    Status = StatusCodes.Status502BadGateway,
                    Title = "External service unavailable",
                    Detail = "An external IP information service is unavailable."
                },

                OperationCanceledException => new ProblemDetails
                {
                    Status = StatusCodes.Status504GatewayTimeout,
                    Title = "External service timeout",
                    Detail = "An external IP information service did not respond in time."
                },

                CryptographicException => new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Report security processing failed",
                    Detail = "The report data could not be processed securely."
                },

                JsonException => new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Invalid report data",
                    Detail = "Report data could not be processed."
                },

                _ => new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Unexpected server error",
                    Detail = "An unexpected error occurred while processing the request."
                }
            };

            problemDetails.Instance = httpContext.Request.Path;

            problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

            LogException(httpContext, exception);

            httpContext.Response.StatusCode =
                problemDetails.Status
                ?? StatusCodes.Status500InternalServerError;

            await httpContext.Response.WriteAsJsonAsync(
                problemDetails,
                cancellationToken);

            return true;
        }

        private void LogException(HttpContext httpContext, Exception exception)
        {
            switch (exception)
            {
                case InvalidIpAddressException:
                    // Expected validation error; no exception log needed.
                    break;
                case HttpRequestException:
                case OperationCanceledException:
                    logger.LogWarning(
                        exception,
                        "External service request failed for {Method} {Path}. TraceId: {TraceId}",
                        httpContext.Request.Method,
                        httpContext.Request.Path,
                        httpContext.TraceIdentifier);
                    break;
                case CryptographicException:
                case JsonException:
                    logger.LogError(
                        exception,
                        "Report data processing failed for {Method} {Path}. TraceId: {TraceId}",
                        httpContext.Request.Method,
                        httpContext.Request.Path,
                        httpContext.TraceIdentifier);
                    break;
                default:
                    logger.LogError(
                        exception,
                        "Unhandled exception for {Method} {Path}. TraceId: {TraceId}",
                        httpContext.Request.Method,
                        httpContext.Request.Path,
                        httpContext.TraceIdentifier);
                    break;
            }
        }
    }
}
