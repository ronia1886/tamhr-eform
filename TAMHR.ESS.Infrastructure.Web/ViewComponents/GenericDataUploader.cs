using System;
using Microsoft.AspNetCore.Mvc;

namespace TAMHR.ESS.Infrastructure.Web.ViewComponents
{
    public class GenericDataUploader : ViewComponent
    {
        public IViewComponentResult Invoke(string url)
        {
            ViewBag.ApiUrl = url;

            return View();
        }
    }
}
