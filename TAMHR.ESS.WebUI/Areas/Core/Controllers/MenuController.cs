using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.ViewModels;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;

namespace TAMHR.ESS.WebUI.Areas.Core.Controllers
{
    #region API Controller
    /// <summary>
    /// Menu API Manager
    /// </summary>
    [Route("api/menu")]
    [Permission(PermissionKey.ManageMenu)]
    public class MenuApiController : ApiControllerBase
    {
        /// <summary>
        /// Get list of menus
        /// </summary>
        /// <remarks>
        /// Get list of menus
        /// </remarks>
        /// <returns>List of Menus</returns>
        [HttpPost("filter")]
        public async Task<DataSourceResult> GetFromFilter([DataSourceRequest] DataSourceRequest request)
        {
            return await CoreService.GetMenuByGroup("main").ToDataSourceResultAsync(request);
        }

        /// <summary>
        /// Get list of menus by roles
        /// </summary>
        /// <remarks>
        /// Get list of menus by roles
        /// </remarks>
        /// <returns>List of Menus by Roles</returns>
        [HttpPost("role-filter")]
        public async Task<DataSourceResult> GetFromRolesAndFilter([DataSourceRequest] DataSourceRequest request)
        {
            return await CoreService.GetMenuByGroup("main", ServiceProxy.UserClaim.Roles).ToDataSourceResultAsync(request);
        }

        /// <summary>
        /// Get Menu by id
        /// </summary>
        /// <remarks>
        /// Get Menu by id
        /// </remarks>
        /// <param name="id">Menu Id</param>
        /// <returns>Menu Object</returns>
        [HttpGet("{id}")]
        public Domain.Menu Get(Guid id) => CoreService.GetMenu(id);

        /// <summary>
        /// Get list of menus
        /// </summary>
        /// <remarks>
        /// Get list of menus
        /// </remarks>
        /// <param name="request">DataSourceRequest Object</param>
        /// <returns>DataSourceResult Object</returns>
        [HttpPost("gets")]
        public async Task<DataSourceResult> GetFromPosts([DataSourceRequest] DataSourceRequest request)
        {
            return await CoreService.GetMenus().ToDataSourceResultAsync(request);
        }

        /// <summary>
        /// Get list of menus by menu group
        /// </summary>
        /// <remarks>
        /// Get list of menus by menu group
        /// </remarks>
        /// <param name="group">Menu Group</param>
        /// <returns>List of Menus</returns>
        [HttpGet("group")]
        public IEnumerable<Domain.Menu> GetFromGroup(string group)
        {
            var menus = CoreService.GetMenuByGroup(group, true);

            return menus;
        }

        /// <summary>
        /// Toggle menu visibility
        /// </summary>
        /// <remarks>
        /// Toggle menu visibility
        /// </remarks>
        /// <param name="id">Menu Id</param>
        /// <returns>No Content</returns>
        [HttpPost("toggle/{id}")]
        public IActionResult Toggle(Guid id)
        {
            var body = this.Request.Body;
            CoreService.ToggleMenu(id);

            return NoContent();
        }

        /// <summary>
        /// Create new menu
        /// </summary>
        /// <remarks>
        /// Create new menu
        /// </remarks>
        /// <param name="menu">Menu Object</param>
        /// <returns>Created Menu Object</returns>
        [HttpPost]
        public IActionResult Create([FromBody] Domain.Menu menu)
        {
            CoreService.UpsertMenu(menu);

            return CreatedAtAction("Get", new { id = menu.Id });
        }

        /// <summary>
        /// Rename menu
        /// </summary>
        /// <remarks>
        /// Rename menu
        /// </remarks>
        /// <param name="viewModel">Rename Menu View Model</param>
        [HttpPost("rename")]
        public IActionResult Rename([FromBody] RenameMenuViewModel viewModel)
        {
            CoreService.RenameMenu(viewModel);

            return NoContent();
        }

        /// <summary>
        /// Set parent and menu order index
        /// </summary>
        /// <remarks>
        /// Set parent and menu order index
        /// </remarks>
        /// <param name="viewModel">Parent Menu View Model</param>
        [HttpPost("setparent")]
        public IActionResult SetParent([FromBody] ParentMenuViewModel viewModel)
        {
            CoreService.SetParent(viewModel);

            return NoContent();
        }

        /// <summary>
        /// Update favourite menu
        /// </summary>
        /// <remarks>
        /// Update favourite menu
        /// </remarks>
        /// <param name="ids">Menu Object</param>
        /// <returns>Created Menu Object</returns>
        [HttpPost]
        [Route("favourite")]
        public IActionResult UpdateFavouriteMenu([FromBody] Guid[] ids)
        {
            CoreService.UpdateFavouriteMenu(ServiceProxy.UserClaim.NoReg, ids);

            return NoContent();
        }

        /// <summary>
        /// Update Menu
        /// </summary>
        /// <remarks>
        /// Update Menu
        /// </remarks>
        /// <param name="menu">Menu Object</param>
        /// <returns>No Content</returns>
        [HttpPut]
        public IActionResult Update(Domain.Menu menu)
        {
            CoreService.UpsertMenu(menu);

            return NoContent();
        }

        /// <summary>
        /// Delete Menu by id
        /// </summary>
        /// <remarks>
        /// Delete Menu by id
        /// </remarks>
        /// <param name="id">Menu Id</param>
        /// <returns>No Content</returns>
        [HttpDelete]
        public IActionResult Delete([FromForm]Guid id)
        {
            CoreService.DeleteMenu(id);

            return NoContent();
        }
    }
    #endregion

    #region MVC Controller
    /// <summary>
    /// Menu page controller
    /// </summary>
    [Area("Core")]
    public class MenuController : MvcControllerBase
    {
        /// <summary>
        /// Core service object
        /// </summary>
        protected CoreService CoreService { get { return ServiceProxy.GetService<CoreService>(); } }

        /// <summary>
        /// View main menu page
        /// </summary>
        /// <returns>Menu Page</returns>
        [Permission(PermissionKey.ViewMenu)]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Load favourite menu form
        /// </summary>
        /// <returns>Favourit Menu Form</returns>
        [HttpPost]
        [Permission(PermissionKey.ChangeFavouriteMenu)]
        public async Task<IActionResult> Favourites()
        {
            var menus = await CoreService.GetFixedMenusAsync(ServiceProxy.UserClaim.NoReg, ServiceProxy.UserClaim.Roles);

            return PartialView("_FavouriteMenuForm", menus);
        }

        /// <summary>
        /// Load submenu form by id
        /// </summary>
        /// <param name="id">Menu Id</param>
        /// <returns>Submenu Form</returns>
        [HttpPost]
        public async Task<IActionResult> SubMenu(Guid id)
        {
            var subMenus = await CoreService.GetSubMenuAsync(id, ServiceProxy.UserClaim.Roles);

            return PartialView("_SubMenu", subMenus);
        }

        /// <summary>
        /// Load menu form by id
        /// </summary>
        /// <param name="id">Menu Id</param>
        /// <param name="pid">Parent Id (if any)</param>
        /// <param name="group">Menu Group Code (if any)</param>
        /// <returns>Menu Form</returns>
        [HttpPost]
        [Permission(PermissionKey.ManageMenu)]
        public IActionResult Load(Guid id, Guid? pid, string group = "")
        {
            var menu = CoreService.GetMenu(id);

            if (id == Guid.Empty)
            {
                menu.ParentId = pid;
                menu.MenuGroupCode = group;
            }

            var permissions = CoreService.GetPermissions(false).Where(x => x.PermissionTypeCode == "menu");

            ViewBag.Permissions = permissions;

            return PartialView("_MenuForm", menu);
        }
    }
    #endregion
}