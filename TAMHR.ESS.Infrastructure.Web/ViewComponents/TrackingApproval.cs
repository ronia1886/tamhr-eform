using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Infrastructure.DomainServices;

namespace TAMHR.ESS.Infrastructure.Web.ViewComponents
{
    public class TrackingApproval : ViewComponent
    {
        private readonly ApprovalService _approvalService;

        public TrackingApproval(ApprovalService approvalService)
        {
            _approvalService = approvalService;
        }

        public async Task<IViewComponentResult> InvokeAsync(Guid id)
        {
            var output = await _approvalService.GetTrackingApprovalViewModel(id);
            var documentApproval = _approvalService.GetDocumentApprovalById(id);

            ViewBag.DocumentStatusCode = documentApproval?.DocumentStatusCode;

            return View(output);
        }
    }
}
