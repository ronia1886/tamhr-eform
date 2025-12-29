using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Domain;
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
    /// User API Manager
    /// </summary>
    [Route("api/user")]
    //[Permission(PermissionKey.ViewUser)]
    public class UserApiController : ApiControllerBase
    {
        #region Domain Services
        /// <summary>
        /// User service object
        /// </summary>
        public UserService UserService => ServiceProxy.GetService<UserService>();
        #endregion

        /// <summary>
        /// Get user by id
        /// </summary>
        /// <remarks>
        /// Get user by id
        /// </remarks>
        /// <param name="id">User Id</param>
        /// <returns>User Object</returns>
        [HttpGet("{id}")]
        [Permission(PermissionKey.ManageUser)]
        public User Get(Guid id) => UserService.Get(id);

        /// <summary>
        /// Get list of users
        /// </summary>
        /// <remarks>
        /// Get list of users
        /// </remarks>
        /// <param name="request">DataSourceRequest Object</param>
        /// <returns>DataSourceResult Object</returns>
        [HttpPost("gets")]
        [Permission(PermissionKey.ManageUser)]
        public async Task<DataSourceResult> GetFromPosts([DataSourceRequest] DataSourceRequest request)
        {
            return await UserService.Gets().ToDataSourceResultAsync(request);
        }

        /// <summary>
        /// Get list of active users
        /// </summary>
        /// <remarks>
        /// Get list of active users
        /// </remarks>
        /// <param name="request">DataSourceRequest Object</param>
        /// <returns>DataSourceResult Object</returns>
        [HttpPost("getactives")]
        [Permission(PermissionKey.ManageUser)]
        public async Task<DataSourceResult> GetActiveUsers([DataSourceRequest] DataSourceRequest request)
        {
            return await UserService.GetActiveUsers().ToDataSourceResultAsync(request);
        }

        /// <summary>
        /// Get list of active users by gender
        /// </summary>
        /// <remarks>
        /// Get list of active users by gender
        /// </remarks>
        /// <param name="request">DataSourceRequest Object</param>
        /// <returns>DataSourceResult Object</returns>
        [HttpPost("get-actives-by-gender/{gender}")]
        [Permission(PermissionKey.FindEmployee)]
        public async Task<DataSourceResult> GetActiveUsersByGender(string gender, [DataSourceRequest] DataSourceRequest request)
        {
            return await UserService.GetActiveUsersByGender(gender).ToDataSourceResultAsync(request);
        }

        /// <summary>
        /// Get list of user roles by id
        /// </summary>
        /// <remarks>
        /// Get list of user roles by id
        /// </remarks>
        /// <param name="request">DataSourceRequest Object</param>
        /// <returns>DataSourceResult Object</returns>
        [HttpPost("getroles")]
        [Permission(PermissionKey.ManageUser)]
        public async Task<DataSourceResult> GetUserRoles([DataSourceRequest] DataSourceRequest request)
        {
            var id = Guid.Parse(Request.Form["id"]);

            return await UserService.GetRoles(id).ToDataSourceResultAsync(request);
        }

        /// <summary>
        /// Create new user
        /// </summary>
        /// <remarks>
        /// Create new user
        /// </remarks>
        /// <param name="user">User Object</param>
        /// <returns>Created User Object</returns>
        [HttpPost]
        [Permission(PermissionKey.ManageUser)]
        public IActionResult Create([FromBody] User user)
        {
            UserService.Upsert(user);

            return CreatedAtAction("Get", new { id = user.Id });
        }

        /// <summary>
        /// Update user
        /// </summary>
        /// <remarks>
        /// Update user
        /// </remarks>
        /// <param name="user">User Object</param>
        /// <returns>No Content</returns>
        [HttpPut]
        [Permission(PermissionKey.ManageUser)]
        public IActionResult Update(UserViewModel user)
        {
            UserService.Update(user);

            return NoContent();
        }

        /// <summary>
        /// Delete user by id
        /// </summary>
        /// <remarks>
        /// Delete user by id
        /// </remarks>
        /// <param name="id">User Id</param>
        /// <returns>No Content</returns>
        [HttpDelete("{id}")]
        [Permission(PermissionKey.ManageUser)]
        public IActionResult Delete(Guid id)
        {
            UserService.SoftDelete(id);

            return NoContent();
        }
    }
    #endregion

    #region MVC Controller
    /// <summary>
    /// User page controller
    /// </summary>
    [Area("Core")]
    [Permission(PermissionKey.ViewUser)]
    public class UserController : MvcControllerBase
    {
        /// <summary>
        /// User service
        /// </summary>
        protected UserService UserService { get { return ServiceProxy.GetService<UserService>(); } }

        /// <summary>
        /// User main page
        /// </summary>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Load user form by id
        /// </summary>
        /// <param name="id">User Id</param>
        /// <returns>User Form</returns>
        [HttpPost]
        public IActionResult Load(Guid id)
        {
            var user = UserService.Get(id);

            return PartialView("_UserForm", user);
        }
    }
    #endregion
}