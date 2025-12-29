using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.ViewModels;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.Web.Runtime.Providers;
using TAMHR.ESS.Infrastructure.Validators;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Localization;
using Agit.Domain.UnitOfWork;
using TAMHR.ESS.Infrastructure.Web.Extensions;

namespace TAMHR.ESS.WebUI.Areas.TimeManagement.Controllers
{
    
    #region API Controller
    [Route("api/weekly-wfh-planning")]
    public class WeeklyWFHPlanningAPIController : FormApiControllerBase<WeeklyWFHPlanningViewModel>
    {
        #region Domain Services
        public UserService UserService => ServiceProxy.GetService<UserService>();
        protected TimeManagementService TimeManagementService => ServiceProxy.GetService<TimeManagementService>();
        protected WeeklyWFHPlanningService WeeklyWFHPlanningService => ServiceProxy.GetService<WeeklyWFHPlanningService>();

        #endregion

        public class WeeklyWFHPlanningException : Exception
        {
            public WeeklyWFHPlanningException(string message) : base(message)
            {
            }
        }

        protected override void ValidateOnPostCreate(DocumentRequestDetailViewModel<WeeklyWFHPlanningViewModel> requestDetailViewModel)
        {
            

            base.ValidateOnPostCreate(requestDetailViewModel);

            if (requestDetailViewModel.Object == null || requestDetailViewModel.Object.WeeklyWFHPlanningDetails.Count == 0) throw new WeeklyWFHPlanningException("Cannot create request because request is empty");

            ////check Plan Month
            //var diffMonth = requestDetailViewModel.Object.WeeklyWFHPlanning.StartDate.Subtract(DateTime.Now).Days / (365.25 / 12);
            //if (diffMonth < -3)
            //{
            //    var MonthMin3 = DateTime.Now.AddMonths(-3);
            //    throw new Exception("Period cannot smaller than " + MonthMin3.Month + " " + MonthMin3.Year);
            //}

            ////check already submitted
            //var noreg = User.GetClaim("NoReg");
            //var postCode = User.GetClaim("PostCode");
            //var checkData = WeeklyWFHPlanningService.GetByKey(noreg, postCode, requestDetailViewModel.Object.WeeklyWFHPlanning.StartDate, requestDetailViewModel.Object.WeeklyWFHPlanning.EndDate);
            //if(checkData != null)
            //{
            //    throw new Exception("Data has been submitted");
            //}
            ////check min WFO
            //var data = requestDetailViewModel.Object.WeeklyWFHPlanningDetails;
            //if (data != null)
            //{
            //    var errMessage = WeeklyWFHPlanningService.WFHValidation(data);
            //    if (errMessage != "") throw new Exception(errMessage);
            //}

            ValidateOnPostCreateUpdate(requestDetailViewModel, "create");

        }

        protected override void ValidateOnPostUpdate(DocumentRequestDetailViewModel<WeeklyWFHPlanningViewModel> requestDetailViewModel)
        {
            base.ValidateOnPostUpdate(requestDetailViewModel);

            if (requestDetailViewModel.Object == null || requestDetailViewModel.Object.WeeklyWFHPlanningDetails.Count == 0) throw new WeeklyWFHPlanningException("Cannot create request because request is empty");

            ValidateOnPostCreateUpdate(requestDetailViewModel, "update");
        }

        protected override void ApprovalService_DocumentCreated(object sender, DocumentRequestDetailViewModel e)
        {
            //var username = ServiceProxy.UserClaim.Username;
            //var noreg = ServiceProxy.UserClaim.NoReg;
            //var postCode = ServiceProxy.UserClaim.PostCode;
            //var actualOrganizationStructure = MdmService.GetActualOrganizationStructure(noreg, postCode);
            //
            //ApprovalService.PostAsync(username, actualOrganizationStructure, "create", e.DocumentApprovalId);
            Upsert(e);
        }

        protected override void ApprovalService_DocumentUpdated(object sender, DocumentRequestDetailViewModel e)
        {
            //var username = ServiceProxy.UserClaim.Username;
            //var noreg = ServiceProxy.UserClaim.NoReg;
            //var postCode = ServiceProxy.UserClaim.PostCode;
            //var actualOrganizationStructure = MdmService.GetActualOrganizationStructure(noreg, postCode);
            //
            //ApprovalService.PostAsync(username, actualOrganizationStructure, "create", e.DocumentApprovalId);
            Upsert(e);
        }


        public void ValidateOnPostCreateUpdate(DocumentRequestDetailViewModel<WeeklyWFHPlanningViewModel> requestDetailViewModel, string method = "create")
        {
            if (requestDetailViewModel.Object == null || requestDetailViewModel.Object.WeeklyWFHPlanningDetails.Count == 0) throw new WeeklyWFHPlanningException("Cannot create request because request is empty");

            //check Plan Month
            var diffMonth = requestDetailViewModel.Object.WeeklyWFHPlanning.StartDate.Subtract(DateTime.Now).Days / (365.25 / 12);
            if (diffMonth < -3)
            {
                var MonthMin3 = DateTime.Now.AddMonths(-3);
                throw new WeeklyWFHPlanningException("Period cannot smaller than " + MonthMin3.Month + " " + MonthMin3.Year);
            }

            if(method == "create")
            {
                //check already submitted
                var noreg = User.GetClaim("NoReg");
                var postCode = User.GetClaim("PostCode");
                var checkData = WeeklyWFHPlanningService.GetByKey(noreg, postCode, requestDetailViewModel.Object.WeeklyWFHPlanning.StartDate, requestDetailViewModel.Object.WeeklyWFHPlanning.EndDate);
                if (checkData != null)
                {
                    throw new WeeklyWFHPlanningException("Data has been submitted");
                }
            }

            //check min WFO
            var data = requestDetailViewModel.Object.WeeklyWFHPlanningDetails;
            if (data != null)
            {
                var errMessage = WeeklyWFHPlanningService.WFHValidation(data);
                if (errMessage != "") throw new WeeklyWFHPlanningException(errMessage);
            }
        }

        [HttpPost("update-weekly-wfh-planning")]
        public IActionResult UpdateWeeklyWFHPlanning([FromBody] DocumentRequestDetailViewModel<WeeklyWFHPlanningViewModel> vm)
        { 
        //public IActionResult UpdateWeeklyWFHPlanning([FromBody] dynamic vmtest)
        //{
            //var vm = JsonConvert.DeserializeObject<DocumentRequestDetailViewModel<WeeklyWFHPlanningViewModel>>(vmtest.ToString().Trim().TrimStart('{').TrimEnd('}'));
            var noreg = ServiceProxy.UserClaim.NoReg;
            var postCode = ServiceProxy.UserClaim.PostCode;

            object objRet = new { };
            
            try
            {
                ValidateOnPostCreateUpdate(vm, "update");
                
                objRet = new
                {
                    status = true,
                    message = "Hybrid Work Schedule Plan Data submitted successfully"
                };

                string action = Request.Query["action"];
                
                
                var actualOrganizationStructure = MdmService.GetActualOrganizationStructure(noreg, postCode);
                WeeklyWFHPlanningService.UpsertWeeklyWFHPlanningRequest(vm.DocumentApprovalId, vm, action, actualOrganizationStructure);
                
                return new JsonResult(objRet);
            }
            catch (WeeklyWFHPlanningException ex)
            {
                //throw new Exception(ex.Message);// Menampilkan pesan error yang lebih informatif
                objRet = new
                {
                    status = false,
                    message = ex.Message
                };
                return new JsonResult(objRet);
            }
            catch (Exception e)
            {
                LogService.InsertLog("Data Changes","Update","WeeklyWFHPlanning","false",e.ToString(), noreg);

                //throw new Exception("Something went wrong when submitting changes.");
                objRet = new
                {
                    status = false,
                    message = "Something went wrong when submitting changes."
                };
                return new JsonResult(objRet);
            }
            
            //return CreatedAtAction("GetDocumentApproval", new { id = vm.DocumentApprovalId, formKey = vm.FormKey });
        }
        //public IActionResult UpdateWeeklyWFHPlanning([FromBody] DocumentRequestDetailViewModel<WeeklyWFHPlanningViewModel> vm)
        //{
        //    base.ValidateOnPostUpdate(vm);

        //    string action = Request.Query["action"];
        //    var noreg = ServiceProxy.UserClaim.NoReg;
        //    var postCode = ServiceProxy.UserClaim.PostCode;
        //    var actualOrganizationStructure = MdmService.GetActualOrganizationStructure(noreg, postCode);
        //    WeeklyWFHPlanningService.UpsertWeeklyWFHPlanningRequest(vm.DocumentApprovalId, vm, action,actualOrganizationStructure);
        //    return new JsonResult("Data updated successfully.");
        //}

        [HttpPost]
        public override IActionResult Create([FromBody] DocumentRequestDetailViewModel<WeeklyWFHPlanningViewModel> documentRequestDetail)
        {
            var noreg = ServiceProxy.UserClaim.NoReg;
            var postCode = ServiceProxy.UserClaim.PostCode;

            try
            {
                ApprovalService.DocumentCreated += ApprovalService_DocumentCreated;

                
                var formKey = documentRequestDetail.FormKey;

                documentRequestDetail = ValidateViewModel(documentRequestDetail);
                ValidateOnCreate(formKey);
                ValidateOnPostCreate(documentRequestDetail);

                ApprovalService.CreateApprovalDocumentByPass(noreg, documentRequestDetail, postCode, (title, dicts) => GenerateTitle(title, dicts, documentRequestDetail));

                LogAction($"Create Document ID <b>{documentRequestDetail.DocumentApprovalId}</b>, Form Key <b>{documentRequestDetail.FormKey}</b>");

                return CreatedAtAction("GetDocumentApproval", new { id = documentRequestDetail.DocumentApprovalId, formKey = documentRequestDetail.FormKey });
            }
            catch (WeeklyWFHPlanningException ex)
            {
                //Console.WriteLine(ex.Message); // Menampilkan pesan error yang lebih informatif
                throw new Exception(ex.Message);
            }
            catch (Exception e)
            {
                LogService.InsertLog("Data Changes", "Update", "WeeklyWFHPlanning", "false", e.ToString(), noreg);
                throw new Exception("Something went wrong when submitting changes.");
                //throw new Exception(e.Message);
            }
            

        }

        private void Upsert(DocumentRequestDetailViewModel e)
        {
            var documentRequestDetail = e as DocumentRequestDetailViewModel<WeeklyWFHPlanningViewModel>;
            
            WeeklyWFHPlanningService.UpsertWeeklyWFHPlanningRequest(documentRequestDetail.DocumentApprovalId, documentRequestDetail, "",null);
        }

        [HttpPost("get-details/{documentApprovalId}")]
        public IEnumerable<WeeklyWFHPlanningDetailView> GetDetails(Guid documentApprovalId)
        {
            var noreg = User.GetClaim("NoReg");
            return WeeklyWFHPlanningService.GetDetails(documentApprovalId, noreg);
        }
    }
    #endregion

    #region MVC Controller
    [Area(ApplicationModule.TimeManagement)]
    [Route("weekly-wfh-planning")]
    public class WeeklyWFHPlanningController : MvcControllerBase
    {
        #region Domain Service

        public ApprovalService ApprovalService => ServiceProxy.GetService<ApprovalService>();
        public WeeklyWFHPlanningService WeeklyWFHPlanningService => ServiceProxy.GetService<WeeklyWFHPlanningService>();

        #endregion
        [HttpGet("edit/{documentApprovalId}")]
        public IActionResult Edit(Guid documentApprovalId)
        {
            DocumentApproval documentApproval = ApprovalService.GetDocumentApprovalById(documentApprovalId);
            var model = new DocumentRequestDetailViewModel<WeeklyWFHPlanningViewModel>();
            if (documentApproval != null)
            {
                model = ApprovalService.GetDocumentRequestDetailViewModel<WeeklyWFHPlanningViewModel>(documentApprovalId, ServiceProxy.UserClaim.NoReg);
                model.FormKey = "weekly-wfh-planning";
            }
            else
            {
                return NotFound();
            }

            bool hasNewerVersion = WeeklyWFHPlanningService.HasNewerVersion(documentApprovalId);

            ViewData.Add("documentApprovalId", documentApproval == null ? null : documentApproval.Id.ToString());
            if (!hasNewerVersion && documentApproval != null)
            {
                ViewData.Add("mode", "edit");
            }

            return View("~/Areas/TimeManagement/Views/Form/WeeklyWFHPlanning.cshtml", model);
        }
        public IActionResult Index()
        {
            return View();
        }
    }

    #endregion
}
