using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authentication;
using TAMHR.ESS.Infrastructure.Web.Extensions;
using TAMHR.ESS.Infrastructure.DomainServices;
using RestSharp;

namespace TAMHR.ESS.Infrastructure.Web.Filters
{
    public class SsoSessionFilter : ActionFilterAttribute
    {
        private ConfigService _configService;

        public SsoSessionFilter(ConfigService configService)
        {
            _configService = configService;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var identifier = context.HttpContext.User.GetClaim("Jti");

            if (!string.IsNullOrEmpty(identifier))
            {
                var passportUrl = _configService.GetConfigValue<string>("Passport.Url", true);
                var sessionApi = _configService.GetConfigValue<string>("Passport.SessionApi", true) + "/" + identifier;

                var client = new RestClient(passportUrl);
                var request = new RestRequest(sessionApi, Method.Get);
                request.AddHeader("Content-Type", "application/json; charset=utf-8");
                var response = client.Get(request);
                var result = bool.Parse(response.Content);

                if (!result)
                {
                    context.HttpContext.SignOutAsync().GetAwaiter().GetResult();
                    context.Result = new RedirectResult("~/core/account/login");

                    return;
                }
            }

            base.OnActionExecuting(context);
        }
    }
}