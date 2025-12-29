using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Extensions;

namespace TAMHR.ESS.Infrastructure.Web.ViewComponents
{
    public class MainDashboard : ViewComponent
    {
        private readonly CoreService _coreService;

        public MainDashboard(CoreService coreService)
        {
            _coreService = coreService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string noreg)
        {
            var dashboardViewModel = await _coreService.GetDashboardViewModel(noreg);

            return View(dashboardViewModel);
        }
    }
}
