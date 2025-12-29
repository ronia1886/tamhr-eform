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
using System.Data.Entity;
using Microsoft.EntityFrameworkCore;

namespace TAMHR.ESS.WebUI.Areas.MasterData.Controllers
{
    #region API Controller
    /// <summary>
    /// JSON category API manager.
    /// </summary>
    [Route("api/json-category")]
    [Permission(PermissionKey.ManageJsonCategory)]
    public class JsonCategoryApiController : GenericApiControllerBase<JsonCategoryService, JsonCategory>
    {
        protected override string[] ComparerKeys => new[] { "Category", "Title" };

        public JsonCategoryService JsonCategoryService => CommonService as JsonCategoryService;

        [HttpPost("gets")]
        public override async Task<DataSourceResult> GetFromPosts([DataSourceRequest] DataSourceRequest request)
        {
            var data = JsonCategoryService.GetJsonCategoriesQuery().ToList();

            return await Task.FromResult(data.ToDataSourceResult(request));
        }
    }
    #endregion

    #region MVC Controller
    /// <summary>
    /// JSON category page controller.
    /// </summary>
    [Area("MasterData")]
    [Permission(PermissionKey.ViewJsonCategory)]
    public class JsonCategoryController : GenericMvcControllerBase<JsonCategoryService, JsonCategory>
    {
        [HttpPost]
        public override IActionResult Load(Guid id)
        {
            object commonData;

            if (id == Guid.Empty)
            {
                commonData = new JsonCategory();
            }
            else
            {
                commonData = CommonService.GetById(id);
            }

            return GetViewData(commonData);
        }
    }
    #endregion
}