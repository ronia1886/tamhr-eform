using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.ViewModels;
using TAMHR.ESS.Infrastructure.DomainServices;
using Agit.Common;
using Agit.Common.Utility;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using Newtonsoft.Json;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using Microsoft.Extensions.Localization;

namespace TAMHR.ESS.WebUI.Areas.TimeManagement.Controllers
{
    #region API Controller
    /// <summary>
    /// Absence API controller.
    /// </summary>
    [Route("api/" + ApplicationForm.Absence)]
    //[Permission(PermissionKey.ViewAbsence)]
    public class AbsenceApiController : FormApiControllerBase<AbsenceViewModel>
    {
        #region Domain Services
        /// <summary>
        /// Time management service
        /// </summary>
        /// 

        private readonly IStringLocalizer<AbsenceApiController> _localizer;

        public AbsenceApiController(IStringLocalizer<AbsenceApiController> localizer)
        {
            _localizer = localizer;
        }
        public TimeManagementService TimeManagementService => ServiceProxy.GetService<TimeManagementService>();

        /// <summary>
        /// Employee leave service
        /// </summary>
        public EmployeeLeaveService EmployeeLeaveService => ServiceProxy.GetService<EmployeeLeaveService>();
        public ApprovalService approvalService => ServiceProxy.GetService<ApprovalService>();
        public AbsenceService absenceService => ServiceProxy.GetService<AbsenceService>();
        public AnnualLeavePlanningService annualLeavePlanningService => ServiceProxy.GetService<AnnualLeavePlanningService>();
        #endregion

        protected override string GenerateTitle(string title, Dictionary<string, object> dicts, DocumentRequestDetailViewModel<AbsenceViewModel> documentRequestDetail)
        {
            var formTitle = base.GenerateTitle(title, dicts, documentRequestDetail);
            var obj = documentRequestDetail.Object;

            return (obj.StartDate.Value.Date == obj.EndDate.Value.Date)
                ? string.Format("{0} at {1:dd/MM/yyyy} with reason <b>{2}</b>", formTitle, obj.StartDate, obj.Reason)
                : string.Format("{0} from {1:dd/MM/yyyy} to {2:dd/MM/yyyy} with reason <b>{3}</b>", formTitle, obj.StartDate, obj.EndDate, obj.Reason);
        }

        protected override void ValidateOnPostCreate(DocumentRequestDetailViewModel<AbsenceViewModel> absenceViewModel)
        {
            base.ValidateOnPostCreate(absenceViewModel);

            ValidateObject(absenceViewModel);
        }

        protected override void ValidateOnPostUpdate(DocumentRequestDetailViewModel<AbsenceViewModel> absenceViewModel)
        {
            base.ValidateOnPostUpdate(absenceViewModel);

            ValidateObject(absenceViewModel);
        }

        private void ValidateObject(DocumentRequestDetailViewModel<AbsenceViewModel> absenceViewModel)
        {
            Assert.ThrowIf(int.Parse(absenceViewModel.Object.TotalAbsence ?? "0") <= 0, "Total duration cannot be lower than 0");
            if (absenceViewModel.Object == null) throw new Exception("Cannot update request because request is empty");

            if (absenceViewModel.Object.StartDate > absenceViewModel.Object.EndDate)
            {
                throw new Exception("Tanggal mulai tidak boleh lebih besar dari tanggal akhir");
            }

            //Cek apakah sudah ada draft dengan tanggal yang sama
            var existingDrafts = approvalService.GetInprogressDraftRequestDetails("absence")
                .Where(x => x.CreatedBy == ServiceProxy.UserClaim.NoReg)
                .Select(x => JsonConvert.DeserializeObject<AbsenceViewModel>(x.ObjectValue))
                .Where(x =>
                            x.StartDate <= absenceViewModel.Object.EndDate &&
                            x.EndDate >= absenceViewModel.Object.StartDate)
                .ToList();

            if (existingDrafts.Any())
            {
                throw new Exception(_localizer["You already have a draft request with the same date."]);
            }


            EmployeeLeaveService.ValidateWorkSchedule(ServiceProxy.UserClaim.NoReg, absenceViewModel.Object.StartDate.Value, absenceViewModel.Object.EndDate.Value);

            var totalAbsence = GenerateAbsenceDay(absenceViewModel.Object.ReasonId.Value, absenceViewModel.Object.StartDate.Value, absenceViewModel.Object.EndDate.Value);
            absenceViewModel.Object.TotalAbsence = totalAbsence.ToString();

            var absence = CoreService.GetAbsenceById(absenceViewModel.Object.ReasonId.Value);
            var maxAbsentDays = absence.MaxAbsentDays ?? 0;

            Assert.ThrowIf(maxAbsentDays > 0 && totalAbsence > maxAbsentDays, $"Maximum total absence day for <b>{absence.Name}</b> is {maxAbsentDays} days");

            if (ObjectHelper.IsIn(absenceViewModel.Object.ReasonType, "cuti", "cutipanjang"))
            {
                var startYear = absenceViewModel.Object.StartDate?.Year ?? DateTime.Now.Year;

                var employeeLeave = EmployeeLeaveService.GetByNoregAndYear(ServiceProxy.UserClaim.NoReg, startYear); // <--- pakai tahun dari tanggal cuti

                var totalDurasi = int.Parse(absenceViewModel.Object.TotalAbsence ?? "0");

                Assert.ThrowIf(absenceViewModel.Object.ReasonType == "cuti" && employeeLeave.AnnualLeave < totalDurasi,
                    $"Annual leave quota for year {startYear} is lower than total duration");

                Assert.ThrowIf(absenceViewModel.Object.ReasonType == "cutipanjang" && employeeLeave.LongLeave < totalDurasi,
                    $"Long leave quota for year {startYear} is lower than total duration");
                TimeManagementService.PreValidateAbsentLimit(ServiceProxy.UserClaim.NoReg, absenceViewModel.Object.StartDate.Value, absenceViewModel.Object.EndDate.Value);

            }

            TimeManagementService.PreValidateAbsence(ServiceProxy.UserClaim.NoReg, absenceViewModel.Object.StartDate.Value, absenceViewModel.Object.EndDate.Value);
            

            //TimeManagementService.PreValidateCrossYear(ServiceProxy.UserClaim.NoReg, absenceViewModel.Object.StartDate.Value, absenceViewModel.Object.ReasonType, int.Parse(absenceViewModel.Object.TotalAbsence));

            absenceViewModel.Refresh();
        }


        [HttpPost("getinfocalculationloan")]
        public async Task<DataSourceResult> GetScheduleWork()
        {
            var noreg = ServiceProxy.UserClaim.NoReg;
            var startDate = Convert.ToDateTime(Request.Form["strDate"]);
            var endDate = Convert.ToDateTime(Request.Form["endDate"]);

            return await TimeManagementService.GetListWorkSchEmp(noreg, startDate, endDate)
                .ToDataSourceResultAsync(new DataSourceRequest());
        }

        [HttpPost("get-available-leave-plan")]
        public async Task<DataSourceResult> GetAvailableLeavePlan()
        {
            string noReg = Request.Form["noReg"];
            string absenceType = Request.Form["absenceType"];

            return await annualLeavePlanningService.GetAvailableAnnualLeavePlan(absenceType, noReg)
                .ToDataSourceResultAsync(new DataSourceRequest());
        }

        [HttpPost("get-leave-plan")]
        public async Task<DataSourceResult> GetLeavePlan()
        {
            string noReg = Request.Form["noReg"];
            string absenceType = Request.Form["absenceType"];

            return await annualLeavePlanningService.GetAnnualLeavePlan(absenceType, noReg)
                .ToDataSourceResultAsync(new DataSourceRequest());
        }

        [HttpPost("get-absence-end-date")]
        public DateTime GetAbsenceEndDate([FromForm] DateTime startDate, [FromForm] int maxAbsentDays)
        {
            var noreg = ServiceProxy.UserClaim.NoReg;

            return TimeManagementService.GetAbsenceEndDate(noreg, startDate, maxAbsentDays);
        }

        [HttpPost("getstartdate-crossyear")]
        public int GetStartAnnualLeaveNextYear([FromForm] DateTime startDate)
        {
            var noreg = ServiceProxy.UserClaim.NoReg;
            return annualLeavePlanningService.GetStartAnnualLeaveNextYear(noreg, startDate);
        }

        [HttpPost("getenddate-crossyear")]
        public int GetEndAnnualLeaveNextYear([FromForm] DateTime endDate)
        {
            var noreg = ServiceProxy.UserClaim.NoReg;
            return annualLeavePlanningService.GetEndAnnualLeaveNextYear(noreg, endDate);
        }

        [HttpPost("limit-crossyear")]
        public int PreValidateLimitCrossYear()
        {
            var noreg = ServiceProxy.UserClaim.NoReg;
            return TimeManagementService.PreValidateLimitCrossYear(noreg);
        }

        [HttpPost("getbynoreg")]
        public EmployeeLeave GetBynoreg()
        {
            string noreg = this.Request.Form["noreg"].ToString();
            Guid idDoc = Guid.Parse(this.Request.Form["iddoc"].ToString());
            var objLeave = EmployeeLeaveService.GetByNoregCurrentYear(noreg);

            var objLeaveOnprogress = approvalService.GetInprogressDraftRequestDetails("absence").Where(x => x.CreatedBy == noreg).Select(x => JsonConvert.DeserializeObject<AbsenceViewModel>(x.ObjectValue));
            if (objLeaveOnprogress != null)
            {
                var totalCuti = 0;
                var totalCutiPanjang = 0;
                int currentYear = DateTime.Now.Year;

                foreach (var item in objLeaveOnprogress)
                {
                    // Ambil start dan end date
                    var start = item.StartDate.Value;
                    var end = item.EndDate.Value;

                    // Ambil list tanggal kerja dari service
                    var workDays = TimeManagementService.GetListWorkSchEmp(noreg, start, end)
                        .Where(w => w.Date.Year == currentYear && !w.Off && !w.Holiday) // hanya hari kerja tahun ini
                        .Select(w => w.Date)
                        .ToList();

                    int validDaysCount = workDays.Count;

                    if (item.ReasonType == "cuti")
                    {
                        totalCuti += validDaysCount;
                    }

                    if (item.ReasonType == "cutipanjang")
                    {
                        totalCutiPanjang += validDaysCount;
                    }
                }

                objLeave.AnnualLeave -= totalCuti;
                objLeave.LongLeave -= totalCutiPanjang;
            }

            return objLeave;
        }

        [HttpPost("get-absence")]
        public async Task<DataSourceResult> GetAbsenceByPeriod()
        {
            string noreg = this.Request.Form["noreg"].ToString();
            int period = Convert.ToInt32(this.Request.Form["period"].ToString());
            var listAbsence = TimeManagementService.GetListAbsenceByPeriod(noreg, period).ToList();

            var objLeaveOnprogress = approvalService.GetInprogressDraftRequestDetails("absence")
                                                    .Where(x => x.CreatedBy == noreg)
                                                    .Select(x => JsonConvert.DeserializeObject<AbsenceViewModel>(x.ObjectValue));
            if (objLeaveOnprogress.Count() > 0)
            {
                foreach (var item in objLeaveOnprogress)
                {
                    string code = TimeManagementService.GetAbsenceCode(item.ReasonId.Value);
                    var employeeAbsent = new EmployeeAbsenceView()
                    {
                        NoReg = noreg,
                        StartDate = item.StartDate.Value,
                        EndDate = item.EndDate.Value,
                        AbsentDuration = int.Parse(item.TotalAbsence),
                        ReasonCode = code,
                        Description = item.Description,
                        KategoriPenyakit = item.KategoriPenyakit,
                        SpesifikPenyakit = item.SpesifikPenyakit

                    };
                    listAbsence.Add(employeeAbsent);
                }



            }

            return await listAbsence.ToDataSourceResultAsync(new DataSourceRequest());
        }

        [HttpDelete("drop-document")]
        public async Task<IActionResult> DropDocument([FromForm] Guid id)
        {
            var noreg = ServiceProxy.UserClaim.NoReg;

            await TimeManagementService.DeleteAbsence(id, noreg);

            return NoContent();
        }

        private int GenerateAbsenceDay(Guid idReason, DateTime strDate, DateTime endDate)
        {
            string Noreg = ServiceProxy.UserClaim.NoReg;

            // Get absence details
            var objabsence = absenceService.GetById(idReason);
            var maxAbsenceDay = objabsence.MaxAbsentDays ?? 0; // Handle null case

            // Get work schedule
            var dataWorkList = TimeManagementService.GetListWorkSchEmp(Noreg, strDate, endDate);
            var dataOff = dataWorkList.Where(x => x.Off);

            // Calculate total days
            var totalDate = strDate - endDate;
            int totalDays = (-1 * totalDate.Days) + 1;

            // Validation: Exclude weekends *only in the next year*
            for (DateTime date = strDate; date <= endDate; date = date.AddDays(1))
            {
                if (date.Year > strDate.Year && (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday))
                {
                    totalDays--; // Remove next year's weekends from the count
                }
            }

            return totalDays - dataOff.Count();
        }

    }
    #endregion
}