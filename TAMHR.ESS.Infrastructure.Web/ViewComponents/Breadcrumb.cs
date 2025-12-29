using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Infrastructure.Extensions;
using TAMHR.ESS.Infrastructure.DomainServices;

namespace TAMHR.ESS.Infrastructure.Web.ViewComponents
{
    public class Breadcrumb : ViewComponent
    {
        private readonly CoreService _coreService;

        public Breadcrumb(CoreService coreService)
        {
            _coreService = coreService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var routeInfo = RouteData.Extract();
            var queryString = Request.QueryString.Value;
            var parentMenus = await _coreService.GetParentMenus($"~/{routeInfo.Area}/{routeInfo.Controller}/{routeInfo.Action}", queryString);
            
            return View(parentMenus);
        }
    }
}
