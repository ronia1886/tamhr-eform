using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;

namespace TAMHR.ESS.WebUI.Areas.Core.Controllers
{
    #region API Controller
    /// <summary>
    /// News API Manager
    /// </summary>
    [Route("api/news")]
    //[Permission(PermissionKey.ViewNews)]
    public class NewsApiController : ApiControllerBase
    {
        #region Domain Services
        /// <summary>
        /// News service object
        /// </summary>
        public NewsService NewsService => ServiceProxy.GetService<NewsService>();
        #endregion

        /// <summary>
        /// Get list of news
        /// </summary>
        /// <remarks>
        /// Get list of news
        /// </remarks>
        /// <returns>List of News</returns>
        [HttpGet]
        [Permission(PermissionKey.ListNews)]
        public IEnumerable<News> Gets() => NewsService.GetListNews();

        /// <summary>
        /// Get news by id
        /// </summary>
        /// <remarks>
        /// Get news by id
        /// </remarks>
        /// <param name="id">News Id</param>
        /// <returns>News Object</returns>
        [HttpGet("{id}")]
        [Permission(PermissionKey.ListNews)]
        public News Get(Guid id) => NewsService.GetNews(id);

        /// <summary>
        /// Get list of news
        /// </summary>
        /// <remarks>
        /// Get list of news
        /// </remarks>
        /// <param name="request">DataSourceRequest Object</param>
        /// <returns>DataSourceResult Object</returns>
        [HttpPost("gets")]
        [Permission(PermissionKey.ListNews)]
        public async Task<DataSourceResult> GetFromPosts([DataSourceRequest] DataSourceRequest request)
        {
            return await NewsService.GetListNewsWithoutBody().ToDataSourceResultAsync(request);
        }

        /// <summary>
        /// Update view count by news
        /// </summary>
        /// <remarks>
        /// Update view count by news
        /// </remarks>
        /// <param name="news">News Object</param>
        /// <returns>Created News Object</returns>
        [HttpPost("view")]
        [Permission(PermissionKey.ViewNewsDetail)]
        public IActionResult UpdateViewCount([FromBody] News news)
        {
            NewsService.UpdateViewCount(news.Id);

            return NoContent();
        }

        /// <summary>
        /// Update read count by news
        /// </summary>
        /// <remarks>
        /// Update read count by news
        /// </remarks>
        /// <param name="news">News Object</param>
        /// <returns>Created News Object</returns>
        [HttpPost("read")]
        [Permission(PermissionKey.ViewNewsDetail)]
        public IActionResult UpdateReadCount([FromBody] News news)
        {
            NewsService.UpdateReadCount(news.Id);

            return NoContent();
        }

        /// <summary>
        /// Create new news
        /// </summary>
        /// <remarks>
        /// Create new news
        /// </remarks>
        /// <param name="news">News Object</param>
        /// <returns>Created News Object</returns>
        [HttpPost]
        [Permission(PermissionKey.ManageNews)]
        public async Task<IActionResult> Create([FromBody] News news)
        {
            await NewsService.UpsertNews(news);

            return CreatedAtAction("Get", new { id = news.Id });
        }

        /// <summary>
        /// Update news
        /// </summary>
        /// <remarks>
        /// Update news
        /// </remarks>
        /// <param name="news">News Object</param>
        /// <returns>No Content</returns>
        [HttpPut]
        [Permission(PermissionKey.ManageNews)]
        public async Task<IActionResult> Update([FromBody]News news)
        {
            await NewsService.UpsertNews(news);

            return NoContent();
        }

        /// <summary>
        /// Delete news by id
        /// </summary>
        /// <remarks>
        /// Delete news by id
        /// </remarks>
        /// <param name="id">News Id</param>
        /// <returns>No Content</returns>
        [HttpDelete]
        [Permission(PermissionKey.ManageNews)]
        public IActionResult Delete([FromForm]Guid id)
        {
            NewsService.DeleteNews(id);

            return NoContent();
        }

        /// <summary>
        /// Download data
        /// </summary>
        /// <remarks>
        /// Download data
        /// </remarks>
        /// <returns>Data in Excel Format</returns>
        [HttpGet("download")]
        public IActionResult Download()
        {
            var data = Gets();

            return ExportToXlsx(data, $"News_{DateTime.Now.ToString("dd_MM_yyyy")}.xlsx");
        }

        /// <summary>
        /// Download data template
        /// </summary>
        /// <remarks>
        /// Download data template
        /// </remarks>
        /// <returns>Data Template in Excel Format</returns>
        [HttpGet("download-template")]
        public virtual IActionResult DownloadTemplate()
        {
            var controllerName = RouteData.Values["controller"].ToString();
            var cleanControllerName = controllerName.Substring(0, controllerName.Length - 3);

            return GenerateTemplate<News>($"{cleanControllerName}");
        }

        /// <summary>
        /// Upload and merge data
        /// </summary>
        /// <remarks>
        /// Upload and merge data
        /// </remarks>
        /// <returns>No Content</returns>
        [HttpPost("merge")]
        public async Task<IActionResult> Merge()
        {
            await UploadAndMergeAsync<News>(Request.Form.Files.FirstOrDefault(), new[] { "Id" });

            return NoContent();
        }
    }
    #endregion

    #region MVC Controller
    [Area("Core")]
    public class NewsController : MvcControllerBase
    {
        /// <summary>
        /// News service object
        /// </summary>
        protected NewsService NewsService { get { return ServiceProxy.GetService<NewsService>(); } }

        /// <summary>
        /// User service object
        /// </summary>
        protected UserService UserService { get { return ServiceProxy.GetService<UserService>(); } }

        /// <summary>
        /// View list of news
        /// </summary>
        /// <returns>List of News</returns>
        [Permission(PermissionKey.ListNews)]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Load news form for create
        /// </summary>
        /// <returns>News Form</returns>
        [Permission(PermissionKey.ManageNews)]
        public IActionResult Create()
        {
            return Edit(Guid.Empty);
        }

        /// <summary>
        /// Load news form for edit by id
        /// </summary>
        /// <param name="id">News Id</param>
        /// <returns>News Form</returns>
        [Permission(PermissionKey.ManageNews)]
        public IActionResult Edit(Guid id)
        {
            var news = NewsService.GetNews(id);

            return View("Edit", news);
        }

        /// <summary>
        /// View detail news by slug url
        /// </summary>
        /// <param name="slug">Slug Url</param>
        /// <returns>Detail News</returns>
        [Permission(PermissionKey.ViewNewsDetail)]
        public IActionResult Detail(string slug)
        {
            var news = NewsService.GetBySlug(slug);
            var creator = UserService.GetByNoReg(news.CreatedBy);

            NewsService.UpdateViewCount(news.Id);

            ViewBag.Creator = creator;

            return View(news);
        }

        /// <summary>
        /// Load news form by id
        /// </summary>
        /// <param name="id">News Id</param>
        /// <returns>News Form</returns>
        [HttpPost]
        [Permission(PermissionKey.ManageNews)]
        public IActionResult Load(Guid id)
        {
            var news = NewsService.GetNews(id);

            return PartialView("_NewsForm", news);
        }
    }
    #endregion
}