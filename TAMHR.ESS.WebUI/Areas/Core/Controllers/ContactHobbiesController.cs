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
using TAMHR.ESS.Infrastructure.Web.ViewComponents;

namespace TAMHR.ESS.WebUI.Areas.Core.Controllers
{
    #region API Controller
    /// <summary>
    /// Form API Manager
    /// </summary>
    [Route("api/contacthobbies")]
    [Permission(PermissionKey.ManageContactHobbies)]
    public class ContactHobbiesApiController : ApiControllerBase
    {
        #region Domain Services
        /// <summary>
        /// Form service object
        /// </summary>
        public FormService FormService => ServiceProxy.GetService<FormService>();
        #endregion

        /// <summary>
        /// Get list of forms
        /// </summary>
        /// <remarks>
        /// Get list of forms
        /// </remarks>
        /// <returns>List of Forms</returns>
        [HttpGet]
        public IEnumerable<Form> Gets() => FormService.Gets();

        /// <summary>
        /// Get form by id
        /// </summary>
        /// <remarks>
        /// Get form by id
        /// </remarks>
        /// <param name="id">Form Id</param>
        /// <returns>Form Object</returns>
        [HttpGet("{id}")]
        public Form Get(Guid id) => FormService.Get(id);

        /// <summary>
        /// Get list of forms
        /// </summary>
        /// <remarks>
        /// Get list of forms
        /// </remarks>
        /// <param name="request">DataSourceRequest Object</param>
        /// <returns>DataSourceResult Object</returns>
        [HttpPost("gets")]
        public async Task<DataSourceResult> GetFromPosts([DataSourceRequest] DataSourceRequest request)
        {
            return await FormService.Gets().ToDataSourceResultAsync(request);
        }

        /// <summary>
        /// Create new form
        /// </summary>
        /// <remarks>
        /// Create new form
        /// </remarks>
        /// <param name="form">Form Object</param>
        /// <returns>Created Form Object</returns>
        [HttpPost]
        public IActionResult Create([FromBody] Form form)
        {
            FormService.Upsert(ServiceProxy.UserClaim.NoReg, form);

            return CreatedAtAction("Get", new { id = form.Id });
        }

        /// <summary>
        /// Update form
        /// </summary>
        /// <remarks>
        /// Update form
        /// </remarks>
        /// <param name="form">Form Object</param>
        /// <returns>No Content</returns>
        [HttpPut]
        public IActionResult Update([FromBody]Form form)
        {
            FormService.Upsert(ServiceProxy.UserClaim.NoReg, form);

            return NoContent();
        }

        /// <summary>
        /// Delete form by id
        /// </summary>
        /// <remarks>
        /// Delete form by id
        /// </remarks>
        /// <param name="id">Form Id</param>
        /// <returns>No Content</returns>
        [HttpDelete]
        public IActionResult Delete([FromForm]Guid id)
        {
            FormService.Delete(id);

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

            return ExportToXlsx(data, $"Form_{DateTime.Now.ToString("dd_MM_yyyy")}.xlsx");
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

            return GenerateTemplate<Form>($"{cleanControllerName}");
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
            await UploadAndMergeAsync<Form>(Request.Form.Files.FirstOrDefault(), new[] { "FormKey" });

            return NoContent();
        }
    }
    #endregion

    #region MVC Controller
    [Area("Core")]
    public class ContactHobbiesController : MvcControllerBase
    {
        /// <summary>
        /// Core service object
        /// </summary>
        public CoreService CoreService => ServiceProxy.GetService<CoreService>();

        /// <summary>
        /// Form service object
        /// </summary>
        public FormService FormService => ServiceProxy.GetService<FormService>();

        [HttpGet]
        public async Task<IActionResult> RedirectTo(string formKey, Guid id)
        {
            var list = new[] {
                new { FormKey = "spkl-overtime", RedirectUrl = "~/timemanagement/spklreport/index?id={0}" },
                new { FormKey = "bdjk-planning", RedirectUrl = "~/timemanagement/bdjkreport/index?id={0}" }
            };

            var item = list.FirstOrDefault(x => x.FormKey == formKey);

            var fkey = FormService.Get(formKey).FormKey;

            if (item != null)
                return Redirect(string.Format(item.RedirectUrl, id));

            return await View(fkey, id);
        }

        [HttpGet]
        public IActionResult Index(string formKey)
        {
            if (!AclHelper.HasPermission($"Form.{formKey.Replace("-", string.Empty)}.View"))
            {
                var form = FormService.Get(formKey);
                return CommonView($"You dont have permission to view <b>{form.Title}</b> dashboard", "Access control list", backUrl: "~/");
            }

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Create(string formKey)
        {
            var menu = await CoreService.GetMenuByGroupAsync("main", "load?formKey=" + formKey);

            return Redirect(menu.Url);
        }

        [HttpGet]
        public async Task<IActionResult> View(string formKey, Guid id)
        {
            var menu = await CoreService.GetMenuByGroupAsync("main", "view?formKey=" + formKey);

            return Redirect($"{menu.Url}&docid=" + id);
        }

        [HttpGet]
        public async Task<IActionResult> Download(string formKey, Guid id)
        {
            var menu = await CoreService.GetMenuByGroupAsync("main", "pdf?formKey=" + formKey);
            if (menu == null)
            {
                throw new Exception("Document is not available.");
            }

            return Redirect($"{menu.Url}&docid=" + id);
        }

        [HttpPost]
        public IActionResult Load(Guid id)
        {
            var form = FormService.Get(id);

            return PartialView("_ContactHobbiesEditForm", form);
        }

        public IActionResult TermAndConditions(Guid documentApprovalId)
        {
            return ViewComponent(typeof(TermAndConditions), new { documentApprovalId });
        }
    }
    #endregion
}