using System;
using System.Net;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TAMHR.ESS.Infrastructure.Extensions;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TAMHR.ESS.Infrastructure.Web.Models;
using TAMHR.ESS.Infrastructure.Web.Extensions;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using TAMHR.ESS.Infrastructure.Web.UserAgentManager;

namespace TAMHR.ESS.Infrastructure.Web.Filters
{
    public class ExceptionFilter : ExceptionFilterAttribute
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DbContextOptions<UnitOfWork> _dbContextOptions;
        private readonly IStringLocalizer<ExceptionFilter> _localizer;
        private readonly IModelMetadataProvider _modelMetadataProvider;

        public ExceptionFilter(DbContextOptions<UnitOfWork> dbContextOptions, IHttpContextAccessor httpContextAccessor, IModelMetadataProvider modelMetadataProvider, IStringLocalizer<ExceptionFilter> localizer)
        {
            _httpContextAccessor = httpContextAccessor;
            _dbContextOptions = dbContextOptions;
            _localizer = localizer;
            _modelMetadataProvider = modelMetadataProvider;
        }

        public override void OnException(ExceptionContext context)
        {
            if (context.Exception != null)
            {
                try
                {
                    if (context.HttpContext.User.Identity.IsAuthenticated)
                    {
                        var isWebApi = !context.RouteData.DataTokens.Keys.Contains("area");
                        var ipAddress = context.HttpContext.Connection.RemoteIpAddress.ToString();
                        var userAgent = context.HttpContext.Request.Headers["User-Agent"].FirstOrDefault();
                        var userAgentManager = new UserAgent(userAgent);
                        var area = context.RouteData.Values["area"]?.ToString();
                        var controller = context.RouteData.Values["controller"].ToString();
                        var action = context.RouteData.Values["action"].ToString();
                        var areaFormat = !string.IsNullOrEmpty(area) ? $"<b>Area: {area}</b><br/>" : string.Empty;

                        using (var uow = new UnitOfWork(_dbContextOptions, _httpContextAccessor))
                        {
                            var service = new LogService(uow);

                            service.LogError(context.HttpContext.User.GetClaim("Username") ?? "system", ipAddress, string.Format("{0} {1} with OS {2} {3}", userAgentManager.Browser.Name, userAgentManager.Browser.Version, userAgentManager.OS.Name, userAgentManager.OS.Version), string.Format($"{areaFormat}<b>Controller: {controller}</b><br/><b>Action: {action}</b>"), context.Exception.ToString());
                        }
                    }
                }
                catch
                {
                }

                if (context.HttpContext.Request.IsAjaxRequest())
                {
                    var exception = context.Exception;

                    context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.ExceptionHandled = true;

                    var title = exception.Data.Contains("Title") ? exception.Data["Title"].ToString() : string.Empty;
                    var message = exception.InnerException != null ? exception.InnerException.Message : exception.Message;

                    context.Result = new JsonResult(new ExceptionInfo(_localizer[title].Value, _localizer[message].Value));
                }
                else
                {
                    base.OnException(context);
                }
            }
        }
    }
}
