using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;

namespace TAMHR.ESS.WebUI.Areas.MasterData.Controllers
{
    #region API Controller
    /// <summary>
    /// Allowance API Manager
    /// </summary>
    [Route("api/allowancedetail")]
    //[Permission(PermissionKey.ManageAllowanceDetail)]
    public class AllowanceDetailApiController : GenericApiControllerBase<AllowanceDetailService, AllowanceDetail>
    {
        protected override string[] ComparerKeys => new[] { "Type" };

        /// <summary>
        /// Get list of allowance details
        /// </summary>
        /// <remarks>
        /// Get list of allowance details
        /// </remarks>
        /// <returns>List of Allowance Details</returns>
        [HttpPost("getamount")]
        public decimal GetAmount()
        {
            var noreg = Request.Form["noreg"].ToString();
            var type = Request.Form["type"].ToString();
            var subtype = Request.Form["subtype"].ToString();

            return CoreService.GetAllowanceAmount(noreg, type, subtype);
        }

        [HttpPost("getinfo")]
        public object GetInfo()
        {
            var noreg = Request.Form["noreg"].ToString();
            var type = Request.Form["type"].ToString();
            var subtype = Request.Form["subtype"].ToString();

            return CoreService.GetInfoAllowance(noreg, type, subtype);
        }

        [HttpPost("getlistinfo")]
        public IEnumerable<object> GetListInfo()
        {
            var noreg = Request.Form["noreg"].ToString();
            var type = Request.Form["type"].ToString();
            var subtype = Request.Form["subtype"].ToString();

            return CoreService.GetListInfoAllowance(noreg, type, subtype);
        }
    }
    #endregion

    #region MVC Controller
    /// <summary>
    /// Allowance page controller
    /// </summary>
    [Area("MasterData")]
    [Permission(PermissionKey.ViewAllowanceDetail)]
    public class AllowanceDetailController : GenericMvcControllerBase<AllowanceDetailService, AllowanceDetail>
    {
        [HttpPost]
        public override IActionResult Load(Guid id)
        {
            object commonData;

            if (id == Guid.Empty)
            {
                commonData = new AllowanceDetail();
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