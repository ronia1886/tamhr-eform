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
    /// BPJS API Manager
    /// </summary>
    [Route("api/master-data/bpjs")]
    [Permission(PermissionKey.ManageBpjs)]
    public class BpjsApiController : GenericApiControllerBase<BpjsService, PersonalDataBpjs>
    {
        protected override string[] ComparerKeys => throw new NotImplementedException();

        [HttpPost("get-bpjs")]
        public async Task<DataSourceResult> GetBpjs([DataSourceRequest] DataSourceRequest request)
        {
            var service = CommonService as BpjsService;

            //return await service.GetBpjs().ToDataSourceResultAsync(request);
            return await CommonService.GetQuery().OrderBy(x => x.NoReg).ToDataSourceResultAsync(request);
        }

        public override async Task<IActionResult> Merge()
        {
            var dicts = new Dictionary<string, Type>
            {
                { "NoReg", typeof(string) },
                { "FamilyMemberId", typeof(string) },
                { "BpjsNumber", typeof(string) },
                { "FaskesCode", typeof(string) },
                { "Telephone", typeof(string) },
                { "Email", typeof(string) },
                { "PassportNumber", typeof(string) },
                { "StartDate", typeof(DateTime) },
                { "EndDate", typeof(DateTime) },
                { "ActionType", typeof(string) },
                { "CompleteStatus", typeof(bool) }
            };

            var foreignKeys = new Dictionary<string, string> {
                { "FamilyMemberId", "Id|Name:FamilyMemberId,NoReg:NoReg|dbo.VW_PERSONAL_DATA_FAMILY_MEMBER" }
            };

            var columnKeys = new[] { "NoReg", "FamilyMemberId" };

            await UploadAndMergeAsync<PersonalDataBpjs>(Request.Form.Files.FirstOrDefault(), dicts, columnKeys, foreignKeys);

            return NoContent();
        }
    }
    #endregion

    #region MVC Controller
    /// <summary>
    /// BPJS page controller
    /// </summary>
    [Area("MasterData")]
    [Permission(PermissionKey.ViewBpjs)]
    public class BpjsController : GenericMvcControllerBase<BpjsService, PersonalDataBpjs>
    {
    }
    #endregion
}