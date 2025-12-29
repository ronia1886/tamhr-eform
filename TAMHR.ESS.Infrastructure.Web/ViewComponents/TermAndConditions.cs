using System;
using Microsoft.AspNetCore.Mvc;

namespace TAMHR.ESS.Infrastructure.Web.ViewComponents
{
    public class TermAndConditions : ViewComponent
    {
        public IViewComponentResult Invoke(Guid documentApprovalId)
        {
            return View();
        }
    }
}
