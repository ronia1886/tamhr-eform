using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Infrastructure.DomainServices;

namespace TAMHR.ESS.Infrastructure.Web.ViewComponents
{
    public class ChildRequestUrl : ViewComponent
    {
        private ApprovalService _approvalService;

        public ChildRequestUrl(ApprovalService service)
        {
            _approvalService = service;
        }

        public IViewComponentResult Invoke(Guid docId)
        {
            var document = _approvalService.GetDocumentApprovalById(docId);
            var childs = _approvalService.GetChildRequestDocuments(document).ToList();

            return View(childs);
        }

    }
}
