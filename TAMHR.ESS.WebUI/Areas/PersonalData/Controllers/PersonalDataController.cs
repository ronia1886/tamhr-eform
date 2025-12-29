using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.DomainServices;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;

namespace TAMHR.ESS.WebUI.Areas.PersonalData.Controllers
{
    #region API Controller
    /// <summary>
    /// Personal data API controller.
    /// </summary>
    [Route("api/personaldata")]
    public class PersonalDataController : ApiControllerBase
    {
        #region Domain Services
        /// <summary>
        /// Config service
        /// </summary>
        protected PersonalDataService PersonalDataService => ServiceProxy.GetService<PersonalDataService>();
        #endregion

        /// <summary>
        /// Get list of family members
        /// </summary>
        /// <remarks>
        /// Get list of family members
        /// </remarks>
        /// <param name="request">DataSourceRequest Object</param>
        /// <returns>DataSourceResult Object</returns>
        [HttpPost("get-family-members")]
        public async Task<DataSourceResult> GetFamilyMembers([FromForm] string noreg, [DataSourceRequest] DataSourceRequest request)
        {
            return await PersonalDataService.GetFamilyMembers(noreg).ToDataSourceResultAsync(request);
        }

        /// <summary>
        /// Get list of medical histories
        /// </summary>
        /// <remarks>
        /// Get list of medical histories
        /// </remarks>
        /// <param name="request">DataSourceRequest Object</param>
        /// <returns>DataSourceResult Object</returns>
        [HttpPost("get-medical-histories")]
        public async Task<DataSourceResult> GetMedicalHistories([DataSourceRequest] DataSourceRequest request)
        {
            var noreg = ServiceProxy.UserClaim.NoReg;

            return await PersonalDataService.GetMedicalHistories(noreg).ToDataSourceResultAsync(request);
        }

        /// <summary>
        /// Get list of Family Member
        /// </summary>
        /// <remarks>
        /// Get list of Family Member
        /// </remarks>
        /// <param name="request">DataSourceRequest Object</param>
        /// <returns>DataSourceResult Object</returns>
        [HttpPost("getfamilymember")]
        public async Task<DataSourceResult> GetFamilyMemberByNoreg()
        {
            string Noreg = ServiceProxy.UserClaim.NoReg;
            return await PersonalDataService.GetObjFamilyMemberBYNoreg(Noreg).ToDataSourceResultAsync(new DataSourceRequest());
        }

        [HttpPost("getinfofamtypecode")]
        public async Task<DataSourceResult> GetInfoFamTypeCode()
        {
            //Guid famMemberId = Guid.Parse(this.Request.Form["FamilyMemberId"].ToString());
            var aa = this.Request.Form["FamilyMemberId"].ToString();
            Guid newGuid;
            List<object> objAlert = new List<object>();
            if (Guid.TryParse(aa, out newGuid))
                return await PersonalDataService.GetObjInfoFamTypeCode(newGuid).ToDataSourceResultAsync(new DataSourceRequest());
            else
                throw new Exception("Nama Keluarga inti tidak ada di dalam data family member"); ;
        }


        [HttpPost("getfamilymemberpasangan")]
        public async Task<DataSourceResult> GetFamilyMemberByPasangan()
        {
            string Noreg = this.Request.Form["noreg"].ToString();
            return await PersonalDataService.GetFamilyMemberPasanganBYNoreg(Noreg).ToDataSourceResultAsync(new DataSourceRequest());
        }
        [HttpPost("get-pasangan")]
        public IActionResult GetPasangan()
        {
            string noReg = ServiceProxy.UserClaim.NoReg;
            var result = PersonalDataService.GetPasanganByNoReg(noReg);
            return result.Match<IActionResult>(success => Ok(success), error => BadRequest(error));
        }

        [HttpPost("getinfotitle")]
        public async Task<DataSourceResult> GetInfoTitle()
        {
            string key = this.Request.Form["formKey"].ToString();
            return await PersonalDataService.GetInfoTitle(key).ToDataSourceResultAsync(new DataSourceRequest());
        }

        [HttpPost("getinfo")]
        public async Task<DataSourceResult> GetInfo()
        {
            string Noreg = ServiceProxy.UserClaim.NoReg;
            return await PersonalDataService.GetInfo(Noreg).ToDataSourceResultAsync(new DataSourceRequest());
        }

        [HttpPost("gethospitaladdress")]
        public async Task<DataSourceResult> GetHostpitalAddress()
        {
            string hospitalName = this.Request.Form["hospitalName"].ToString();
            return await PersonalDataService.GetHospitalAddress(hospitalName).ToDataSourceResultAsync(new DataSourceRequest());
        }

        [HttpPost("getinfoemployee")]
        public async Task<DataSourceResult> GetInfoEmployee()
        {
            string noreg = Request.Form["noreg"].ToString();
            return await PersonalDataService.GetInfoEmployee(noreg).ToDataSourceResultAsync(new DataSourceRequest());
        }

        [HttpPost("getinfotunjangan")]
        public async Task<DataSourceResult> GetTunjangan()
        {
            int np = Convert.ToInt32(this.Request.Form["np"].ToString());
            return await PersonalDataService.GetInfoTunjangan(np).ToDataSourceResultAsync(new DataSourceRequest());
        }

        [HttpPost("getinfomedicalbpk")]
        public async Task<DataSourceResult> GetInfoMedicalBPK()
        {
            int np = Convert.ToInt32(this.Request.Form["np"].ToString());
            string newtaxstatus = this.Request.Form["newtaxstatus"].ToString();
            return await PersonalDataService.GetInfoMedicalBPK(np, newtaxstatus).ToDataSourceResultAsync(new DataSourceRequest());
        }

        [HttpPost("getinfobpjs")]
        public async Task<DataSourceResult> GetAsuransi()
        {
            return await PersonalDataService.GetInfoBPJS().ToDataSourceResultAsync(new DataSourceRequest());
        }

        [HttpPost("getinfoaviva")]
        public async Task<DataSourceResult> GetAviva()
        {
            return await PersonalDataService.GetInfoAVIVA().ToDataSourceResultAsync(new DataSourceRequest());
        }

        [HttpPost("getnonfamily")]
        public async Task<DataSourceResult> GetNonMainFamily()
        {
            string Noreg = ServiceProxy.UserClaim.NoReg;
            return await PersonalDataService.GetNonMainFamily(Noreg).ToDataSourceResultAsync(new DataSourceRequest());
        }

        [HttpPost("getgender")]
        public async Task<DataSourceResult> GetGender()
        {
            string Noreg = ServiceProxy.UserClaim.NoReg;
            return await PersonalDataService.GetGender(Noreg).ToDataSourceResultAsync(new DataSourceRequest());
        }

        [HttpPost("GetCountChild")]
        public async Task<DataSourceResult> GetCountChild()
        {
            string Noreg = this.Request.Form["noreg"].ToString();
            return await PersonalDataService.GetCountChild(Noreg).ToDataSourceResultAsync(new DataSourceRequest());
        }
    }
    #endregion
}