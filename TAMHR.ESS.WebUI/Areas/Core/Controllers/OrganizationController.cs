using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.ViewModels;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using Newtonsoft.Json;

namespace TAMHR.ESS.WebUI.Areas.Core.Controllers
{
    #region API Controller
    [Route("api/organization")]
    //[Permission(PermissionKey.ViewOrganization)]
    [ApiController]
    public class OrganizationApiController : ApiControllerBase
    {
        #region Domain Services
        /// <summary>
        /// MDM service
        /// </summary>
        protected MdmService MdmService => ServiceProxy.GetService<MdmService>();

        /// <summary>
        /// Claim Benefit service
        /// </summary>
        protected ClaimBenefitQueryService ClaimBenefitService => ServiceProxy.GetService<ClaimBenefitQueryService>();

        protected ApprovalService approvalService => ServiceProxy.GetService<ApprovalService>();

        #endregion
        [HttpPost("get-structures")]
        public async Task<TreeDataSourceResult> GetStructures([DataSourceRequest] DataSourceRequest request)
        {
            var structures = await MdmService.GetOrganizationStructuresAsync();

            return structures.Select(x => new { x.OrgCode, x.ParentOrgCode, x.ObjectText, x.ObjectDescription }).ToTreeDataSourceResult(request, e => e.OrgCode, e => e.ParentOrgCode, e => e);
        }

        [HttpPost("get-employee-structures")]
        public DataSourceResult GetEmployeeStructures([FromForm] string orgCode, [DataSourceRequest] DataSourceRequest request)
        {
            var keyDate = DateTime.Now.Date;

            return ServiceProxy.GetTableValuedDataSourceResult<EmployeeOrganizationStoredEntity>(request, new { orgCode, keyDate });
        }

        [HttpPost("all-employee")]
        public DataSourceResult GetAllEmployee([DataSourceRequest] DataSourceRequest request)
        {
            return ServiceProxy.GetTableValuedDataSourceResult<SubordinateStroredEntity>(request, new { orgCode = "*", orgLevel = 0 });
        }

        [HttpPost("subordinates")]
        public DataSourceResult GetSubordinates([DataSourceRequest] DataSourceRequest request)
        {
            var noreg = ServiceProxy.UserClaim.NoReg;
            var postCode = ServiceProxy.UserClaim.PostCode;
            var org = MdmService.GetActualOrganizationStructure(noreg, postCode);
            var orgCode = org.OrgCode;
            var orgLevel = org.OrgLevel;

            if (AclHelper.HasPermission(PermissionKey.ViewAllEmployeeProfile))
            {
                orgCode = "*";
                orgLevel = 0;
            }

            return ServiceProxy.GetTableValuedDataSourceResult<SubordinateStroredEntity>(request, new { orgCode, orgLevel });
        }

        [HttpGet("download-subordinates")]
        public IActionResult DownloadSubordinates()
        {
            var noreg = ServiceProxy.UserClaim.NoReg;
            var postCode = ServiceProxy.UserClaim.PostCode;
            var org = MdmService.GetActualOrganizationStructure(noreg, postCode);
            var orgCode = org.OrgCode;
            var orgLevel = org.OrgLevel;

            if (AclHelper.HasPermission(PermissionKey.ViewAllEmployeeProfile))
            {
                orgCode = "*";
                orgLevel = 0;
            }

            var data = MdmService.GetSubordinateFamilies(orgCode, orgLevel).OrderBy(x => x.NoReg).ThenBy(x => x.BirthDate);

            return ExportToXlsx(data, string.Format("Subordinate Family {0:ddMMyyyyHHmm}.xlsx", DateTime.Now), excludes: new[] { "Id", "OrgName" });
        }

        [HttpGet("departments")]
        public IActionResult Departments()
        {
            var postcode = ServiceProxy.UserClaim.PostCode;
            var objs = MdmService.GetOrganizationObjects("O", "Department", ServiceProxy.UserClaim.NoReg, postcode);
            return Ok(objs);
        }

        [HttpGet("divisions")]
        public IActionResult Divisions()
        {
            var objs = MdmService.GetEmployeeOrganizationObjects(ServiceProxy.UserClaim.NoReg, ServiceProxy.UserClaim.PostCode).Where(x => x.ObjectDescription == "Division");
            return Ok(objs);
        }

        [HttpGet("departments-noreg")]
        public IActionResult DepartmentsNoreg(string noreg)
        {
            var postcode = MdmService.GetActualOrganizationStructure(noreg).PostCode;
            var objs = MdmService.GetOrganizationObjects("O", "Department", noreg, postcode);
            return Ok(objs);
        }

        [HttpGet("departments-noreg-vacation")]
        public IActionResult DepartmentsNoregVacation(string noreg, DateTime vacationDate,bool isActual)
        {
            var postcode = ServiceProxy.UserClaim.PostCode;
            //var postcode = MdmService.GetActualOrganizationStructure(noreg).PostCode;
            var objs = MdmService.GetOrganizationObjects("O", "Department", noreg, postcode);
            
            if (!isActual)
            {
                var objDocOnprogress = approvalService.GetInprogressRequestDetails("vacation-allowance");
                var ObjVacationOnprogress = objDocOnprogress.Select(x => JsonConvert.DeserializeObject<VacationAllowanceViewModel>(x.ObjectValue)).Where(v => v.VacationDate.Value.Date == vacationDate.Date).ToList();
                if (ObjVacationOnprogress != null)
                {
                    var VacationDeptOnprogress = ObjVacationOnprogress.Select(v => v.Departments.Select(d => d.ObjectID).ToList()).ToList();
                    List<string> listDept = new List<string>();
                    foreach (var item in VacationDeptOnprogress)
                    {
                        listDept.AddRange(item);
                    }

                    objs = objs.Where(o => !listDept.Contains(o.ObjectID));
                }
            }
            
            var objDocComplete = approvalService.GetCompleteRequestDetails("vacation-allowance");
            var ObjVacationComplete = objDocComplete.Select(x => JsonConvert.DeserializeObject<VacationAllowanceViewModel>(x.ObjectValue)).Where(v => v.VacationDate.Value.Date == vacationDate.Date).ToList();
            if (ObjVacationComplete != null)
            {
                var VacationDeptComplete = ObjVacationComplete.Select(v => v.Departments.Select(d => d.ObjectID).ToList()).ToList();
                List<string> listDeptComplete = new List<string>();
                foreach (var item in VacationDeptComplete)
                {
                    listDeptComplete.AddRange(item);
                }

                objs = objs.Where(o => !listDeptComplete.Contains(o.ObjectID));
            }

            return Ok(objs);
        }

        [HttpGet("departments-noreg-vacation-selected")]
        public IActionResult DepartmentsNoregVacationSelected(Guid docId, string noreg)
        {
            var postcode = ServiceProxy.UserClaim.PostCode;
            //var postcode = MdmService.GetActualOrganizationStructure(noreg).PostCode;
            var objs = MdmService.GetOrganizationObjects("O", "Department", noreg, postcode);
            var xxx = approvalService.GetDocumentRequestDetailByApprovalId(docId);
            var objVacation = JsonConvert.DeserializeObject<VacationAllowanceViewModel>(approvalService.GetDocumentRequestDetailByApprovalId(docId).ObjectValue);
            var listDeptVacation = objVacation.Departments.Select(d => d.ObjectID).ToList();
            objs = objs.Where(o => listDeptVacation.Contains(o.ObjectID));

            return Ok(objs);
        }

        [HttpGet("divisions-noreg")]
        public IActionResult DivisionsNoreg(string noreg)
        {
            var postCode = ServiceProxy.UserClaim.PostCode;
            var objs = MdmService.GetEmployeeOrganizationObjects(noreg,postCode).Where(x => x.ObjectDescription == "Division" /*&& Convert.ToInt32(x.Staffing) == 100*/);
            return Ok(objs);
        }

        [HttpGet("divisions-noreg-vacation")]
        public IActionResult DivisionsNoregVacation(string noreg, DateTime vacationDate)
        {
            var postCode = ServiceProxy.UserClaim.PostCode;
            var objs = MdmService.GetEmployeeOrganizationObjects(noreg,postCode).Where(x => x.ObjectDescription == "Division" /*&& Convert.ToInt32(x.Staffing) == 100*/);

            var objDocOnprogress = approvalService.GetInprogressRequestDetails("vacation-allowance");
            var ObjVacationOnprogress = objDocOnprogress.Select(x => JsonConvert.DeserializeObject<VacationAllowanceViewModel>(x.ObjectValue)).Where(v => v.VacationDate.Value.Date == vacationDate.Date).ToList();
            if (ObjVacationOnprogress != null)
            {
                var VacationDeptOnprogress = ObjVacationOnprogress.Select(v => v.Departments.Select(d => d.ObjectID).ToList()).ToList();
                List<string> listDept = new List<string>();
                foreach (var item in VacationDeptOnprogress)
                {
                    listDept.AddRange(item);
                }
                objs = objs.Where(o => !listDept.Contains(o.ObjectID));
            }

            var objDocComplete = approvalService.GetCompleteRequestDetails("vacation-allowance");
            var ObjVacationComplete = objDocComplete.Select(x => JsonConvert.DeserializeObject<VacationAllowanceViewModel>(x.ObjectValue)).Where(v => v.VacationDate.Value.Date == vacationDate.Date).ToList();
            if (ObjVacationComplete != null)
            {
                var VacationDeptComplete = ObjVacationComplete.Select(v => v.Departments.Select(d => d.ObjectID).ToList()).ToList();
                List<string> listDeptComplete = new List<string>();
                foreach (var item in VacationDeptComplete)
                {
                    listDeptComplete.AddRange(item);
                }

                objs = objs.Where(o => !listDeptComplete.Contains(o.ObjectID));
            }

            return Ok(objs);
        }

        [HttpGet("divisions-noreg-vacation-selected")]
        public IActionResult DivisionsNoregVacationSelected(Guid docId, string noreg)
        {
            var postCode = ServiceProxy.UserClaim.PostCode;
            var objs = MdmService.GetEmployeeOrganizationObjects(noreg,postCode).Where(x => x.ObjectDescription == "Division" /*&& Convert.ToInt32(x.Staffing) == 100*/);
            var xxx = approvalService.GetDocumentRequestDetailByApprovalId(docId);
            var objVacation = JsonConvert.DeserializeObject<VacationAllowanceViewModel>(approvalService.GetDocumentRequestDetailByApprovalId(docId).ObjectValue);
            var listDeptVacation = objVacation.Departments.Select(d => d.ObjectID).ToList();
            objs = objs.Where(o => listDeptVacation.Contains(o.ObjectID));

            return Ok(objs);
        }

        [HttpGet("departments-with-employees")]
        public IActionResult DepartmentsEmployees()
        {
            var postcode = ServiceProxy.UserClaim.PostCode;
            var objs = MdmService.GetOrganizationObjects("O", "Department", ServiceProxy.UserClaim.NoReg, postcode);
            var result = new
            {
                Departments = new List<Object>()
            };

            foreach (var item in objs)
            {
                result.Departments.Add(new
                {
                    Code = item.ObjectID,
                    ObjectID = item.ObjectID,
                    ObjectText = item.ObjectText,
                    ObjectDescription = item.ObjectDescription,
                    Employees = MdmService.GetEmployeeByDepartment(item.ObjectID)
                });
            }

            return Ok(result);
        }

        [HttpGet("employee-by-dept")]
        public IActionResult Employees(string orgCode)
        {
            var objs = MdmService.GetEmployeeByDepartment(orgCode);
            return Ok(objs.Where(x => x.Classification < 9));
        }

        [HttpGet("employee-by-dept-all")]
        public IActionResult EmployeesDeptAll(string orgCode, DateTime keyDate)
        {
            var objs = MdmService.GetEmployeeByDepartmentVacation(orgCode, keyDate);
            return Ok(objs);
        }

        [HttpGet("employee-by-div")]
        public IActionResult EmployeesByDiv(string orgCode, DateTime keyDate)
        {
            var objs = MdmService.GetEmployeeByDivisionVacation(orgCode, keyDate);
            return Ok(objs);
        }

        [HttpPost("employee-by-dept-combo")]
        public IActionResult EmployeesCombo([FromQuery] string orgCode)
        {
            var objs = MdmService.GetEmployeeByDepartment(orgCode);
            return Ok(new { Data = objs, Total = objs.Count() });
        }

        [HttpPost("employee-by-dept-combo-shift")]
        public IActionResult EmployeesComboShift([FromQuery] string orgCode)
        {
            //var orgCodeUser = MdmService.GetActualOrganizationStructure(ServiceProxy.UserClaim.NoReg).OrgCode;
            //var objs = MdmService.GetEmployeeByDepartmentShift(orgCodeUser, ServiceProxy.UserClaim.NoReg);
            var objs = MdmService.GetEmployeeByDepartmentShift(orgCode, ServiceProxy.UserClaim.NoReg);
            return Ok(new { Data = objs, Total = objs.Count() });
        }

        [HttpGet("employee-shift-meal-by-dept")]
        public IActionResult EmployeesShiftData(string noreg, DateTime period)
        {
            var orgCodeUser = MdmService.GetActualOrganizationStructure(noreg).OrgCode;
            var objs = MdmService.GetEmployeeShiftMealAllowanceByDepartment(orgCodeUser, period);
            return Ok(objs);
        }

        [HttpGet("get-allowances")]
        public IActionResult GetAllowance(string type)
        {
            var data = ClaimBenefitService.GetAllowances(type).Select(x => new
            {
                ClassFrom = x.ClassFrom,
                ClassTo = x.ClassTo,
                Ammount = x.Ammount
            }).ToList();

            return Ok(data);
        }

        [HttpPost("employee-by-class")]
        public DataSourceResult GetEmployeeByClass(
            [FromForm] string orgCode,
            [FromForm] DateTime keyDate,
            [FromForm] int min,
            [FromForm] int max,
            [DataSourceRequest] DataSourceRequest request)
        {
            return ServiceProxy.GetTableValuedDataSourceResult<EmployeeClassStroredEntity>(request, new { orgCode, keyDate, min, max });
        }

        [HttpPost("get-departments")]
        public DataSourceResult GetDepartments([DataSourceRequest] DataSourceRequest request)
        {
            return MdmService.GetDepartments().ToDataSourceResult(request);
        }
    }
    #endregion

    #region MVC Controller
    [Area("Core")]
    [Permission(PermissionKey.ViewOrganization)]
    public class OrganizationController : MvcControllerBase
    {
        public MdmService MdmService => ServiceProxy.GetService<MdmService>();

        /// <summary>
        /// Organization structure 
        /// </summary>
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult GetEmployeeStructures(string id)
        {
            ViewBag.OrgCode = id;

            return PartialView("_EmployeeStructures");
        }
    }
    #endregion
}