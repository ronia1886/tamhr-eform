using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using TAMHR.ESS.Domain;

namespace TAMHR.ESS.Infrastructure.Web.ViewComponents
{
    public class Attachment : ViewComponent
    {
        public IViewComponentResult Invoke(IEnumerable<DocumentApprovalFile> files, string fieldCategory)
        {
            var objFiles = files?.Where(x => x.FieldCategory.EndsWith(fieldCategory)).ToList();

            return View(objFiles);
        }
    }
}
