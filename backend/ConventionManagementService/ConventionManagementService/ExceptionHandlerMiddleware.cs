using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ConventionManagementService
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _Next;
        public ExceptionHandlerMiddleware(RequestDelegate next) 
        {
            _Next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                if (_Next != null)
                {
                    await _Next.Invoke(context);
                }
            }
            catch (Exception exception)
            {
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = 500;
                Stream bodyStream = context.Response.Body;
                var writer = new StreamWriter(bodyStream);
                writer.Write($"{{\"error\":{{\"code\":\"{HttpStatusCode.InternalServerError}\"}}}}");
                await writer.FlushAsync(); 
            }
        }
    }

    public static class ExceptionHandlerMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionHandler(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionHandlerMiddleware>();
        }
    }
}
