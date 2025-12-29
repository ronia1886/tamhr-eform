using System.Net.Mime;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.Requests;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;
using Agit.Common;
using Kendo.Mvc.Extensions;
using System.Collections.Generic;
using System.Linq;
using TAMHR.ESS.Domain;
using Kendo.Mvc.UI;
using TAMHR.ESS.Infrastructure.ViewModels;
using System;


using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using TAMHR.ESS.Infrastructure.Web.Models;
using TAMHR.ESS.Infrastructure.Web.Filters;
using TAMHR.ESS.Infrastructure.DomainServices;
using RestSharp;
using Newtonsoft.Json;
using Rotativa.AspNetCore;

namespace TAMHR.ESS.WebUI.Areas.PersonalData.Controllers
{
    [Route("api/form-pengkinian-data")]
    //[Permission(PermissionKey.ViewPayslip)]
    public class FormPengkinianDataApiController : ApiControllerBase
    {
        #region Domain Services
        /// <summary>
        /// Payslip service object.
        /// </summary>
        protected  UpdateDataService updateDataService =>  ServiceProxy.GetService<UpdateDataService>();
    
        #endregion

        /// <summary>
        /// Get list of forms
        /// </summary>
        /// <remarks>
        /// Get list of forms
        /// </remarks>
        /// <returns>List of Forms</returns>
        [HttpPost("gets")]
        public async Task<DataSourceResult> Gets()
        {
            return await updateDataService.GetMenuUpdateData().OrderBy(x => x.Title).ToDataSourceResultAsync(new DataSourceRequest());
        }

        [HttpPost("confirm-all")]
        public IActionResult Confirm([FromBody] PersonalDataUpdate entity)
        {
            var isValid = updateDataService.ConfirmUpdateData(entity);
            return Ok(isValid);
        }
        [HttpPost("get-general-category/{category}")]
        public async Task<DataSourceResult> GetByCategoryQuery(string category, [DataSourceRequest] DataSourceRequest request)
        {
           
                return await ConfigService.GetGeneralCategoriesQuery(category).ToDataSourceResultAsync(request);
        }
        [HttpPost("getsbycategory")]
        public async Task<DataSourceResult> GetByCategory(string category, bool all = false)
        {
            var data = await ConfigService.GetGeneralCategories(category, true).ToDataSourceResultAsync(new DataSourceRequest());

            //data.Add();

            return data;
        }
        [HttpPost("get-mapping-query")]
        public async Task<DataSourceResult> GetByMappingCategoryQuery([DataSourceRequest] DataSourceRequest request)
        {
            string code = Request.Form["ParentGeneralCategoryCode"].ToString();
            if (code == "")
            {
                return await ConfigService.GetGeneralCategoryMapping().ToDataSourceResultAsync(request);
            }
            else
            {
                return await ConfigService.GetGeneralCategoryMappingQuery(code).ToDataSourceResultAsync(request);
            }
        }
        [HttpPost("update-address")]
        public IActionResult Confirm([FromBody]AddressViewModel entity)
        {
            var isValid = updateDataService.UpdateAddress(entity);
            return Ok(isValid);
        }

        [HttpPost("update-family-regist")]
        public IActionResult UpdateFamily([FromBody]FamilyRegistViewModel entity)
        {
            var isValid = updateDataService.UpdateFamily(entity);
            return Ok(isValid);
        }

        [HttpPost("submit-kk")]
        public IActionResult SubmitKK([FromBody]PersonalDataKkMember entity)
        {
            var isValid = updateDataService.SubmitKK(entity);
            return Ok(isValid);
        }

        [HttpPost("delete-kkmember")]
        public IActionResult DeleteKKMember([FromBody]PersonalDataKkMember entity)
        {
            entity.ModifiedBy  = ServiceProxy.UserClaim.NoReg;
            return Ok(updateDataService.DeletePersonalDataKK(entity));
        }
        

    }

    [Area(ApplicationModule.PersonalData)]
    //[Permission(PermissionKey.ViewPayslip)]
    public class FormPengkinianDataController : MvcControllerBase
    {
        public FormService FormService => ServiceProxy.GetService<FormService>();
        #region Pages
        /// <summary>
        /// Payslip default page.
        /// </summary>
        public IActionResult Index()
        {
            return View();
        }
     
        [HttpGet]
        public IActionResult form(string formKey, string entity)
        {
            ViewBag.StringObject = entity;
            var fkey = FormService.Get(formKey);
            return View(fkey);
        }

        #endregion
    }
}