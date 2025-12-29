using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Infrastructure.DomainServices;

namespace TAMHR.ESS.Infrastructure.Web.ViewComponents
{
    public class Carousel : ViewComponent
    {
        private NewsService _newsService;

        public Carousel(NewsService newsService)
        {
            _newsService = newsService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var stickyNews = await _newsService.GetStickyNewsWithoutBody();

            return View(stickyNews);
        }
    }
}
