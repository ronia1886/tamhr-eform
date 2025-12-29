using Agit.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TAMHR.ESS.Infrastructure.Extensions;

namespace TAMHR.ESS.Infrastructure.Web.Middlewares
{
    public class ExceptionMiddleware
    {
        private RequestDelegate _next;
        private ExceptionHandlerOptions _options;
        public ExceptionMiddleware(RequestDelegate next, IOptions<ExceptionHandlerOptions> options)
        {
            _next = next;
            _options = options.Value;

            Assert.ThrowIf(_options.ExceptionHandlingPath == null, "ExceptionHandlingPath must be defined");

            _options.ExceptionHandlingPath = string.IsNullOrEmpty(_options.ExceptionHandlingPath.Value) ? "/error" : _options.ExceptionHandlingPath.Value;
            _options.ExceptionHandler = next;
        }

        public async Task Invoke(HttpContext context)
         {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var path = context.Request.Path;
                var exceptionHandlerFeature = new ExceptionHandlerFeature { Path = path, Error = ex };

                context.Response.StatusCode = (int) HttpStatusCode.OK;
                context.Request.Path = _options.ExceptionHandlingPath;

                context.Response.Clear();
                context.Features.Set<IExceptionHandlerFeature>(exceptionHandlerFeature);

                await _next(context);

                return;
            }
        }
    }

    public static class ExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionMiddleware(this IApplicationBuilder builder, string errorHandler)
        {
            return builder.UseMiddleware<ExceptionMiddleware>(Options.Create(new ExceptionHandlerOptions
            {
                ExceptionHandlingPath = errorHandler
            }));
        }
    }
}
