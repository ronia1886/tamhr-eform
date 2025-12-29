using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Infrastructure.DomainServices;

namespace TAMHR.ESS.Infrastructure.Web.ViewComponents
{
    public class ReportingStructure : ViewComponent
    {
        private readonly MdmService _mdmService;

        public ReportingStructure(MdmService mdmService)
        {
            _mdmService = mdmService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string noreg, string postCode)
        {
            var output = await _mdmService.GetActualReportingStructuresAsync(noreg, postCode);

            return View(output);
        }
    }
}
