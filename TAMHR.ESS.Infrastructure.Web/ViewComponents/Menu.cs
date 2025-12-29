using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Infrastructure.DomainServices;

namespace TAMHR.ESS.Infrastructure.Web.ViewComponents
{
    public class Menu : ViewComponent
    {
        private CoreService _coreService;

        public Menu(CoreService coreService)
        {
            _coreService = coreService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string noreg, string[] roles)
        {
            var menus = await _coreService.GetHomeMenusAsync(noreg, roles);

            return View(menus);
        }
    }
}
