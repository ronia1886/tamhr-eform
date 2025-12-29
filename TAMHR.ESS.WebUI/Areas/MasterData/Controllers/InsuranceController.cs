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

namespace TAMHR.ESS.WebUI.Areas.MasterData.Controllers
{
    #region API Controller
    /// <summary>
    /// Insurance API Manager
    /// </summary>
    [Route("api/master-data/insurance")]
    [Permission(PermissionKey.ManageInsurance)]
    public class InsuranceApiController : GenericApiControllerBase<InsuranceService, PersonalDataInsurance>
    {
        protected override string[] ComparerKeys => throw new NotImplementedException();

        [HttpPost("get-insurances")]
        public async Task<DataSourceResult> GetInsurances([DataSourceRequest] DataSourceRequest request)
        {
            var service = CommonService as InsuranceService;

            return await service.GetInsruances().ToDataSourceResultAsync(request);
        }

        public override async Task<IActionResult> Merge()
        {
            var dicts = new Dictionary<string, Type>
            {
                { "NoReg", typeof(string) },
                { "FamilyMemberId", typeof(Guid?) },
                { "MemberNumber", typeof(string) },
                { "BenefitClassification", typeof(string) },
                { "StartDate", typeof(DateTime) },
                { "EndDate", typeof(DateTime) },
                { "ActionType", typeof(string) },
                { "CompleteStatus", typeof(bool) }
            };

            //var foreignKeys = new Dictionary<string, string> {
            //    { "FamilyMemberId", "Id|Name:FamilyMemberId,NoReg:NoReg|dbo.VW_PERSONAL_DATA_FAMILY_MEMBER" }
            //};
            var foreignKeys = new Dictionary<string, string> {
                { "FamilyMemberId", "FamilyMemberId|FamilyMemberId:FamilyMemberId,NoReg:NoReg|dbo.TB_M_PERSONAL_DATA_INSURANCE" }
            };

            var columnKeys = new[] { "NoReg", "FamilyMemberId" };

            //await UploadAndMergeAsync<PersonalDataInsurance>(Request.Form.Files.FirstOrDefault(), dicts, columnKeys, foreignKeys);
            await UploadAndMergeAsync<PersonalDataInsurance>(Request.Form.Files.FirstOrDefault(), dicts, columnKeys, null);

            return NoContent();
        }
    }
    #endregion

    #region MVC Controller
    /// <summary>
    /// Insurance page controller
    /// </summary>
    [Area("MasterData")]
    [Permission(PermissionKey.ViewInsurance)]
    public class InsuranceController : GenericMvcControllerBase<InsuranceService, PersonalDataInsurance>
    {
    }
    #endregion
}