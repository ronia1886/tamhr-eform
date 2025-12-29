using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Infrastructure.DomainServices;

namespace TAMHR.ESS.Infrastructure.Web.ViewComponents
{
    public class Impersonator : ViewComponent
    {
        private CoreService _coreService;

        public Impersonator(CoreService coreService)
        {
            _coreService = coreService;
        }

        public IViewComponentResult Invoke(string noreg)
        {
            var userImpersonations = _coreService.GetUserImpersonations(noreg);

            return View(userImpersonations);
        }
    }
}
