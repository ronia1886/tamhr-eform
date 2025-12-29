using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Infrastructure.ViewModels;

namespace TAMHR.ESS.Infrastructure.Web.ViewComponents
{
    public class RequestHistory : ViewComponent
    {
        public IViewComponentResult Invoke(string formKey = "", bool showFormTitle = false)
        {
            var requestHistoryViewModel = RequestHistoryViewModel.Create(formKey, showFormTitle, string.Empty);

            return View(requestHistoryViewModel);
        }
    }
}
