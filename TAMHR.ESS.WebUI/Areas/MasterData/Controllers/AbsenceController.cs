using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using System.Linq;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;

namespace TAMHR.ESS.WebUI.Areas.MasterData.Controllers
{
    #region API Controller
    [Route("api/masterdata/absence")]
    //[Permission(PermissionKey.ManageAbsence)]
    [ApiController]
    public class AbsenceApiController : GenericApiControllerBase<AbsenceService, Absence>
    {
        protected override string[] ComparerKeys => new[] { "Code" };

        #region Domain Services
        public UserService UserService => ServiceProxy.GetService<UserService>();

        public AbsenceService AbsenceService => CommonService as AbsenceService;
        #endregion

        [HttpPost("get-defaults")]
        public DataSourceResult GetDefaults([DataSourceRequest] DataSourceRequest request)
        {
            return AbsenceService.GetDefaultCategories().ToDataSourceResult(request);
        }

        [HttpPost("getbyid")]
        public Domain.Absence GetById()
        {
            Guid Id = Guid.Parse(this.Request.Form["id"].ToString());
            return CoreService.GetAbsenceById(Id);
        }

        [HttpPost("getbyplan")]
        public async Task<IActionResult> GetByPlan()
        {
            var noReg = ServiceProxy.UserClaim.NoReg;
            var user = UserService.GetByNoReg(noReg);
            if (user == null)
            {
                return BadRequest("User Not Found!");
            }
            var q = CoreService.GetAbsenceQuery();
            if (user.Gender == "Male")
            {
                q = q.Where(x => x.Code != "up-CutiHaid");
            }

            if (!string.IsNullOrEmpty(Request.Form["IsPlanning"]))
            {
                bool IsPlanning = bool.Parse(this.Request.Form["IsPlanning"].ToString());
                var result = await q.Where(x => x.RowStatus && ((x.AbsenceType != "default" && x.AbsenceType != "cutihamil") || string.IsNullOrEmpty(x.AbsenceType))
                && (x.Planning == IsPlanning || x.Unplanning == !IsPlanning)).OrderBy(o => o.CodePresensi).ToDataSourceResultAsync(new DataSourceRequest());

                return Ok(result);
            }

            return Ok(new DataSourceResult() { Total = 0, Data = new List<object>() });
        }

        /// <summary>
        /// Get list of leave users by type
        /// </summary>
        /// <remarks>
        /// Get list of leave users by type
        /// </remarks>
        /// <param name="request">DataSourceRequest Object</param>
        /// <returns>DataSourceResult Object</returns>
        [HttpPost("get-leaves")]
        public async Task<DataSourceResult> GetLeaves([FromForm]string category, [DataSourceRequest] DataSourceRequest request)
        {
            var noreg = ServiceProxy.UserClaim.NoReg;

            return await UserService.GetLeaves(noreg, category).ToDataSourceResultAsync(request);
        }
    }
    #endregion

    #region MVC Controller
    /// <summary>
    /// Absence page controller
    /// </summary>
    [Area("MasterData")]
    [Permission(PermissionKey.ViewAbsence)]
    public class AbsenceController : GenericMvcControllerBase<AbsenceService, Absence>
    {
        [HttpPost]
        public override IActionResult Load(Guid id)
        {
            object commonData;

            if (id == Guid.Empty)
            {
                commonData = new Absence();
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