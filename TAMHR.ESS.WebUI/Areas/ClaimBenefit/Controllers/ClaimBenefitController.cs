using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.ViewModels;
using TAMHR.ESS.Infrastructure.DomainServices;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using Newtonsoft.Json;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;
using TAMHR.ESS.Infrastructure.Web.Authorization;

namespace TAMHR.ESS.WebUI.Areas.ClaimBenefit.Controllers
{
    #region API Controller
    /// <summary>
    /// Claim Benefit API Manager
    /// </summary>
    [Route("api/claimbenefit")]
    //[Permission(PermissionKey.ViewClaimBenefit)]
    public class ClaimBenefitApiController : ApiControllerBase
    {
        #region Domain Services
        protected ClaimBenefitService ClaimBenefitService => ServiceProxy.GetService<ClaimBenefitService>();
        protected ClaimBenefitQueryService ClaimBenefitServicePartial => ServiceProxy.GetService<ClaimBenefitQueryService>();
        protected MdmService MdmService => ServiceProxy.GetService<MdmService>();
        protected ApprovalService approvalService => ServiceProxy.GetService<ApprovalService>();

        #endregion

        [HttpPost("geteyeglassesalert")]
        public List<object> GetEyeGlassesAlert()
        {
            var now = DateTime.Now.Date;
            var objClaim = ClaimBenefitService.GetLastEyeGlassesClaim(ServiceProxy.UserClaim.NoReg);
            var objFrame = objClaim.Where(x => x.AllowanceType == "frame").OrderByDescending(d => d.TransactionDate).FirstOrDefault();
            var objLensa = objClaim.Where(x => x.AllowanceType == "lensa").OrderByDescending(d => d.TransactionDate).FirstOrDefault();
            var currentCulture = System.Globalization.CultureInfo.CurrentUICulture;
            var localizer = ServiceProxy.GetLocalizer();

            List<object> objAlert = new List<object>();
            if (objFrame != null)
            {
                var newDate = objFrame.TransactionDate.AddYears(2);
                var newOutDate = new DateTime(newDate.Year, newDate.Month, 1);

                if (newOutDate > now)
                {
                    objAlert.Add(new { type = "frame", val = localizer["You can claim the frame again at"].Value + " " + newOutDate.Date.ToString("MMMMM yyyy", currentCulture) });
                }
            }

            if (objLensa != null)
            {
                var newDate = objLensa.TransactionDate.AddYears(1);
                var newOutDate = new DateTime(newDate.Year, newDate.Month, 1);

                if (newOutDate > now)
                {
                    objAlert.Add(new { type = "lensa", val = localizer["You can claim the lens again at"].Value + " " + newOutDate.Date.ToString("MMMMM yyyy", currentCulture) });
                }
            }

            return objAlert;
        }

        /// <summary>
        /// Get Info Header
        /// </summary>
        /// <remarks>
        /// Get Info Header
        /// </remarks>
        [HttpPost("getinfo")]
        public async Task<DataSourceResult> GetInfo()
        {
            string Noreg = ServiceProxy.UserClaim.NoReg;
            return await ClaimBenefitService.GetInfo(Noreg).ToDataSourceResultAsync(new DataSourceRequest());
        }

        [HttpPost("getinfoammountconceptidea")]
        public async Task<DataSourceResult> GetInfoAmmountConceptIdea()
        {
            int value = Convert.ToInt32(this.Request.Form["value"].ToString());
            string type = this.Request.Form["type"].ToString();
            return await ClaimBenefitServicePartial.GetInfoAmmountConceptIdea(value, type).ToDataSourceResultAsync(new DataSourceRequest());
        }

        [HttpPost("getmaxlens")]
        public async Task<DataSourceResult> GetMaxLens()
        {
            int np = Convert.ToInt32(this.Request.Form["np"].ToString());
            return await ClaimBenefitService.GetMaxLens(np).ToDataSourceResultAsync(new DataSourceRequest());
        }

        [HttpPost("getmaxframe")]
        public async Task<DataSourceResult> GetMaxFrame()
        {
            int np = Convert.ToInt32(this.Request.Form["np"].ToString());
            return await ClaimBenefitService.GetMaxFrame(np).ToDataSourceResultAsync(new DataSourceRequest());
        }

        [HttpPost("getayosekolah")]
        public async Task<DataSourceResult> GetAyoSekolah()
        {
            string Noreg = ServiceProxy.UserClaim.NoReg;
            return await MdmService.GetEmployeeOrganizationObjects(Noreg).ToDataSourceResultAsync(new DataSourceRequest());
        }


        [HttpPost("get-dates-period")]
        public IActionResult GetDatesFromPeriod([FromQuery] DateTime period)
        {
            var dates = ClaimBenefitServicePartial.GetDatesFromPeriod(period).OrderBy(x => x.Date);
            return Ok(dates);
        }

        [HttpPost("getchild")]
        public async Task<DataSourceResult> GetChild()
        {
            string Noreg = ServiceProxy.UserClaim.NoReg;
            return await ClaimBenefitServicePartial.GetChild(Noreg).ToDataSourceResultAsync(new DataSourceRequest());
        }

        [HttpPost("getchildbiological")]
        public async Task<DataSourceResult> GetChildBiological()
        {
            string Noreg = ServiceProxy.UserClaim.NoReg;
            string FamilyTypeCode = "anakkandung";
            return await ClaimBenefitServicePartial.GetChildStatus(Noreg, FamilyTypeCode).ToDataSourceResultAsync(new DataSourceRequest());
        }

        [HttpPost("getchildadopted")]
        public async Task<DataSourceResult> GetChildAdopted()
        {
            string Noreg = ServiceProxy.UserClaim.NoReg;
            string FamilyTypeCode = "anakangkat";
            return await ClaimBenefitServicePartial.GetChildStatus(Noreg, FamilyTypeCode).ToDataSourceResultAsync(new DataSourceRequest());
        }

        [HttpPost("getpartner")]
        public async Task<DataSourceResult> GetPartner()
        {
            string Noreg = ServiceProxy.UserClaim.NoReg;
            bool ismainfamily = Convert.ToBoolean(this.Request.Form["ismainfamily"].ToString());
            return await ClaimBenefitServicePartial.GetPartner(Noreg, ismainfamily).ToDataSourceResultAsync(new DataSourceRequest());
        }

        [HttpPost("getinfoemployee")]
        public async Task<DataSourceResult> GetInfoEmployee()
        {
            string Noreg = ServiceProxy.UserClaim.NoReg;
            bool ismainfamily = Convert.ToBoolean(this.Request.Form["ismainfamily"].ToString());
            return await ClaimBenefitServicePartial.GetInfoEmployee(Noreg, ismainfamily).ToDataSourceResultAsync(new DataSourceRequest());
        }

        [HttpPost("getinfochild")]
        public async Task<DataSourceResult> GetInfoChild()
        {
            string Noreg = ServiceProxy.UserClaim.NoReg;
            string Name = this.Request.Form["PatientChildName"];
            return await ClaimBenefitServicePartial.GetInfoChild(Noreg, Name).ToDataSourceResultAsync(new DataSourceRequest());
        }

        [HttpPost("getcheckupcount")]
        public IActionResult GetCheckUpCount([FromQuery] DateTime period)
        {
            string Noreg = ServiceProxy.UserClaim.NoReg;
            string ControlCriteria = this.Request.Form["ControlCriteria"];
            string Diagnosa = this.Request.Form["Diagnosa"];
            var dates = ClaimBenefitServicePartial.GetCheckUpCount(Noreg, ControlCriteria, Diagnosa).Count();
            return Ok(dates);
        }

        [HttpPost("getinfocalculationloan")]
        public async Task<DataSourceResult> GetCalculationLoan()
        {
            string Noreg = ServiceProxy.UserClaim.NoReg;
            int Class = Convert.ToInt32(this.Request.Form["kelas"]);
            return await ClaimBenefitServicePartial.GetCalculationLoan(Class).ToDataSourceResultAsync(new DataSourceRequest());
        }

        //[HttpPost("getclaimreward")]
        //public async Task<DataSourceResult> GetClaimReward()
        //{
        //    //string name = ServiceProxy.UserClaim.Name;
        //    string noreg = this.Request.Form["noreg"];
        //    string type = this.Request.Form["type"];
        //    return await ClaimBenefitServicePartial.GetClaimReward(noreg, type).ToDataSourceResultAsync(new DataSourceRequest());
        //}

        [HttpPost("departments-with-employees-pta")]
        public object DepartmentsEmployeesPta()
        {
            string type = this.Request.Form["type"];
            DateTime date = Convert.ToDateTime(this.Request.Form["date"]);
            decimal amount = Convert.ToDecimal(this.Request.Form["amount"]);

            var postcode = ServiceProxy.UserClaim.PostCode;
            var objs = MdmService.GetOrganizationObjects("O", "Department", ServiceProxy.UserClaim.NoReg, postcode);
            //var objs = MdmService.GetEmployeeOrganizationObjects(ServiceProxy.UserClaim.NoReg);   x
            var result = new
            {
                Departments = new List<Object>()
            };

            foreach (var item in objs)
            {
                var ObjEmp = MdmService.GetEmployeeByDepartmentPta(item.ObjectID, type, date, amount).OrderBy(s => s.Classification).ToList();
                var objDocOnprogress = approvalService.GetInprogressRequestDetails("pta-allowance");
                var objDocComplete = approvalService.GetInprogressRequestDetails("pta-allowance");

                var ObjOnProgress = objDocOnprogress.Select(x => JsonConvert.DeserializeObject<PtaAllowanceViewModel>(x.ObjectValue)).Where(p => p.date == date).ToList();
                var ObjComplete = objDocComplete.Select(x => JsonConvert.DeserializeObject<PtaAllowanceViewModel>(x.ObjectValue)).Where(p => p.date == date).ToList();

                var EmpOnProgress = ObjOnProgress.Select(e => e.Summaries.Select(x => x.Employees).ToList()).ToList();
                var EmpComplete = ObjComplete.Select(e => e.Summaries.Select(x => x.Employees)).ToList();

                List<string> ListEmp = new List<string>();

                foreach (var list in EmpOnProgress)
                {
                    foreach (var emp in list.Where(x => x != null))
                    {
                        foreach (var itemEmp in emp)
                        {
                            ListEmp.Add(itemEmp.NoReg);
                        }
                    }
                }

                foreach (var list in EmpComplete)
                {
                    foreach (var emp in list.Where(x => x != null))
                    {
                        foreach (var itemEmp in emp)
                        {
                            ListEmp.Add(itemEmp.NoReg);
                        }
                    }
                }

                ObjEmp = ObjEmp.Where(x => !ListEmp.Contains(x.NoReg)).ToList();


                result.Departments.Add(new
                {
                    Code = item.ObjectID,
                    ObjectID = item.ObjectID,
                    ObjectText = item.ObjectText,
                    ObjectDescription = item.ObjectDescription,
                    Employees = ObjEmp
                });
            }

            return result;
        }

        [HttpPost("division-with-employees-pta")]
        public object DivisionsEmployeesPta()
        {
            string divcode = this.Request.Form["divcode"];
            string type = this.Request.Form["type"];
            DateTime date = Convert.ToDateTime(this.Request.Form["date"]);
            decimal amount = Convert.ToDecimal(this.Request.Form["amount"]);

            var objs = MdmService.GetEmployeeOrganizationObjects(ServiceProxy.UserClaim.NoReg, ServiceProxy.UserClaim.PostCode).Where(x => x.ObjectDescription == "Division" /*&& Convert.ToInt32(x.Staffing) == 100*/);
            var result = new
            {
                divisions = new List<Object>()
            };

            foreach (var item in objs)
            {
                var ObjEmp = MdmService.GetEmployeeByDivisionPta(item.ObjectID, type, date, amount).OrderBy(s => s.Classification).ToList();
                var objDocOnprogress = approvalService.GetInprogressRequestDetails("pta-allowance");
                var objDocComplete = approvalService.GetInprogressRequestDetails("pta-allowance");

                var ObjOnProgress = objDocOnprogress.Select(x => JsonConvert.DeserializeObject<PtaAllowanceViewModel>(x.ObjectValue)).Where(p => p.date == date).ToList();
                var ObjComplete = objDocComplete.Select(x => JsonConvert.DeserializeObject<PtaAllowanceViewModel>(x.ObjectValue)).Where(p => p.date == date).ToList();

                var EmpOnProgress = ObjOnProgress.Select(e => e.Summaries.Select(x => x.Employees).ToList()).ToList();
                var EmpComplete = ObjComplete.Select(e => e.Summaries.Select(x => x.Employees)).ToList();

                List<string> ListEmp = new List<string>();

                foreach (var list in EmpOnProgress)
                {
                    foreach (var emp in list.Where(x => x != null))
                    {
                        foreach (var itemEmp in emp)
                        {
                            ListEmp.Add(itemEmp.NoReg);
                        }
                    }
                }

                foreach (var list in EmpComplete)
                {
                    foreach (var emp in list.Where(x => x != null))
                    {
                        foreach (var itemEmp in emp)
                        {
                            ListEmp.Add(itemEmp.NoReg);
                        }
                    }
                }

                ObjEmp = ObjEmp.Where(x => !ListEmp.Contains(x.NoReg)).ToList();

                result.divisions.Add(new
                {
                    Code = item.ObjectID,
                    ObjectID = item.ObjectID,
                    ObjectText = item.ObjectText,
                    ObjectDescription = item.ObjectDescription,
                    Employees = MdmService.GetEmployeeByDivisionPta(item.ObjectID, type, date, amount).OrderBy(s => s.Classification)
                });
            }

            return result;
        }


        [HttpPost("getclaimreward")]
        public decimal GetClaimReward()
        {
            //string name = ServiceProxy.UserClaim.Name;
            string noreg = this.Request.Form["noreg"];
            string type = this.Request.Form["type"];
            //DateTime date = Convert.ToDateTime(this.Request.Form["date"]);

            return ClaimBenefitServicePartial.GetClaimReward(noreg, type);
        }

        [HttpPost("getmaxreward")]
        public async Task<DataSourceResult> GetMaxClaimReward()
        {
            string type = this.Request.Form["type"];
            return await ClaimBenefitServicePartial.GetMaxClaimReward(type).ToDataSourceResultAsync(new DataSourceRequest());
        }

        [HttpPost("getContolPascaRawatInap")]
        public async Task<DataSourceResult> ContolPascaRawatInap()
        {
            string noreg = ServiceProxy.UserClaim.NoReg;
            string ControlCriteria = this.Request.Form["ControlCriteria"];
            return await ClaimBenefitServicePartial.ContolPascaRawatInap(noreg, ControlCriteria).ToDataSourceResultAsync(new DataSourceRequest());
        }

        [HttpPost("getContolNonPascaRawatInap")]
        public async Task<DataSourceResult> ContolNonPascaRawatInap()
        {
            string noreg = ServiceProxy.UserClaim.NoReg;
            string ControlCriteria = this.Request.Form["ControlCriteria"];
            string Diagnosa = this.Request.Form["Diagnosa"];
            return await ClaimBenefitServicePartial.ContolNonPascaRawatInap(noreg, Diagnosa, ControlCriteria).ToDataSourceResultAsync(new DataSourceRequest());
        }
    } 
    #endregion
}
