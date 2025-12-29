using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Infrastructure.DomainServices;

namespace TAMHR.ESS.Infrastructure.Web.ViewComponents
{
    public class Guideline : ViewComponent
    {
        private readonly CoreService _coreService;

        public Guideline(CoreService coreService)
        {
            _coreService = coreService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var guidelines = await _coreService.GetGuidelines();
            
            return View(guidelines);
        }
    }
}
