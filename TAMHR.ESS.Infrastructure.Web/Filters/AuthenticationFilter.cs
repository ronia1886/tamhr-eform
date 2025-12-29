using Microsoft.AspNetCore.Mvc.Filters;

namespace TAMHR.ESS.Infrastructure.Web.Filters
{
    public class AuthenticationFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
        }
    }
}