using System;
using System.IO;
using System.Data;
using System.Linq;
using System.Collections;
using System.Globalization;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using TAMHR.ESS.Infrastructure.ViewModels;

namespace TAMHR.ESS.WebUI.Areas.MasterData.Controllers
{
    #region API Controller
    /// <summary>
    /// Employee Work Schedule API Manager
    /// </summary>
    [Route("api/master-data/employee-work-plan")]
    [Permission(PermissionKey.ManageEmployeeWorkPlan)]
    public class EmployeeWorkPlanApiController : ApiControllerBase
    {
        public EmployeeWorkPlanService employeeWorkPlanService => ServiceProxy.GetService<EmployeeWorkPlanService>();
        //protected override string[] ComparerKeys => new[] { "Code", "Name" };
        /// <summary>
        /// Get list of WFO Plan
        /// </summary>
        /// <remarks>
        /// Get list of WFO Plan
        /// </remarks>
        /// <param name="request">DataSourceRequest Object</param>
        /// <returns>DataSourceResult Object</returns>
        [HttpPost("gets")]
        public  async Task<DataSourceResult> GetFromPosts([DataSourceRequest] DataSourceRequest request)
        {
            return await employeeWorkPlanService.GetMasterPlan().ToDataSourceResultAsync(request);
        }

        /// <summary>
        /// Update Plan
        /// </summary>
        /// <remarks>
        /// Update role
        /// </remarks>
        /// <param name="viewModel">Role View Model</param>
        /// <returns>No Content</returns>
        [HttpPut]
        public IActionResult Update([FromBody] WorkDivisionPlanViewModel viewModel)
        {
            employeeWorkPlanService.UpsertPlan(viewModel);

            return NoContent();
        }


        /// <summary>
        /// Get list of division by plan id
        /// </summary>
        /// <remarks>
        /// Get list of division by plan id
        /// </remarks>
        /// <param name="request">DataSourceRequest Object</param>
        /// <returns>DataSourceResult Object</returns>
        [HttpPost("get-divisions")]
        public async Task<DataSourceResult> GetDivision([DataSourceRequest] DataSourceRequest request)
        {
            var planId = Guid.Parse(Request.Form["planId"]);

            return await employeeWorkPlanService.GetDivisions(planId).ToDataSourceResultAsync(request);
        }



        #endregion

        #region MVC Controller
        /// <summary>
        /// Employee work schedule page controller
        /// </summary>
        [Area("MasterData")]
        [Permission(PermissionKey.ViewEmployeeWorkPlan)]
        public class EmployeeWorkPlanController : MvcControllerBase
        {
            public EmployeeWorkPlanService employeeWorkPlanService =>ServiceProxy.GetService<EmployeeWorkPlanService>();


            /// <summary>
            /// Load Work Plan data form by id
            /// </summary>
            /// <param name="id">Data Form Id</param>
            /// <returns>Data Form</returns>
            [HttpPost]
            public  IActionResult LoadPlan([FromForm]Guid id)
            {
                var commonData = employeeWorkPlanService.GetMasterPlanById(id);

                return PartialView("_EmployeeWorkPlanForm", commonData);
            }
        }
        #endregion
    }
}
