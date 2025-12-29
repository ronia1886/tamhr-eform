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
    [Route("api/abnormality-bdjk")]
    [Permission(PermissionKey.ManageAbnormalityBDJK)]
    public class AbnormalityBDJKApiController : FormApiControllerBase<AbnormalityBdjkViewModel>
    {
        #region Domain Services
        //protected FormService FormService => ServiceProxy.GetService<FormService>();
        protected AbnormalityOverTimeService abnormalityOverTimeService => ServiceProxy.GetService<AbnormalityOverTimeService>();

        protected AbnormalityBdjkService abnormalityBdjkService => ServiceProxy.GetService<AbnormalityBdjkService>();

        protected ConfigService configService => ServiceProxy.GetService<ConfigService>();

        protected ProxyTimeService proxyTimeService => ServiceProxy.GetService<ProxyTimeService>();

        protected BdjkService bdjkService => ServiceProxy.GetService<BdjkService>();

        protected UserService userService => ServiceProxy.GetService<UserService>();

        #endregion

        [HttpPost("gets")]
        public DataSourceResult GetFromPosts([DataSourceRequest] DataSourceRequest request)
        {
            var noreg = ServiceProxy.UserClaim.NoReg;
            var postCode = ServiceProxy.UserClaim.PostCode;
            var username = ServiceProxy.UserClaim.Username;
            var organizationStructure = MdmService.GetActualOrganizationStructure(noreg, postCode);
            var orgCode = organizationStructure?.OrgCode;
            var orgLevel = organizationStructure?.OrgLevel;

            if (!ServiceProxy.UserClaim.Chief)
            {
                if (request.Filters == null)
                {
                    request.Filters = new List<IFilterDescriptor>();
                }

                request.Filters.Add(new FilterDescriptor("NoReg", FilterOperator.IsEqualTo, noreg));
            }
            var data = abnormalityBdjkService.GetBdjkRequestDetailsByUser(noreg
                //, username, orgCode, orgLevel
                ).Where(x => x.DocumentStatusCode.ToLower() == "completed").ToDataSourceResult(request);
            return data;
     
        }

        [HttpPost("gets-master")]
        public DataSourceResult GetMasterBdjk([DataSourceRequest] DataSourceRequest request)
        {
           return abnormalityBdjkService.GetsMasterDataBdjkView().ToDataSourceResult(new DataSourceRequest());
        }

        [HttpPost("gets-general-category")]
        public DataSourceResult GetGeneralCategory(string category)
        {
            return configService.GetGeneralCategories(category).ToDataSourceResult(new DataSourceRequest());
        }


        [HttpGet("get-employee")]
        public IActionResult GetEmployee(string text)
        {
            string noreg = ServiceProxy.UserClaim.NoReg;
            if (text == null)
            {
                text = "";
            }
            var data = configService.GetGeneralCategories("BdjkCode").ToList();
            return new JsonResult(data);
        }

        [HttpPost("request")]
        public IActionResult Createdata([FromBody] Domain.AbnormalityBdjk entity)
        {
            var ceWorkingDatetime = entity.WorkingDate.Date; // handler if enable defaullt value 1/1/0001
            entity.WorkingDate = ceWorkingDatetime;
            var wk = entity.WorkingDate.ToShortDateString();
            var clkIn = entity.WorkingTimeIn.Value.ToString("hh:mm tt");
            DateTime wrkIn = DateTime.Parse(wk + " " + clkIn);
            entity.WorkingTimeIn = wrkIn;
            var clkout = entity.WorkingTimeOut.Value.ToString("hh:mm tt");
            DateTime wrkOut = DateTime.Parse(wk + " " + clkout);
            entity.WorkingTimeOut = wrkOut;
            entity.RowStatus = true;
            entity.Status = "New Request";
            //TimeSpan duration = entity.WorkingTimeOut - entity.WorkingTimeIn;
            //entity.BdjkDuration = Convert.ToDecimal(duration.TotalHours);

            var dataReady = abnormalityBdjkService.GetQuery().Where(x => x.WorkingDate == entity.WorkingDate).Count();
            if (dataReady > 0 && entity.Id == Guid.Empty)
            {
                Assert.ThrowIf(true, "Working date is already request BDJK");
            }

            var haveProxt = proxyTimeService.GetQuery()
                 .Where(x => x.WorkingDate == ceWorkingDatetime && x.NoReg == entity.NoReg).Count();
            if (haveProxt < 1)
            {
                ValidateOnPostCreate(true);
            }

            var ls = entity.BDJKCode.Split(" ");
            var bdjkCode = "";
            foreach (var item in ls)
            {
                if (item == "D")
                {
                    entity.UangMakanDinas = true;
                }
                else if (item == "A" && entity.BDJKDuration.GetValueOrDefault() < 2)
                {
                    Assert.ThrowIf(true, "Cannot create BDJK Request, please check BDJK Code value (A) duration must more than 2 hours");
                }
                else if (item == "T")
                {
                    if (entity.Level > 6 && entity.WorkingTimeOut.Value.Hour > 21)
                    {
                        entity.Taxi = true;
                    }
                    else
                    {
                        Assert.ThrowIf(true, "Cannot create BDJK Request, please check BDJK Code value Taxi money after 21.00, for Class 7 & 8spv");
                    }
                }
                else
                {
                    if (bdjkCode == "")
                    {
                        bdjkCode = bdjkCode + item;
                    }
                    else
                    {
                        bdjkCode = bdjkCode + " " + item;
                    }
                }
            }
            entity.BDJKCode = bdjkCode;
            abnormalityBdjkService.Upsert(ServiceProxy.UserClaim.NoReg, entity);
            return NoContent();
        }

        [HttpPost("request-master-data")]
        public IActionResult CreateMasterData([FromBody] Domain.AbnormalityBdjk entity)
        {
            var isAlready = abnormalityBdjkService.GetQuery().Where(x => x.WorkingDate == entity.WorkingDate && x.NoReg == entity.NoReg).Count();
            if (isAlready > 0)
            {
                Assert.ThrowIf(true, "Data is already exist for this user, please chek BDJK Request data");
            }

            var ceWorkingDatetime = entity.WorkingDate.Date; // handler if enable defaullt value 1/1/0001
            entity.WorkingDate = ceWorkingDatetime;
            var wk = entity.WorkingDate.ToShortDateString();
            var clkIn = entity.WorkingTimeIn.Value.ToString("hh:mm tt");
            DateTime wrkIn = DateTime.Parse(wk + " " + clkIn);
            entity.WorkingTimeIn = wrkIn;
            var clkout = entity.WorkingTimeOut.Value.ToString("hh:mm tt");
            DateTime wrkOut = DateTime.Parse(wk + " " + clkout);
            entity.WorkingTimeOut = wrkOut;
            entity.RowStatus = true;
            entity.Status = "Completed";
            var dayOfWeek = entity.WorkingDate.DayOfWeek;
            var NormalTimeOut = abnormalityBdjkService.GetBySfitCode("1NS1");
            var isHoliday = abnormalityBdjkService.isHoliday(entity.WorkingDate);
            var isWeekEnd = (dayOfWeek.ToString().ToLower() == "sunday" || dayOfWeek.ToString().ToLower() == "saturday");
            var normalHour = NormalTimeOut.NormalTimeOut.Value.TotalHours;
            var JobHour = entity.WorkingTimeOut.Value.TimeOfDay.TotalHours - normalHour;
            entity.BDJKDuration = Convert.ToDecimal(JobHour);

            var ls = entity.BDJKCode.Split(" ");
            var bdjkCode = "";
            foreach (var item in ls)
            {
                //A Hari Kerja Biasa Minimal 2 Jam
                //B Hari Libur Minimal 2 Jam > 5 Jam
                //C Hari Libur diatas 5 Jam
                //D Uang Makan Dinas Luar
                //T Uang Taksi diatas jam 21.00, untuk Kls 7 & 8spv

                if (item == "A")
                {
                    if (isHoliday && isWeekEnd)
                    {
                        Assert.ThrowIf(true, "Cannot create BDJK Request, please check BDJK Code value (A) Working Date in holiday, public leave or week end");
                    }
                    else if (JobHour < 2)
                    {
                        Assert.ThrowIf(true, "Cannot create BDJK Request, please check BDJK Code value (A) duration must more than 2 hours");
                        entity.BDJKDuration = Convert.ToDecimal(JobHour);
                    }
                    else
                    {
                        if (bdjkCode == "")
                        {
                            bdjkCode = bdjkCode + item;
                        }
                        else
                        {
                            bdjkCode = bdjkCode + " " + item;
                        }
                    }
                   
                }
                if (item == "B")
                {
                    if (!isHoliday || !isWeekEnd)
                    {
                        Assert.ThrowIf(true, "Cannot create BDJK Request, please check BDJK Code value (B) Working Date not in holiday, public leave or week end");
                    }
                    else if (JobHour < 2)
                    {
                        Assert.ThrowIf(true, "Cannot create BDJK Request, please check BDJK Code value (B) duration must more than 2 hours");
                        entity.BDJKDuration = Convert.ToDecimal(JobHour);
                    }
                    else
                    {
                        if (bdjkCode == "")
                        {
                            bdjkCode = bdjkCode + item;
                        }
                        else
                        {
                            bdjkCode = bdjkCode + " " + item;
                        }
                    }

                }
                if (item == "C")
                {
                    if (!isHoliday || !isWeekEnd)
                    {
                        Assert.ThrowIf(true, "Cannot create BDJK Request, please check BDJK Code value (C) Working Date not in holiday, public leave or week end");
                    }
                    else if (JobHour < 5)
                    {
                        Assert.ThrowIf(true, "Cannot create BDJK Request, please check BDJK Code value (B) duration must more than 5 hours");
                        entity.BDJKDuration = Convert.ToDecimal(JobHour);
                    }
                    else
                    {
                        if (bdjkCode == "")
                        {
                            bdjkCode = bdjkCode + item;
                        }
                        else
                        {
                            bdjkCode = bdjkCode + " " + item;
                        }
                    }

                }
                else if (item == "D")
                {
                    entity.UangMakanDinas = true;
                }
                else if (item == "T")
                {
                    if (entity.WorkingTimeOut.Value.TimeOfDay.TotalHours > 21)
                    {
                        entity.Taxi = true;
                    }
                    else
                    {
                        Assert.ThrowIf(true, "Cannot create BDJK Request, please check BDJK Code value (T) Taxi money after 21.00, for Class 7 up");
                    }
                }
                
            }
            entity.BDJKCode = bdjkCode;
            abnormalityBdjkService.Upsert(ServiceProxy.UserClaim.NoReg, entity);
            return NoContent();
        }

        protected void ValidateOnPostCreate(bool isValidate)
        {
            Assert.ThrowIf(isValidate, "Cannot create BDJK Request, proxy time must be defined");
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

        [HttpPost("soft-delete")]
        public IActionResult SoftDelete([FromBody] Domain.AbnormalityFile entity)
        {
            abnormalityBdjkService.SoftDelete(entity.Id);
            return NoContent();
        }

        [HttpPost("all-employee")]
        public DataSourceResult GetAllEmployee([DataSourceRequest] DataSourceRequest request)
        {
            return ServiceProxy.GetTableValuedDataSourceResult<EmployeeOrganizationLevelStoredEntity>(request, new { min = "7", max = "99", keyDate = DateTime.Now.Date });
        }


        [HttpPost("approve")]
        public async Task<IActionResult> Approve([FromBody] DocumentRequestDetailViewModel<AbnormalityBdjkViewModel> documentRequestDetail)
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

        [HttpPost]
        [Route("update-detail")]
        public IActionResult UpdateDetail(BdjkRequestUpdateViewModel viewModel)
        {
            bdjkService.UpdateBdjkDetail(viewModel);

            return NoContent();
        }

        [HttpPost("get-unavailable-date-noreg")]
        public string GetUnavailableDate()
        {
            var noReg = this.Request.Form["NoReg"].ToString();

            List<DateTime> unavailableDate = abnormalityBdjkService.GetQueryView().Where(x => x.NoReg == noReg && (x.AbnormalityBdjkId != null && x.Status != "Locked")).Select(x => x.WorkingDate).ToList();

            return JsonConvert.SerializeObject(unavailableDate);
        }

        [HttpPost("get-master-data-date-noreg")]
        public string GetMasterDataDate()
        {
            var noReg = this.Request.Form["NoReg"].ToString();

            List<DateTime> unavailableDate = abnormalityBdjkService.GetQuery().Where(x => x.NoReg == noReg).Select(x => x.WorkingDate).ToList();

            return JsonConvert.SerializeObject(unavailableDate);
        }
    }


    #region MVC Controller
    /// <summary>
    /// Bank page controller
    /// </summary>
    [Area("TimeManagement")]
    public class AbnormalityBDJKController : MvcControllerBase
    {
        protected AbnormalityBdjkService abnormalityBdjkService { get { return ServiceProxy.GetService<AbnormalityBdjkService>(); } }
        protected ProxyTimeService proxyTimeService => ServiceProxy.GetService<ProxyTimeService>();
        protected BdjkService bdjkService => ServiceProxy.GetService<BdjkService>();

        [Permission(PermissionKey.ViewAbnormalityBDJK)]
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
        public IActionResult Load(Guid id, Guid abnormalityBdjkId, DateTime? workingDate, int progress)
        {
            ViewBag.request = progress == 0;
            AbnormalityBdjk bdjk = new AbnormalityBdjk();
            bdjk.Id = id;
            bdjk.TimeManagementBdjkId = abnormalityBdjkId == Guid.Empty ? id : Guid.Empty;
            return PartialView("_AbnormalityBdjkForm", bdjk);
        }

        /// <summary>
        /// Load time management form by id
        /// </summary>
        /// <param name="id">Time Management Id</param>
        /// <returns>Time Management Form</returns>
        [HttpPost]
        public IActionResult LoadFile(Guid id, Guid abnormalityBdjkId, int progress)
        {
            ViewBag.request = progress == 0;
            AbnormalityBdjk bdjk = new AbnormalityBdjk();
            bdjk.Id = id;
            bdjk.TimeManagementBdjkId = abnormalityBdjkId == Guid.Empty ? id : Guid.Empty;
            return PartialView("_AbnormalityBdjkFile", bdjk);
        }

        /// <summary>
        /// Load time management form by id
        /// </summary>
        /// <param name="id">Time Management Id</param>
        /// <returns>Time Management Form</returns>
        [HttpPost]
        public IActionResult LoadMasterBdjk()
        {
            AbnormalityBdjk bdjk = new AbnormalityBdjk();
           // bdjk = abnormalityBdjkService.Get(id);
            return PartialView("_MasterDataBdjkForm", bdjk);
        }

        [HttpPost]
        public IActionResult LoadMasterBdjkEdit(Guid id)
        {
            MasterDataBdjkView bdjk = new MasterDataBdjkView();
            bdjk = abnormalityBdjkService.GetsMasterDataBdjkView().Where(x => x.Id == id).FirstOrDefault();
            bdjk.IsEdit = false;
            return PartialView("_MasterDataBdjkFormEdit", bdjk);
        }

        [HttpPost]
        public IActionResult LoadAbnormalityBdjkEdit(Guid id)
        {
            MasterDataBdjkView bdjk = new MasterDataBdjkView();

            AbnormalityBdjkView bdjkAb = abnormalityBdjkService.GetAbnormalityBdjkById(id);

            bdjk.Id = id;
            bdjk.AbnormalityBdjkId = bdjkAb.Id;
            bdjk.DocumentApprovalId = bdjkAb.DocumentApprovalId;
            bdjk.DocumentNumber = bdjkAb.DocumentNumber;
            bdjk.DocumentStatusCode = bdjkAb.DocumentStatusCode;
            bdjk.DocumentStatusName = bdjkAb.DocumentStatusName;
            bdjk.EmployeeSubgroup = bdjkAb.EmployeeSubgroup;
            bdjk.BdjkCode = bdjkAb.BdjkCode;
            bdjk.NoReg = bdjkAb.NoReg;
            bdjk.WorkingDate = bdjkAb.WorkingDate;
            bdjk.WorkingTimeIn = bdjkAb.WorkingTimeIn;
            bdjk.WorkingTimeOut = bdjkAb.WorkingTimeOut;
            bdjk.ActivityCode = bdjkAb.ActivityCode;
            bdjk.UangMakanDinas = bdjkAb.UangMakanDinas;
            bdjk.Taxi = bdjkAb.Taxi;
            bdjk.BdjkReason = bdjkAb.BdjkReason;
            bdjk.Duration = bdjkAb.Duration;
            bdjk.ParentId = bdjkAb.ParentId;
            bdjk.IsEdit = true;
            

            return PartialView("_MasterDataBdjkFormEdit", bdjk);
        }
    }
    #endregion
}