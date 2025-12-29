using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
using TAMHR.ESS.Infrastructure.Exceptions;
using TAMHR.ESS.Infrastructure.ViewModels;
using TAMHR.ESS.Infrastructure.DomainServices;
using Microsoft.Extensions.Hosting;

namespace TAMHR.ESS.Infrastructure.Web
{
    [Authorize]
    public abstract class MvcControllerBase : CommonControllerBase
    {
        /// <summary>
        /// Get pdf header url
        /// </summary>
        /// <returns>PDF Header URL</returns>

        protected string GetHeaderPdfUrl(string routeUrl)
        {
            var hostingEnvironment = ServiceProxy.GetHostingEnvironment();
            var baseUrl = string.Empty;
            var contextRequest = HttpContext.Request;

            if (hostingEnvironment.IsDevelopment())
            {
                baseUrl = $"{contextRequest.Scheme}://{contextRequest.Host}{contextRequest.PathBase}{routeUrl}";
            }
            else
            {
                var baseUrlConfigValue = ConfigService.GetConfig("Application.Url")?.ConfigValue;

                baseUrl = string.IsNullOrEmpty(baseUrlConfigValue) ? $"http://localhost{contextRequest.PathBase}{routeUrl}" : baseUrlConfigValue + routeUrl;
            }

            var uri = new Uri(baseUrl, UriKind.Absolute);
            return uri.ToString();
        }


        protected IActionResult CommonView(string message, string title = "Cannot create request", string backUrl = null)
        {
            var commonViewModel = new CommonViewModel(title, message, backUrl);

            return View("Commons/Warning", commonViewModel);
        }

        protected IActionResult RedirectIfException(Func<IActionResult> actionDelegate)
        {
            try
            {
                return actionDelegate();
            }
            catch (CommonViewException ex)
            {
                return CommonView(ex.Message, ex.Title, ex.BackUrl);
            }
        }
    }
}
