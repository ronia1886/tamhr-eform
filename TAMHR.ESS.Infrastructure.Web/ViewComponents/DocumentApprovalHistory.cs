using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Infrastructure.DomainServices;

namespace TAMHR.ESS.Infrastructure.Web.ViewComponents
{
    public class DocumentApprovalHistory : ViewComponent
    {
        private readonly ApprovalService _approvalService;

        public DocumentApprovalHistory(ApprovalService approvalService)
        {
            _approvalService = approvalService;
        }

        public async Task<IViewComponentResult> InvokeAsync(Guid id, bool deferred = false)
        {
            if (!deferred)
            {
                var output = await _approvalService.GetDocumentApprovalHistoriesAsync(id);

                return View(output);
            }
            else
            {
                ViewBag.Id = id;

                return View("Deferred");
            }
        }
    }
}
