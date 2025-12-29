using System;
using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Infrastructure.Extensions;

namespace TAMHR.ESS.Infrastructure.Web.ViewComponents
{
    public class FileUpload : ViewComponent
    {
        public IViewComponentResult Invoke(Guid docId, string field, string allowedTypes, bool enable = true)
        {
            ViewData["IS_MOBILE"] = this.HttpContext.Request.IsMobile();
            ViewData["DISABLE"] = enable;

            return View<dynamic>(new {
                docId = docId,
                fieldName = field,
                allowedTypes = allowedTypes
            });
        }
    }
}
