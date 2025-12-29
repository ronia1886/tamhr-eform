using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Infrastructure.DomainServices;

namespace TAMHR.ESS.Infrastructure.Web.ViewComponents
{
    public class ChangeTracker : ViewComponent
    {
        private readonly ApprovalService _approvalService;

        public ChangeTracker(ApprovalService approvalService)
        {
            _approvalService = approvalService;
        }

        public async Task<IViewComponentResult> InvokeAsync(Guid id)
        {
            var changeTrackers = await _approvalService.GetChangeTrackers(id);
            
            return View(changeTrackers);
        }
    }
}
