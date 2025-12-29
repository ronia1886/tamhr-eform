using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using TAMHR.ESS.Infrastructure.Helpers;
using TAMHR.ESS.Infrastructure.ViewModels;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;
using Kendo.Mvc.UI;
using System;
using System.IO;
using Kendo.Mvc;
using System.Collections.Generic;
using Kendo.Mvc.Extensions;
using OfficeOpenXml;
using Newtonsoft.Json;
using System.Drawing;
using OfficeOpenXml.Drawing;
using System.Data;
using System.Globalization;
using Agit.Common;

namespace TAMHR.ESS.WebUI.Areas.Core.Controllers
{
    [Route("api/abnormality-over-time")]
    [Permission(PermissionKey.ManageAbnormalityOT)]
    public class AbnormalityOtApiController : FormApiControllerBase<AbnormalityOverTimeViewModel>
    {
        #region Domain Services
        //protected FormService FormService => ServiceProxy.GetService<FormService>();
        protected AbnormalityOverTimeService abnormalityOverTimeService => ServiceProxy.GetService<AbnormalityOverTimeService>();

        protected ProxyTimeService proxyTimeService => ServiceProxy.GetService<ProxyTimeService>();

        protected AbnormalityBdjkService abnormalityBdjkServiceService => ServiceProxy.GetService<AbnormalityBdjkService>();

        protected EmployeeWorkScheduleService employeeWorkScheduleService => ServiceProxy.GetService<EmployeeWorkScheduleService>();

        protected SpklMasterDataService spklMasterDataService => ServiceProxy.GetService<SpklMasterDataService>();

        protected UserService userService => ServiceProxy.GetService<UserService>();
        protected ConfigService configService => ServiceProxy.GetService<ConfigService>();

        #endregion

        #region
        private class DateInformation
        {
            public DateTime OvertimeDate { get; set; }
            public DateTime ProxyIn { get; set; }
            public DateTime ProxyOut { get; set; }
            public bool Available { get; set; }
        }
        #endregion

        #region MyRegion

        [HttpPost("request")]
        public IActionResult Createdata([FromBody] Domain.AbnormalityOverTimeView entity)
        {
            var rs = new AbnormalityOverTime();
            rs.Id = entity.AbnormalityOverTimeId.HasValue ? entity.AbnormalityOverTimeId.Value : Guid.Empty;
            var cekOverDatetime = entity.OvertimeDate.Date; // handler if enable defaullt value 1/1/0001
            entity.OvertimeDate = cekOverDatetime;
            rs.OvertimeDate = entity.OvertimeDate;

            var wk = entity.OvertimeDate.ToShortDateString();

            var clkIn = entity.OvertimeIn.ToString("hh:mm tt");
            DateTime wrkIn = DateTime.Parse(wk + " " + clkIn);
            rs.OvertimeIn = wrkIn;

            var clkout = entity.OvertimeOut.ToString("hh:mm tt");
            DateTime wrkOut = DateTime.Parse(wk + " " + clkout);
            rs.OvertimeOut = wrkOut;

            var proxyIn = entity.ProxyIn.ToString("hh:mm tt");
            DateTime prxIn = DateTime.Parse(wk + " " + proxyIn);
            rs.ProxyIn = prxIn;

            var proxyOut = entity.OvertimeOut.ToString("hh:mm tt");
            DateTime prxOut = DateTime.Parse(wk + " " + proxyOut);
            rs.ProxyOut = prxOut;

            var dataReady = abnormalityOverTimeService.GetQuery().Where(x => x.OvertimeDate == entity.OvertimeDate && (x.AbnormalityOverTimeId != null) && x.NoReg == entity.NoReg).Count();
            if (dataReady > 0 && rs.Id == Guid.Empty)
            {
                Assert.ThrowIf(true, "Working date is already request over time");
            }
            rs.DocumentApprovalId = entity.DocumentApprovalId;
            rs.TimeManagementSpklRequestId = entity.AbnormalityOverTimeId.HasValue ? Guid.Empty : entity.Id;
            rs.Status = "Draft";
            rs.NoReg = string.IsNullOrEmpty(entity.NoReg) ? ServiceProxy.UserClaim.NoReg : entity.NoReg;
            rs.OvertimeInAdjust = entity.OvertimeInAdjust;
            rs.OvertimeOutAdjust = entity.OvertimeOutAdjust;
            rs.OvertimeBreak = entity.OvertimeBreak;
            rs.OvertimeBreakAdjust = entity.OvertimeBreakAdjust;
            rs.Duration = entity.Duration;
            rs.DurationAdjust = entity.DurationAdjust;
            rs.OvertimeCategoryCode = entity.OvertimeCategoryCode;
            rs.OvertimeReason = entity.OvertimeReason;
            rs.RowStatus = entity.RowStatus;
            rs.CreatedBy = entity.CreatedBy;
            rs.CreatedOn = entity.CreatedOn;


           
            var haveProxt = proxyTimeService.GetQuery()
                    .Where(x => x.WorkingDate == cekOverDatetime && x.NoReg == rs.NoReg && x.WorkingTimeOut != null).Count();
            if (entity.Level > 6)
            {
                Assert.ThrowIf(true, "Cannot create over time request, level must be under 7");
            }
            //var minHourOT = abnormalityOverTimeService.GetDataConfig("Abnormality.OT.Hours");
            //var NormalTimeOut = abnormalityBdjkServiceService.GetBySfitCode("1NS1");
            //var normalHour = NormalTimeOut.NormalTimeOut.Value.TotalHours;
            //var JobHour = rs.OvertimeOut.TimeOfDay.TotalHours - normalHour - (entity.OvertimeBreak / 60);

            var minHourOT = abnormalityOverTimeService.GetDataConfig("Abnormality.OT.Hours");
            var JobHour = entity.Duration;
            if (JobHour < Convert.ToDecimal(minHourOT.ConfigValue))
            {
                Assert.ThrowIf(true, "Cannot create over time requst, duration over time must be  more then "+ minHourOT.ConfigValue + " hour");
            }
            rs.Duration = Convert.ToDecimal(JobHour);

            ValidateOnPostCreate((haveProxt < 1));
            abnormalityOverTimeService.Upsert(ServiceProxy.UserClaim.NoReg, rs);
            return NoContent();
        }

        [HttpPost("insert-file")]
        public IActionResult InsertFile([FromBody] Domain.AbnormalityFile entity)
        {
            AbnormalityFile datafile = new AbnormalityFile();
            datafile.CommonFileId = entity.CommonFileId;
            datafile.TransactionId = entity.TransactionId;
            datafile.RowStatus = true;
            datafile.CreatedBy = ServiceProxy.UserClaim.NoReg;
            datafile.CreatedOn = DateTime.Now;
            abnormalityOverTimeService.AddFile(ServiceProxy.UserClaim.NoReg, datafile);
            return NoContent();
        }

        [HttpPost("get-abnormality-file")]
        public async Task<DataSourceResult> GetHistoriesFromPosts([FromForm] string noreg, [FromForm] AbnormalityFileView entity, [DataSourceRequest] DataSourceRequest request)
        {
            return await abnormalityOverTimeService.GetsAbnormalityFile(entity).ToDataSourceResultAsync(request);
        }

        [HttpPost("delete-file")]
        public IActionResult DeleteFile([FromBody] Domain.AbnormalityFile entity)
        {
            abnormalityOverTimeService.DeleteFile(entity.Id);
            return NoContent();
        }

        [HttpPost("working-date")]
        public TimeManagementView IActionResult(TimeManagementView entiy)
        {
          return proxyTimeService.GetQuery()
                    .Where(x => x.WorkingDate == entiy.WorkingDate && x.NoReg == entiy.NoReg).FirstOrDefault();
        }
        protected  void ValidateOnPostCreate(bool isValidate)
        {
            Assert.ThrowIf(isValidate, "Cannot create, absence proxy time must be defined");
        }

        [HttpPost]
        [Route("calculate-overtime-duration")]
        public decimal CalculateOvertimeDuration()
        {
            DateTime ProxyIn;
            DateTime.TryParseExact(this.Request.Form["ProxyIn"], "dd/MM/yyyy HH:mm", new CultureInfo("en-US"), DateTimeStyles.None, out ProxyIn);
            DateTime ProxyOut;
            DateTime.TryParseExact(this.Request.Form["ProxyOut"], "dd/MM/yyyy HH:mm", new CultureInfo("en-US"), DateTimeStyles.None, out ProxyOut);
            DateTime OvertimeIn;
            DateTime.TryParseExact(this.Request.Form["OvertimeIn"], "dd/MM/yyyy HH:mm", new CultureInfo("en-US"), DateTimeStyles.None, out OvertimeIn);
            DateTime OvertimeOut;
            DateTime.TryParseExact(this.Request.Form["OvertimeOut"], "dd/MM/yyyy HH:mm", new CultureInfo("en-US"), DateTimeStyles.None, out OvertimeOut);

            int OvertimeBreak = int.Parse(this.Request.Form["OvertimeBreak"]);
            DateTime OvertimeDate = DateTime.ParseExact(this.Request.Form["OvertimeDate"], "dd/MM/yyyy", new CultureInfo("en-US"), DateTimeStyles.None);

            var normalSchedule = abnormalityOverTimeService.GetNormalWorkSchedule(ServiceProxy.UserClaim.NoReg, OvertimeDate);

            if (ProxyIn == DateTime.MinValue && ProxyOut == DateTime.MinValue)
            {
                return 0;
            }

            if (OvertimeIn == DateTime.MinValue && OvertimeOut == DateTime.MinValue)
            {
                return 0;
            }

            //DateTime CalculateIn = ProxyIn;
            //
            //if(OvertimeIn < ProxyIn)
            //{
            //    CalculateIn = OvertimeIn;
            //}
            //
            //DateTime CalculateOut = ProxyOut;
            //
            //if (OvertimeOut > ProxyOut)
            //{
            //    CalculateOut = OvertimeOut;
            //}

            return ServiceHelper.CalculateProxyDuration(OvertimeIn, OvertimeOut, OvertimeIn, OvertimeOut, (DateTime?) normalSchedule.NormalTimeIn, (DateTime?) normalSchedule.NormalTimeOut, OvertimeBreak);
        }

        [HttpPost("approve")]
        public async Task<IActionResult> Approve([FromBody] DocumentRequestDetailViewModel<AbnormalityOverTimeViewModel> documentRequestDetail)
        {
            //update
            var username = ServiceProxy.UserClaim.Username;
            var noreg = ServiceProxy.UserClaim.NoReg;
            var postCode = ServiceProxy.UserClaim.PostCode;
            var docApp = ApprovalService.GetDocumentDetailApprovalById(documentRequestDetail.DocumentApprovalId);

            //ClaimBenefitService.InsertAllowanceSeq36(docApp.CreatedBy, documentRequestDetail.Object);
            documentRequestDetail.ObjectValue = JsonConvert.SerializeObject(documentRequestDetail.Object);

            documentRequestDetail.Id = docApp.Id;

            ApprovalService.DocumentUpdated += ApprovalService_DocumentUpdated;
            ApprovalService.UpdateDocumentRequestDetail(documentRequestDetail);

            var actualOrganizationStructure = MdmService.GetActualOrganizationStructure(noreg, postCode);

            List<User> userList = userService.GetUsersByRole("HR_PROXY_ABNORMALITY").ToList();

            if (userList.Where(x => x.NoReg == noreg).Count() > 0)
            {
                await ApprovalService.PostAsync(username, actualOrganizationStructure, "complete", documentRequestDetail.DocumentApprovalId);
            }
            else
            {
                await ApprovalService.PostAsync(username, actualOrganizationStructure, "approve", documentRequestDetail.DocumentApprovalId);
            }

            return NoContent();
        }

        [HttpPost("get-date-create-ot")]
        public string GetUnavailableDate()
        {
            var configStart = configService.GetConfig("Abnormality.StartDate");
            var configEnd = configService.GetConfig("Abnormality.EndDate");
            var noReg = ServiceProxy.UserClaim.NoReg;

            DateTime dtStart = Convert.ToDateTime(configStart.ConfigValue);
            DateTime dtEnd = Convert.ToDateTime(configEnd.ConfigValue);

            var minHourOT = abnormalityOverTimeService.GetDataConfig("Abnormality.OT.Hours");

            List<DateInformation> dateList = new List<DateInformation>();

            List<DateTime> availableDate = new List<DateTime>();

            DataSourceResult availableDateDs = ServiceProxy.GetTableValuedDataSourceResult<AbnormalityOverTimeAvailableDateStoredEntity>(new DataSourceRequest(), new { noReg, dtStart, dtEnd });

            foreach (AbnormalityOverTimeAvailableDateStoredEntity v in availableDateDs.Data)
            {
                decimal JobHour = ServiceHelper.CalculateProxyDuration(v.WorkingTimeIn, v.WorkingTimeOut, v.WorkingTimeIn, v.WorkingTimeOut, v.NormalTimeIn, v.NormalTimeOut, 0);
                
                if (JobHour >= Convert.ToDecimal(minHourOT.ConfigValue))
                {
                    DateInformation temp = new DateInformation();
                    temp.Available = true;
                    temp.OvertimeDate = v.WorkingDate;
                    temp.ProxyIn = v.WorkingTimeIn;
                    temp.ProxyOut = v.WorkingTimeOut;

                    dateList.Add(temp);
                    availableDate.Add(v.WorkingDate);
                }
            }

            foreach(DateTime day in ServiceHelper.EachDay(dtStart, dtEnd))
            {
                if (!availableDate.Contains(day))
                {
                    DateInformation temp = new DateInformation();
                    temp.Available = false;
                    temp.OvertimeDate = day;

                    dateList.Add(temp);
                }
            }

            return JsonConvert.SerializeObject(dateList);
        }

        [HttpPost("get-master-data-date-noreg")]
        public string GetMasterDataDate()
        {
            var noReg = this.Request.Form["NoReg"].ToString();

            List<DateTime> unavailableDate = spklMasterDataService.GetQuery().Where(x => x.NoReg == noReg).Select(x => x.OvertimeDate).OrderBy(x => x.Date).ToList();

            return JsonConvert.SerializeObject(unavailableDate);
        }
        #endregion
    }


    #region MVC Controller
    /// <summary>
    /// Bank page controller
    /// </summary>
    [Area("TimeManagement")]
    public class AbnormalityOtController : MvcControllerBase
    {
        protected AbnormalityOverTimeService abnormalityOverTimeService => ServiceProxy.GetService<AbnormalityOverTimeService>();
        protected ProxyTimeService proxyTimeService => ServiceProxy.GetService<ProxyTimeService>();

        [Permission(PermissionKey.ViewAbnormalityOT)]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Load time management form by id
        /// </summary>
        /// <param name="id">Time Management Id</param>
        /// <returns>Time Management Form</returns>
        [HttpPost]
        public IActionResult Load(Guid id, DateTime? workingDate, int progress)
        {
            ViewBag.request = progress == 0;
            var faskes = abnormalityOverTimeService.Get(id);
            if (workingDate.HasValue)
            {
                var haveProxt = proxyTimeService.GetQuery().Where(x => x.WorkingDate == workingDate.Value && x.NoReg == ServiceProxy.UserClaim.NoReg).FirstOrDefault();
                if (haveProxt != null)
                {
                    faskes.ProxyIn = haveProxt.WorkingTimeIn.Value;
                    faskes.ProxyOut = haveProxt.WorkingTimeOut.Value;
                }
            }
           
            return PartialView("_AbnormalityOverTimeForm", faskes);
        }

        /// <summary>
        /// Load time management form by id
        /// </summary>
        /// <param name="id">Time Management Id</param>
        /// <returns>Time Management Form</returns>
        [HttpPost]
        public IActionResult LoadFile(Guid id, int progress)
        {
            ViewBag.request = progress == 0;
            var faskes = abnormalityOverTimeService.Get(id);
            return PartialView("_AbnormalityOverTimeFile", faskes);
        }


    }
    #endregion
}