using System.Linq;
using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Infrastructure.DomainServices;

namespace TAMHR.ESS.Infrastructure.Web.ViewComponents
{
    public class ConcurrentPosition : ViewComponent
    {
        private readonly MdmService _mdmService;

        public ConcurrentPosition(MdmService mdmService)
        {
            _mdmService = mdmService;
        }

        public IViewComponentResult Invoke(string noreg, string currentPostCode)
        {
            var organizationStructures = _mdmService.GetActualOrganizationStructures(noreg).Where(x => x.PostCode != currentPostCode);

            return View(organizationStructures);
        }
    }
}
