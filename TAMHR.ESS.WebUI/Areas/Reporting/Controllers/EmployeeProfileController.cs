using Agit.Common;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUglify.Helpers;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.ViewModels;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using TAMHR.ESS.Infrastructure.Web.Configurations;
using TAMHR.ESS.Infrastructure.Web.Extensions;
using TAMHR.ESS.Infrastructure.Web.Runtime.Providers;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;

namespace TAMHR.ESS.WebUI.Areas.Reporting.Controllers
{
    #region API Controller
    /// <summary>
    /// Api controller for time evaluation
    /// </summary>
    [Route("api/employe-profile")]
   // [Permission(PermissionKey.ViewEmployeeProfile)]
    public class EmployeeProfileAPIController : ApiControllerBase
    {
        protected EmployeeProfileService employeeProfileService => ServiceProxy.GetService<EmployeeProfileService>();
        protected MdmService MdmService { get { return ServiceProxy.GetService<MdmService>(); } }
        protected PersonalDataService PersonalDataService => ServiceProxy.GetService<PersonalDataService>();

        [HttpPost("get-employee")]
        public async Task<DataSourceResult> GetEmployeeProfile([DataSourceRequest] DataSourceRequest request)
        {
            var result = await employeeProfileService.getEmployee().ToDataSourceResultAsync(request);

            return result;
        }

        [HttpPost("get-employee-actual")]
        public async Task<DataSourceResult> GetEmployeeProfileActual([DataSourceRequest] DataSourceRequest request)
        {
            var result = await employeeProfileService.getEmployeeActual().ToDataSourceResultAsync(request);

            return result;
        }

        [HttpPost("get-dashboard-unique-column-values")]
        public IActionResult GetUniqueColumnValuesDashboard(string field)
        {
            var uniqueValues = employeeProfileService.GetInvitationUniqueColumnValuesDashboard(field);
            return Ok(uniqueValues);
        }

        [HttpPost("get-unique-column-values-directorate")]
        public IActionResult GetUniqueColumnValuesDirectorate()
        {
            string field = "Directorate";
            var uniqueValues = employeeProfileService.GetInvitationUniqueColumnValuesDashboard(field);
            return Ok(uniqueValues);
        }

        [HttpPost("get-unique-column-values-division")]
        public IActionResult GetUniqueColumnValuesDivision()
        {
            string field = "Divisi";
            var uniqueValues = employeeProfileService.GetInvitationUniqueColumnValuesDashboard(field);
            return Ok(uniqueValues);
        }

        [HttpPost("get-unique-column-values-department")]
        public IActionResult GetUniqueColumnValuesDepartment()
        {
            string field = "Department";
            var uniqueValues = employeeProfileService.GetInvitationUniqueColumnValuesDashboard(field);
            return Ok(uniqueValues);
        }

        [HttpPost("get-unique-column-values-section")]
        public IActionResult GetUniqueColumnValuesSection()
        {
            string field = "JobName";
            var uniqueValues = employeeProfileService.GetInvitationUniqueColumnValuesDashboard(field);
            return Ok(uniqueValues);
        }

        [HttpPost("get-unique-column-values-noreg")]
        public IActionResult GetUniqueColumnValuesNoReg()
        {
            string field = "NoReg";
            var uniqueValues = employeeProfileService.GetInvitationUniqueColumnValuesDashboard(field);
            return Ok(uniqueValues);
        }

        [HttpPost("get-unique-column-values-name")]
        public IActionResult GetUniqueColumnValuesName()
        {
            string field = "Name";
            var uniqueValues = employeeProfileService.GetInvitationUniqueColumnValuesDashboard(field);
            return Ok(uniqueValues);
        }

        [HttpPost("get-unique-column-values-status")]
        public IActionResult GetUniqueColumnValuesStatus()
        {
            string field = "Expr1";
            var uniqueValues = employeeProfileService.GetInvitationUniqueColumnValuesDashboard(field);
            return Ok(uniqueValues);
        }

        [HttpPost("get-unique-column-values-contract")]
        public IActionResult GetUniqueColumnValuesContract()
        {
            string field = "WorkContractText";
            var uniqueValues = employeeProfileService.GetInvitationUniqueColumnValuesDashboard(field);
            return Ok(uniqueValues);
        }

        [HttpPost("get-performance-developments")]
        public async Task<DataSourceResult> GetPerformanceDevelopments([FromForm] string noreg, [DataSourceRequest] DataSourceRequest request)
        {
            //var noreg = ServiceProxy.UserClaim.NoReg;

            return await MdmService.GetPerformanceDevelopments(noreg).ToDataSourceResultAsync(request);
        }

        [HttpPost("get-medical-histories-1")]
        public async Task<DataSourceResult> GetMedicalHistories([FromForm] string noreg, [DataSourceRequest] DataSourceRequest request)
        {
            //var noreg = ServiceProxy.UserClaim.NoReg;

            return await PersonalDataService.GetMedicalHistories(noreg).ToDataSourceResultAsync(request);
        }

        private string CreateUrl(string localPath)
        {
            var reg = new Regex(@".*\\wwwroot\\");
            var result = reg.Replace(localPath, @"~/");
            result = result.Contains("\\") ? result.Replace('\\', '/') : result.Trim('"');

            return result;

        }

        private string CreateCustomUrl(string localPath)
        {
            // Mencari posisi indeks folder "avatar"
            int avatarIndex = localPath.IndexOf("\\avatar\\", StringComparison.OrdinalIgnoreCase);

            if (avatarIndex != -1)
            {
                // Mengambil bagian dari path setelah folder "avatar"
                string pathAfterAvatar = localPath.Substring(avatarIndex + "\\avatar\\".Length);

                // Menambahkan tanda tilde (~) di depannya dan "/uploads" sebelumnya
                string customUrl = "~\\uploads\\avatar\\" + pathAfterAvatar;
                
                string result = customUrl.Replace("\\", "/");

                return result;
            }
            else
            {
                // Jika folder "avatar" tidak ditemukan, kembalikan input asli
                return localPath;
            }
        }

        [Route("download/{noReg}")]
        public async Task<IActionResult> Download(string noReg)
        {
            //PathProvider pathProvider = (PathProvider)HttpContext.RequestServices.GetService(typeof(PathProvider));
            //var configPath = ConfigService.GetConfig("Photo.Path", true)?.ConfigValue;
            //var filesPath = !string.IsNullOrEmpty(configPath) ? configPath : pathProvider.ContentPath("uploads/avatar");

            //if (!Regex.IsMatch(noReg, @"^[A-Za-z0-9]+$"))
            //    return BadRequest("Invalid NoReg");

            //var safeNoreg = Path.GetFileName(noReg);
            //var fileName = safeNoreg + ".jpg";
            //var fullFilePath = Path.Combine(filesPath, fileName);

            //var fullPathNormalized = Path.GetFullPath(fullFilePath);
            //var basePathNormalized = Path.GetFullPath(filesPath);

            //if (!fullPathNormalized.StartsWith(basePathNormalized))
            //    return BadRequest("Invalid file path");

            //if (!System.IO.File.Exists(fullPathNormalized))
            //    return NotFound();

            //using (var fileStream = System.IO.File.OpenRead(fullPathNormalized))
            //using (var ms = new MemoryStream())
            //{
            //    await fileStream.CopyToAsync(ms);
            //    return File(ms.ToArray(), "image/jpeg");
            //}
            if (string.IsNullOrWhiteSpace(noReg) || !Regex.IsMatch(noReg, @"^[A-Za-z0-9]+$"))
                return BadRequest("Invalid NoReg");

            var pathProvider = HttpContext.RequestServices.GetService<PathProvider>();
            var configPath = ConfigService.GetConfig("Photo.Path", true)?.ConfigValue;
            var baseFolder = !string.IsNullOrEmpty(configPath) ? configPath : pathProvider.ContentPath("uploads/avatar");

            var fileName = Path.GetFileName(noReg) + ".jpg";
            var filePath = Path.Combine(baseFolder, fileName);

            // Prevent path traversal
            var fullPath = Path.GetFullPath(filePath);
            var basePath = Path.GetFullPath(baseFolder);
            if (!fullPath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
                return BadRequest("Invalid file path");

            if (!System.IO.File.Exists(fullPath))
                return NotFound();

            var fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath);
            return File(fileBytes, "image/jpeg");
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadPhoto()
        {
            try
            {
                var files = Request.Form.Files;
                var noReg = Request.Form["noreg"].ToString();

                PathProvider pathProvider = (PathProvider)HttpContext.RequestServices.GetService(typeof(PathProvider));
                var configPath = ConfigService.GetConfig("Photo.Path",true)?.ConfigValue;
                //var configPath1 = ConfigService.GetConfig("Photo.Thumbnail",true)?.ConfigValue;
                //var configPath = "";
                //var configPath = configs.FirstOrDefault(x => x.ConfigKey == "Photo.Path")?.ConfigValue;

                var filesPath = !string.IsNullOrEmpty(configPath) ? configPath : pathProvider.ContentPath("uploads/avatar");
                //var filesPath = pathProvider.ContentPath("VSV-C003-016138\\photo");
                if (!Directory.Exists(filesPath))
                {
                    Directory.CreateDirectory(filesPath);
                }

                List<CommonFile> uploadedFiles = new List<CommonFile>();

                foreach (var file in files)
                {
                    var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName;
                    var fileSize = (int)(file.Length / 1024);

                    fileName = fileName.Contains("\\")
                        ? fileName.Trim('"').Substring(fileName.LastIndexOf("\\", StringComparison.Ordinal) + 1)
                        : fileName.Trim('"');

                    var commonFile = new CommonFile()
                    {
                        FileUrl = "",
                        FileName = fileName,
                        FileSize = fileSize,
                        FileType = file.ContentType,
                    };

                    //employeeProfileService.SaveCommonFile(commonFile);
                    // Dapatkan ekstensi tipe file
                    var fileExtension = Path.GetExtension(fileName);

                    var safeNoreg = Path.GetFileName(noReg);
                    fileName = safeNoreg + ".jpg";

                    var fullFilePath = Path.Combine(filesPath, fileName);

                    if (file.Length <= 0)
                    {
                        continue;
                    }

                    using (var stream = new FileStream(fullFilePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                        commonFile.FileUrl = fullFilePath;
                            //Url.ToAbsoluteContent(CreateUrl(fullFilePath));
                        employeeProfileService.SaveCommonFile(commonFile);
                        uploadedFiles.Add(commonFile);
                    }
                }

                return CreatedAtAction("Get", new { files = uploadedFiles });
            }
            catch (Exception ex)
            {
                return this.BadRequest(ex.Message);
            }


            /// <summary>
            /// Get list of tasks by username
            /// </summary>
            /// <remarks>
            /// Get list of tasks by username
            /// </remarks>
            /// <param name="request">DataSourceRequest Object</param>
            /// <returns>DataSourceResult Object</returns>

        }

        /*[HttpGet("generate-document")]
        public IActionResult GenerateReportSummary()
        {
            var pathProvider = ServiceProxy.GetPathProvider();
            var filename = "ReportEmployeeProfile";
            var templatePath = pathProvider.ContentPath($"templates\\{filename}.trdp");

            var bytes = EmployeeProfileService.GenerateReportSummary(templatePath);

            System.Net.Mime.ContentDisposition cd = new System.Net.Mime.ContentDisposition
            {
                FileName = "Report Summary.xlsx",
                Inline = true  // false = prompt the user for downloading;  true = browser to try to show the file inline
            };
            Response.Headers.Add("Content-Disposition", cd.ToString());
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        }*/

        [HttpGet("GetEmployeeProfile")]
        public IActionResult GetEmployeeProfile()
        {
            // Retrieve the roles from the user claims
            string roles = ServiceProxy.UserClaim.RoleText;

            // Define the roles that allow editing
            var allowedRoles = new[] { "ER_ADMIN", "OT_ADMIN", "SHD ADMIN", "HR_ADMIN" };

            // Check if the user's role is in the allowed list
            bool canEdit = allowedRoles.Contains(roles);

            // Return the JSON response
            return Ok(new { canEdit });
        }

        [HttpPost("updateprofile")]
        public ActionResult SaveProfileData(FormCollection form)
        {
            // Collect form data from the form collection
            string noreg = form["noreg"];
            string pob = form["Pob"];

            DateTime dob = Convert.ToDateTime(form["dob"]);

            string nik = form["nil"];
            string address = form["Address"];
            string kk = form["kk"];
            string accountnumber = form["accountnumber"];
            string religion = form["Religion"];
            string bg = form["Bg"];
            string taxstatus = form["taxstatus"];
            string bpjs = form["bpjs"];
            string insuranceno = form["insuranceno"];
            string npwp = form["npwp"];

            // Call the service to update the profile data
            employeeProfileService.SaveProfileData(noreg, pob, dob, nik, address, kk, accountnumber, religion, bg, taxstatus, bpjs, insuranceno, npwp);

            return RedirectToAction("Profile");
        }

        [HttpPost("updateeducation")]
        public IActionResult UpdateEducation([FromBody] EducationUpdateViewModel model)
        {
            if (model == null)
                return BadRequest("Invalid data.");

            model.ModifiedBy = ServiceProxy.UserClaim.NoReg;

            bool isUpdated = employeeProfileService.UpdateEducation(model);

            if (isUpdated)
                return Ok(new { success = true, message = "Education updated successfully." });

            return BadRequest(new { success = false, message = "Failed to update Education." });
        }

        [HttpDelete("{id}")] // <-- Menunggu permintaan DELETE dengan 'id' di URL
        public IActionResult DeleteEducation(Guid id)
        {
            try
            {
                employeeProfileService.DeleteEducation(id);
                return NoContent();
            }
            catch (Exception)
            {
                // Log error
                return BadRequest(new { message = "Failed to delete education data." });
            }
        }

        [HttpPost("updateprofilemain")]
        public IActionResult UpdateProfileMain([FromBody] PersonalMainProfileViewModel model)
        {
            if (model == null)
                return BadRequest("Invalid data.");

            model.ModifiedBy = ServiceProxy.UserClaim.NoReg;

            bool isUpdated = employeeProfileService.UpdateProfileMain(model);

            if (isUpdated)
                return Ok(new { success = true, message = "Profile updated successfully." });

            return BadRequest(new { success = false, message = "Failed to Profile Data." });
        }

        [HttpPost("addeducation")]
        public IActionResult AddEducation([FromBody] EducationUpdateViewModel model)
        {
            if (model == null)
                return BadRequest("Invalid data.");

            bool isUpdated = employeeProfileService.AddEducation(model);

            if (isUpdated)
                return Ok(new { success = true, message = "Education added successfully." });

            return BadRequest(new { success = false, message = "Failed to added Education." });
        }

        [HttpPost("addfamilymember")]
        public IActionResult AddFamilyMember([FromBody] FamilyMemberViewModel model)
        {
            if (model == null)
                return BadRequest("Invalid data.");

            bool isUpdated = employeeProfileService.AddFamilyMember(model);

            if (isUpdated)
                return Ok(new { success = true, message = "Family added successfully." });

            return BadRequest(new { success = false, message = "Failed to added Family Details." });
        }

        [HttpPost("updatefamilymember")]
        public IActionResult UpdateFamilyMember([FromBody] FamilyMemberViewModel model)
        {
            if (model == null)
                return BadRequest("Invalid data.");

            model.ModifiedBy = ServiceProxy.UserClaim.NoReg;
            bool isUpdated = employeeProfileService.UpdateFamilyMember(model);

            if (isUpdated)
                return Ok(new { success = true, message = "Family updated successfully." });

            return BadRequest(new { success = false, message = "Failed to added Family Details." });
        }

        [HttpPost("deletefamilymember")]
        public IActionResult DeleteFamilyMember([FromBody] FamilyMemberViewModel model)
        {
            if (model == null)
                return BadRequest("Invalid data.");

            model.ModifiedBy = ServiceProxy.UserClaim.NoReg;
            bool isUpdated = employeeProfileService.DeleteFamilyMember(model);

            if (isUpdated)
                return Ok(new { success = true, message = "Family Delete successfully." });

            return BadRequest(new { success = false, message = "Failed to delete Family Details." });
        }


        [HttpPost("updateprofiledata")]
        public IActionResult UpdateProfileData([FromBody] UpdateProfileViewModel model)
        {
            if (model == null)
                return BadRequest("Invalid data.");

            model.ModifiedBy = ServiceProxy.UserClaim.NoReg;
            bool isUpdated = employeeProfileService.UpdateProfileData(model);

            if (isUpdated)
                return Ok(new { success = true, message = "Profile updated successfully." });

            return BadRequest(new { success = false, message = "Failed to update Profile." });
        }

        [HttpPost("updateemergencycontact")]
        public IActionResult UpdateEmergencyContact([FromBody] EmergencyContanctViewModel model)
        {
            if (model == null)
                return BadRequest("Invalid data.");

            model.ModifiedBy = ServiceProxy.UserClaim.NoReg;
            bool isUpdated = employeeProfileService.UpdateEmergencyContact(model);

            if (isUpdated)
                return Ok(new { success = true, message = "Emergency Contact updated successfully." });

            return BadRequest(new { success = false, message = "Failed to update Emergency Contact." });
        }

        [HttpPost("uploadprofiles")]
        public IActionResult UploadFile()
        {
            var file = Request.Form.Files.FirstOrDefault();

            if (file == null || file.Length == 0)
            {
                return BadRequest(new { success = false, message = "No file uploaded or file is empty." });
            }

            using (var ms = new MemoryStream())
            {
                file.CopyTo(ms);
                ms.Position = 0; // Reset stream position

                try
                {
                    string resultMessage = employeeProfileService.ProcessUploadedFile(ms);

                    if (resultMessage == "SUCCESS")
                        return Ok(new { success = true, message = "File processed successfully." });

                    return BadRequest(new { success = false, message = resultMessage });
                }
                catch (Exception ex)
                {
                    return BadRequest(new { success = false, message = $"Upload error: {ex.Message}" });
                }
            }
        }


        [HttpGet("download-templateupload")]
        public IActionResult DownloadTemplate()
        {
            string wwwRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            string filePath = Path.Combine(wwwRootPath, "uploads", "excel-template", "Employee Profile Template.xlsx");
            //string filePath = Path.Combine(wwwRootPath, "uploads", "excel-template", "Employee Profile Report templates.xlsx");

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound(new { message = "Template file not found." });
            }

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/vnd.ms-excel", "Employee Profile Template.xlsx");
            //return File(fileBytes, "application/vnd.ms-excel", "Employee Profile Report templates.xlsx");
        }

        [HttpPost("download-report")]
        [AllowAnonymous]
        public IActionResult Download([FromForm] string directorate, [FromForm] string division, [FromForm] string department, [FromForm] string section,
            [FromForm] string noreg, [FromForm] string name, [FromForm] string employeeStatus, [FromForm] string class1, [FromForm] string category)
        {

            var getData = employeeProfileService.getPersonalDatas().ToList();

            var dvDivorce = PersonalDataService.GetPersonalDataEventQuery().Where(x => x.EventType == ApplicationForm.Divorce).Select(x => x.FamilyMemberId);
            var getDataFamily = employeeProfileService.getFamilyDatas()
                .Where(x => !dvDivorce.Contains(x.ID))
                .ToList();

            var getDataEducation = employeeProfileService.getEducationDatas().ToList();

            //var getDataFamily = employeeProfileService.getPersonalDataFamily().ToList();
            // var getData = MonitoringReportAllService.GetEmployeProfiles(startDate, endDate, isEligible).ToList();
            if (!string.IsNullOrEmpty(directorate) && directorate.Split(";").Length > 0)
            {
                getData = getData.Where(c => directorate.Split(";").Contains(c.DirOrgCode + "#" + c.Directorate)).ToList();
            }

            if (!string.IsNullOrEmpty(division) && division.Split(";").Length > 0)
            {
                getData = getData.Where(c => division.Split(";").Contains(c.DivOrgCode + "#" + c.Divisi)).ToList();
            }

            if (!string.IsNullOrEmpty(department) && department.Split(";").Length > 0)
            {
                getData = getData.Where(c => department.Split(";").Contains(c.DepOrgCode + "#" + c.Department)).ToList();
            }

            if (!string.IsNullOrEmpty(section) && section.Split(";").Length > 0)
            {
                getData = getData.Where(c => section.Split(";").Contains(c.SecOrgCode + "#" + c.Section)).ToList();
            }

            if (!string.IsNullOrEmpty(class1) && class1.Split(";").Length > 0)
            {
                getData = getData.Where(c => class1.Split(";").Contains(c.Nk_SubKelas)).ToList();
            }

            if (!string.IsNullOrEmpty(noreg) && noreg.Split(";").Length > 0)
            {
                getData = getData.Where(c => noreg.Split(";").Contains(c.Noreg)).ToList();
            }

            if (!string.IsNullOrEmpty(name) && name.Split(";").Length > 0)
            {
                getData = getData.Where(c => name.Split(";").Contains(c.Nama_Pegawai)).ToList();
            }

            if (!string.IsNullOrEmpty(category) && category.Split(";").Length > 0)
            {
                getData = getData.Where(c => category.Split(";").Contains(c.Expr1)).ToList();
            }

            if (!string.IsNullOrEmpty(employeeStatus) && employeeStatus.Split(";").Length > 0)
            {
                getData = getData.Where(c => employeeStatus.Split(";").Contains(c.WorkContractText)).ToList();
            }


            //education

            if (!string.IsNullOrEmpty(directorate) && directorate.Split(";").Length > 0)
            {
                getDataEducation = getDataEducation.Where(c => directorate.Split(";").Contains(c.DirOrgCode + "#" + c.Directorate)).ToList();
            }

            if (!string.IsNullOrEmpty(division) && division.Split(";").Length > 0)
            {
                getDataEducation = getDataEducation.Where(c => division.Split(";").Contains(c.DivOrgCode + "#" + c.Divisi)).ToList();
            }

            if (!string.IsNullOrEmpty(department) && department.Split(";").Length > 0)
            {
                getDataEducation = getDataEducation.Where(c => department.Split(";").Contains(c.DepOrgCode + "#" + c.Department)).ToList();
            }

            if (!string.IsNullOrEmpty(section) && section.Split(";").Length > 0)
            {
                getDataEducation = getDataEducation.Where(c => section.Split(";").Contains(c.SecOrgCode + "#" + c.Section)).ToList();
            }

            if (!string.IsNullOrEmpty(class1) && class1.Split(";").Length > 0)
            {
                getDataEducation = getDataEducation.Where(c => class1.Split(";").Contains(c.Nk_SubKelas)).ToList();
            }

            if (!string.IsNullOrEmpty(noreg) && noreg.Split(";").Length > 0)
            {
                getDataEducation = getDataEducation.Where(c => noreg.Split(";").Contains(c.Noreg)).ToList();
            }

            if (!string.IsNullOrEmpty(name) && name.Split(";").Length > 0)
            {
                getDataEducation = getDataEducation.Where(c => name.Split(";").Contains(c.Name)).ToList();
            }

            if (!string.IsNullOrEmpty(category) && category.Split(";").Length > 0)
            {
                getDataEducation = getDataEducation.Where(c => category.Split(";").Contains(c.Expr1)).ToList();
            }

            if (!string.IsNullOrEmpty(employeeStatus) && employeeStatus.Split(";").Length > 0)
            {
                getDataEducation = getDataEducation.Where(c => employeeStatus.Split(";").Contains(c.WorkContractText)).ToList();
            }


            //familymember

            if (!string.IsNullOrEmpty(directorate) && directorate.Split(";").Length > 0)
            {
                getDataFamily = getDataFamily.Where(c => directorate.Split(";").Contains(c.DirOrgCode + "#" + c.Directorate)).ToList();
            }

            if (!string.IsNullOrEmpty(division) && division.Split(";").Length > 0)
            {
                getDataFamily = getDataFamily.Where(c => division.Split(";").Contains(c.DivOrgCode + "#" + c.Divisi)).ToList();
            }

            if (!string.IsNullOrEmpty(department) && department.Split(";").Length > 0)
            {
                getDataFamily = getDataFamily.Where(c => department.Split(";").Contains(c.DepOrgCode + "#" + c.Department)).ToList();
            }

            if (!string.IsNullOrEmpty(section) && section.Split(";").Length > 0)
            {
                getDataFamily = getDataFamily.Where(c => section.Split(";").Contains(c.SecOrgCode + "#" + c.Section)).ToList();
            }

            if (!string.IsNullOrEmpty(class1) && class1.Split(";").Length > 0)
            {
                getDataFamily = getDataFamily.Where(c => class1.Split(";").Contains(c.Nk_SubKelas)).ToList();
            }

            if (!string.IsNullOrEmpty(noreg) && noreg.Split(";").Length > 0)
            {
                getDataFamily = getDataFamily.Where(c => noreg.Split(";").Contains(c.Noreg)).ToList();
            }

            if (!string.IsNullOrEmpty(name) && name.Split(";").Length > 0)
            {
                getDataFamily = getDataFamily.Where(c => name.Split(";").Contains(c.Nama_Pegawai)).ToList();
            }

            if (!string.IsNullOrEmpty(category) && category.Split(";").Length > 0)
            {
                getDataFamily = getDataFamily.Where(c => category.Split(";").Contains(c.Expr1)).ToList();
            }

            if (!string.IsNullOrEmpty(employeeStatus) && employeeStatus.Split(";").Length > 0)
            {
                getDataFamily = getDataFamily.Where(c => employeeStatus.Split(";").Contains(c.WorkContractText)).ToList();
            }

            string title = "Employee Profile";
            
            var fileName = string.Format(title + " Report.xlsx");

            using (var ms = new MemoryStream())
            {
                using (var package = new ExcelPackage())
                {
                    int rowIndex = 2;
                    var sheet = package.Workbook.Worksheets.Add("Personal Data");

                    //var cols = new[] { "Noreg", "Employee", "Entry Date", "Employee Category", "Employee Status", "Pers. Area", "SubArea", "Class",
                    //    "Nationality", "Place of Birth", "Date of Birth", "Gender", "Identity Number", "Family Card Number", "NPWP", "Email", "Address", "Status", "Religion", "Blood Group",
                    //    "Account Number", "Branch", "Tax Status", "BPJS Health Number", "BPJS Employment number", "Insurance Number", "No Dana Pensiun", "Division", "Department", "Section"};

                    var cols = new[] { "Noreg", "Employee Full Name", "Identity Card Name", "Class", "Entry Date TAM", "Entry Date Astra", "Work Location", "Employee Category", "Employee Status",
                        "Passport Number", "Nationality", "Phone Number", "Whatsapp Number", "Place of Birth", "Date of Birth", "Gender", "Identity Card Number", "Family Card Number", "NPWP",
                        "Email", "Personal Email", "KTP Address", "KTP Address Province", "KTP Address District", "KTP Address SubDistrict", "KTP Address Village", "KTP Address Postal Code", "KTP Address Rt",
                        "KTP Address Rw", "KTP Address Residential Status", "Domicile Address", "Domicile Address Province","Domicile Address District", "Domicile Address SubDistrict", 
                        "Domicile Address Administrative Village", "Domicile Address Postal Code", "Domicile Address Rt",
                        "Domicile Address Rw", "Domicile Address Residential Status", "Domicile Address Total Family", "Marital Status", "Religion", "Blood Group", "Account Number", "Branch", "Tax Status", "BPJS Health Number", 
                        "BPJS Employment Number", "Insurance Number", "Dana Pensiun Number", "SIM A Number", "SIM C Number", "Division", "Department", "Section"};

                    for (var i = 1; i <= cols.Length; i++)
                    {
                        sheet.Cells[1, i].Value = cols[i - 1];
                        sheet.Cells[1, i].Style.Font.Bold = true;
                        sheet.Cells[1, i].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);

                        sheet.Cells[1, i].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        sheet.Cells[1, i].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#b4c6e7"));

                    }
                    sheet.Column(1).Width = 20;
                    sheet.Column(2).Width = 30;
                    sheet.Column(3).Width = 30;
                    sheet.Column(4).Width = 20;
                    sheet.Column(5).Width = 20;
                    sheet.Column(6).Width = 20;
                    sheet.Column(7).Width = 20;
                    sheet.Column(8).Width = 20;
                    sheet.Column(9).Width = 20;
                    sheet.Column(10).Width = 30;
                    sheet.Column(11).Width = 20;
                    sheet.Column(12).Width = 20;
                    sheet.Column(13).Width = 20;
                    sheet.Column(14).Width = 25;
                    sheet.Column(15).Width = 25;
                    sheet.Column(16).Width = 25;
                    sheet.Column(17).Width = 25;
                    sheet.Column(18).Width = 25;
                    sheet.Column(19).Width = 30;
                    sheet.Column(20).Width = 20;
                    sheet.Column(21).Width = 20;
                    sheet.Column(22).Width = 20;
                    sheet.Column(23).Width = 25;
                    sheet.Column(24).Width = 25;
                    sheet.Column(25).Width = 25;
                    sheet.Column(26).Width = 25;
                    sheet.Column(27).Width = 25;
                    sheet.Column(28).Width = 25;
                    sheet.Column(29).Width = 25;
                    sheet.Column(30).Width = 25;
                    sheet.Column(31).Width = 30;
                    sheet.Column(32).Width = 30;
                    sheet.Column(33).Width = 30;
                    sheet.Column(34).Width = 30;
                    sheet.Column(35).Width = 30;
                    sheet.Column(36).Width = 30;
                    sheet.Column(37).Width = 30;
                    sheet.Column(38).Width = 30;
                    sheet.Column(39).Width = 30;
                    sheet.Column(40).Width = 30;
                    sheet.Column(41).Width = 30;
                    sheet.Column(42).Width = 30;
                    sheet.Column(43).Width = 30;
                    sheet.Column(44).Width = 30;
                    sheet.Column(45).Width = 30;
                    sheet.Column(46).Width = 30;
                    sheet.Column(47).Width = 30;
                    sheet.Column(48).Width = 30;
                    sheet.Column(49).Width = 30;
                    sheet.Column(50).Width = 30;
                    sheet.Column(51).Width = 30;
                    sheet.Column(52).Width = 30;
                    sheet.Column(53).Width = 30;
                    sheet.Column(54).Width = 30;
                    sheet.Column(55).Width = 30;

                    foreach (var data in getData)
                    {
                        sheet.Cells[rowIndex, 1].Value = data.Noreg;
                        sheet.Cells[rowIndex, 2].Value = data.Nama_Pegawai;
                        sheet.Cells[rowIndex, 3].Value = data.IdentityCardName;
                        sheet.Cells[rowIndex, 4].Value = data.Nk_SubKelas;
                        sheet.Cells[rowIndex, 5].Value = data.TamDate;
                        sheet.Cells[rowIndex, 5].Style.Numberformat.Format = "dd-MM-yyyy";
                        sheet.Cells[rowIndex, 6].Value = data.Tanggal_Masuk_Astra;
                        sheet.Cells[rowIndex, 6].Style.Numberformat.Format = "dd-MM-yyyy";
                        sheet.Cells[rowIndex, 7].Value = data.Lokasi;
                        sheet.Cells[rowIndex, 8].Value = data.Expr1;
                        sheet.Cells[rowIndex, 9].Value = data.WorkContractText;
                        sheet.Cells[rowIndex, 10].Value = data.PassportNumber;
                        sheet.Cells[rowIndex, 11].Value = data.NationalityCode;
                        sheet.Cells[rowIndex, 12].Value = data.PhoneNumber;
                        sheet.Cells[rowIndex, 13].Value = data.WhatsappNumber;
                        sheet.Cells[rowIndex, 14].Value = data.Tempat_Lahir;
                        sheet.Cells[rowIndex, 15].Value = data.Tanggal_Lahir;
                        sheet.Cells[rowIndex, 15].Style.Numberformat.Format = "dd-MM-yyyy";
                        sheet.Cells[rowIndex, 16].Value = data.Jenis_Kelamin;
                        sheet.Cells[rowIndex, 17].Value = data.Nik;
                        sheet.Cells[rowIndex, 18].Value = data.No_KK;
                        sheet.Cells[rowIndex, 19].Value = data.Npwp;
                        sheet.Cells[rowIndex, 20].Value = data.Email;
                        sheet.Cells[rowIndex, 21].Value = data.PersonalEmail;
                        sheet.Cells[rowIndex, 22].Value = data.Alamat;
                        sheet.Cells[rowIndex, 23].Value = data.Nama_Region;
                        sheet.Cells[rowIndex, 24].Value = data.Alamat_District;
                        sheet.Cells[rowIndex, 25].Value = data.Alamat_Subdistrict;
                        sheet.Cells[rowIndex, 26].Value = data.Alamat_AdministrativeVillage;
                        sheet.Cells[rowIndex, 27].Value = data.Kode_Pos;
                        sheet.Cells[rowIndex, 28].Value = data.Rt;
                        sheet.Cells[rowIndex, 29].Value = data.Rw;
                        sheet.Cells[rowIndex, 30].Value = data.AddressResidentialStatus;
                        sheet.Cells[rowIndex, 31].Value = data.Domisili;
                        sheet.Cells[rowIndex, 32].Value = data.Dom_Region;
                        sheet.Cells[rowIndex, 33].Value = data.Dom_District;
                        sheet.Cells[rowIndex, 34].Value = data.Dom_Subdistrict;
                        sheet.Cells[rowIndex, 35].Value = data.Dom_AdministrativeVillage;
                        sheet.Cells[rowIndex, 36].Value = data.Dom_PostalCode;
                        sheet.Cells[rowIndex, 37].Value = data.Dom_Rt;
                        sheet.Cells[rowIndex, 38].Value = data.Dom_Rw;
                        sheet.Cells[rowIndex, 39].Value = data.DomicileResidentialStatus;
                        sheet.Cells[rowIndex, 40].Value = data.TotalFamily;
                        sheet.Cells[rowIndex, 41].Value = data.Status_Nikah;
                        sheet.Cells[rowIndex, 42].Value = data.Agama;
                        sheet.Cells[rowIndex, 43].Value = data.BloodTypeCode;
                        sheet.Cells[rowIndex, 44].Value = data.AccountNumber;
                        sheet.Cells[rowIndex, 45].Value = data.Branch;
                        sheet.Cells[rowIndex, 46].Value = data.TaxStatus;
                        sheet.Cells[rowIndex, 47].Value = data.BPJS_Kesehatan;
                        sheet.Cells[rowIndex, 48].Value = data.BPJS_Ketenagakerjaan;
                        sheet.Cells[rowIndex, 49].Value = data.No_Aviva;
                        sheet.Cells[rowIndex, 50].Value = data.Dana_Pensiun_Astra;
                        sheet.Cells[rowIndex, 51].Value = data.SimANumber;
                        sheet.Cells[rowIndex, 52].Value = data.SimCNumber;
                        sheet.Cells[rowIndex, 53].Value = data.Divisi;
                        sheet.Cells[rowIndex, 54].Value = data.Department;
                        sheet.Cells[rowIndex, 55].Value = data.Section;



                        for (var i = 1; i <= cols.Length; i++)
                        {
                            sheet.Cells[rowIndex, i].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                        }

                        rowIndex++;
                    }

                    // Second sheet
                    rowIndex = 2; // Reset rowIndex for the second sheet
                    var sheet2 = package.Workbook.Worksheets.Add("Education");

                    var colsSheet2 = new[] { "Noreg", "Employee", "Education", "Name", "Major", "Country" };

                    for (var i = 1; i <= colsSheet2.Length; i++)
                    {
                        sheet2.Cells[1, i].Value = colsSheet2[i - 1];
                        sheet2.Cells[1, i].Style.Font.Bold = true;
                        sheet2.Cells[1, i].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);

                        sheet2.Cells[1, i].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        sheet2.Cells[1, i].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#b4c6e7"));


                    }

                    sheet2.Column(1).Width = 30;
                    sheet2.Column(2).Width = 40;
                    sheet2.Column(3).Width = 30;
                    sheet2.Column(4).Width = 40;
                    sheet2.Column(5).Width = 30;
                    sheet2.Column(6).Width = 40;

                    // Populate data for the second sheet
                    foreach (var data in getDataEducation)
                    {
                        sheet2.Cells[rowIndex, 1].Value = data.Noreg;
                        sheet2.Cells[rowIndex, 2].Value = data.Name;
                        sheet2.Cells[rowIndex, 3].Value = data.Education;
                        sheet2.Cells[rowIndex, 4].Value = data.Institute;
                        sheet2.Cells[rowIndex, 5].Value = data.Major;
                        sheet2.Cells[rowIndex, 6].Value = data.Country;

                        for (var i = 1; i <= colsSheet2.Length; i++)
                        {
                            sheet2.Cells[rowIndex, i].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                        }

                        rowIndex++;
                    }

                   // Sheet 3
                    rowIndex = 2; // Reset for Sheet 3
                    var sheet3 = package.Workbook.Worksheets.Add("Family Member");
                    var colsSheet3 = new[] { "NOREG", "EMPLOYEE NAME", "FAMILY TYPE", "FAMILY NAME", "IDENTITY NUMBER (NIK)", "LIFE STATUS", "DATE OF BIRTH", "PLACE OF BIRTH", "DATE OF DEATH", "GENDER", "CHILD STATUS",
                        "CHILD ORDER", "PHONE NUMBER" , "DOMICILE ADDRESS", "EDUCATION", "JOB", "BPJS HEALTH", "INSURANCE NUMBER"};

                    for (var i = 1; i <= colsSheet3.Length; i++)
                    {
                        sheet3.Cells[1, i].Value = colsSheet3[i - 1];
                        sheet3.Cells[1, i].Style.Font.Bold = true;
                        sheet3.Cells[1, i].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);

                        sheet3.Cells[1, i].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        sheet3.Cells[1, i].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#b4c6e7"));


                    }

                    sheet3.Column(1).Width = 30;
                    sheet3.Column(2).Width = 30;
                    sheet3.Column(3).Width = 30;
                    sheet3.Column(4).Width = 30;
                    sheet3.Column(5).Width = 30;
                    sheet3.Column(6).Width = 30;
                    sheet3.Column(7).Width = 30;
                    sheet3.Column(8).Width = 30;
                    sheet3.Column(9).Width = 30;
                    sheet3.Column(10).Width = 30;
                    sheet3.Column(11).Width = 30;
                    sheet3.Column(12).Width = 30;
                    sheet3.Column(13).Width = 30;
                    sheet3.Column(14).Width = 30;
                    sheet3.Column(15).Width = 30;
                    sheet3.Column(16).Width = 30;
                    sheet3.Column(17).Width = 30;
                    sheet3.Column(18).Width = 30;

                    foreach (var dataf in getDataFamily)
                    {
                        sheet3.Cells[rowIndex, 1].Value = dataf.Noreg;
                        sheet3.Cells[rowIndex, 2].Value = dataf.Nama_Pegawai;
                        sheet3.Cells[rowIndex, 3].Value = dataf.FamilyType;
                        sheet3.Cells[rowIndex, 4].Value = dataf.Name;
                        sheet3.Cells[rowIndex, 5].Value = dataf.Nik;
                        sheet3.Cells[rowIndex, 6].Value = dataf.LifeStatus;
                        sheet3.Cells[rowIndex, 7].Value = dataf.BirthDate;
                        sheet3.Cells[rowIndex, 7].Style.Numberformat.Format = "dd-MM-yyyy";
                        sheet3.Cells[rowIndex, 8].Value = dataf.BirthPlace;
                        sheet3.Cells[rowIndex, 9].Value = dataf.DeathDate;
                        sheet3.Cells[rowIndex, 9].Style.Numberformat.Format = "dd-MM-yyyy";
                        sheet3.Cells[rowIndex, 10].Value = dataf.Gender;
                        sheet3.Cells[rowIndex, 11].Value = dataf.ChildStatus;
                        sheet3.Cells[rowIndex, 12].Value = dataf.ChildOrder;
                        sheet3.Cells[rowIndex, 13].Value = dataf.PhoneNumber;
                        sheet3.Cells[rowIndex, 14].Value = dataf.Address;
                        sheet3.Cells[rowIndex, 15].Value = dataf.EducationLevel;
                        sheet3.Cells[rowIndex, 16].Value = dataf.Job;
                        sheet3.Cells[rowIndex, 17].Value = dataf.BpjsNumber;
                        sheet3.Cells[rowIndex, 18].Value = dataf.MemberNumber;
                        for (var i = 1; i <= colsSheet3.Length; i++)
                        {
                            sheet3.Cells[rowIndex, i].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                        }
                        rowIndex++;
                    }

                    package.SaveAs(ms);
                }
                //return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                //return ms;
                return Ok(Convert.ToBase64String(ms.ToArray()));
            }
        }
    }
    #endregion
    
    #region MVC Controller
        [Area("Reporting")]
        [Permission(PermissionKey.ViewEmployeeProfile)]
        public class EmployeeProfileController : MvcControllerBase
        {
            #region Domain Services
            /// <summary>
            /// Core service object
            /// </summary>
            public CoreService CoreService => ServiceProxy.GetService<CoreService>();

            /// <summary>
            /// Employee leave service object
            /// </summary>
            public EmployeeLeaveService EmployeeLeaveService => ServiceProxy.GetService<EmployeeLeaveService>();
            public ClaimBenefitService claimBenefitService => ServiceProxy.GetService<ClaimBenefitService>();

            /// <summary>
            /// Profile service object
            /// </summary>
            protected ProfileService ProfileService { get { return ServiceProxy.GetService<ProfileService>(); } }

            /// <summary>
            /// Master data management service object
            /// </summary>
            protected MdmService MdmService { get { return ServiceProxy.GetService<MdmService>(); } }
            protected EmployeeProfileService EmployeeService { get { return ServiceProxy.GetService<EmployeeProfileService>(); } }
            #endregion
     
 
            public IActionResult Index()
            {
                //var dataClass = EmployeeService.getClass();
                
                ViewBag.directorateData = GetDirectorateTree();
                ViewBag.divisionData = GetDivisionTree();
                ViewBag.classData = GetClassTree();
                
                return View();
            }


        [Permission(PermissionKey.ViewEmployeeProfileDetail)]
        public IActionResult ProfileEdit(string noreg)
        {
            var employeeData = EmployeeService.GetEmployeeOrganizationObjects(noreg);
            var subgrupData = EmployeeService.GetPersonalSubArea();
            var statusData = ProfileService.GetStatus(noreg);
            var profileData = ProfileService.GetProfiles(noreg);
            var nationalityListData = EmployeeService.GetNationalityList();
            var regionListData = EmployeeService.GetRegionList();
            var districtListData = EmployeeService.GetDistrictList();
            var subDistrictListData = EmployeeService.GetSubDistrictList();
            var postalCodeListData = EmployeeService.GetPostalCodeList();
            var admVillageListData = EmployeeService.GetAdministrativeVillageList();
            var adrResidenStatusData = EmployeeService.GetResidentialStatus();
            var maritalStatusData = EmployeeService.GetMaritalStatus();
            var religionListData = EmployeeService.GetReligion();
            var bloodListData = EmployeeService.GetBloodType();
            var collageListData = EmployeeService.GetCollage();
            var bankListData = EmployeeService.GetBank();
            var bankSelectData = EmployeeService.SelectedBank(noreg);
            var educationLevelData = EmployeeService.GetEducationLevel();
            var residentialStatusData = EmployeeService.GetResidentialStatus();
            var familyTypeListData = EmployeeService.GetFamilyType();
            var persAreaListData = EmployeeService.GetPersonalArea();
            var majorListData = EmployeeService.GetMajors();
            var workContractListData = EmployeeService.GetWorkContract();
            var pobListData = EmployeeService.GetBirthPlace();
            var majorListerData = EmployeeService.GetMajors();
            var taxListData = EmployeeService.GetTaxStatus(noreg);
            var taxSelectData = EmployeeService.SelectedTax(noreg);
            var cbfData = claimBenefitService.GetDataBank(noreg, DateTime.Now);
            var phoneListData = EmployeeService.GetPhoneCode();
            var employeeEducationData = EmployeeService.GetPersonalDataEducation(noreg);
            var profileEducationData = ProfileService.GetProfileDataEducation(noreg);
            var familyData = ProfileService.GetFamilyMembers(noreg);
            var emergencyContactData = ProfileService.GetEmergencyContact(noreg);
            var organizationAssignmentsData = ProfileService.GetOrganizationalAssignments(noreg);
            var trainingData = ProfileService.GetDataTraining(noreg);
            var benefitData = ProfileService.GetProfileDataClaimBenefit(noreg);
            var statusContractData = EmployeeService.getContranct(noreg);

            ViewData["NameUser"] = HttpUtility.HtmlEncode(employeeData?.FirstOrDefault()?.Name ?? ""); 

            ViewData["Kelas"] = HttpUtility.HtmlEncode(employeeData?.FirstOrDefault()?.Kelas ?? "");

            ViewData["subgrup"] = employeeData?.FirstOrDefault()?.EmployeeSubGroupText ?? "";

            ViewData["subgrupList"] = subgrupData.ToList();

            ViewData["Expr1"] = statusData ?? "";

            ViewData["Nationality"] = HttpUtility.HtmlEncode(profileData?.Nationality ?? "");

            ViewData["NationalityList"] = nationalityListData.ToList();


            var datebirth = profileData?.BirthDate; 

            if (datebirth.HasValue)
            {
                ViewData["dob"] = datebirth.Value.ToString("yyyy-MM-dd");
               
            }
            else
            {
                ViewData["dob"] = ""; 
            }

            ViewData["gender"] = HttpUtility.HtmlEncode(profileData?.Gender ?? "");

            ViewData["RegionList"] = regionListData.ToList();

            ViewData["DistrictList"] = districtListData.ToList();

            ViewData["SubDistrictList"] = subDistrictListData.ToList();

            ViewData["AdrPostCodeList"] = postalCodeListData.ToList();

            ViewData["AdmVillageList"] = admVillageListData.ToList();

            ViewData["adrResidential"] = profileData.ResidentialStatus ?? "";

            ViewData["AdrResidentialStatus"] = adrResidenStatusData.ToList();

            ViewData["address"] = HttpUtility.HtmlEncode(profileData?.Address ?? "");

            //Coba KTP Address Fatih 17 june 2025

            ViewData["adrProvince"] = HttpUtility.HtmlEncode(profileData?.Region ?? "");
            ViewData["adrDistrict"] = HttpUtility.HtmlEncode(profileData?.District ?? "");
            ViewData["adrSubDistrict"] = HttpUtility.HtmlEncode(profileData?.SubDistrict ?? "");
            ViewData["addressLine"] = HttpUtility.HtmlEncode(profileData?.Address ?? "");
            ViewData["adrAdmVillage"] = HttpUtility.HtmlEncode(profileData?.AdministrativeVillage ?? "");
            ViewData["adrRw"] = HttpUtility.HtmlEncode(profileData?.Rw ?? "");
            ViewData["adrRt"] = HttpUtility.HtmlEncode(profileData?.Rt ?? "");
            ViewData["adrPostalCode"] = HttpUtility.HtmlEncode(profileData?.PostalCode ?? "");

            var addressLine = profileData?.Address ?? "";
            var rt = profileData?.Rt ?? "";
            var rw = profileData?.Rw ?? "";
            var administrativeVillage = profileData?.AdministrativeVillage ?? "";
            var subDistrict = profileData?.SubDistrict ?? "";
            var district = profileData?.District ?? "";
            var region = profileData?.Region ?? "";
            var postalCode = profileData?.PostalCode ?? "";

            var addressParts = new List<string>();

            if (!string.IsNullOrEmpty(addressLine)) addressParts.Add(addressLine);

            var rtRwCombined = "";
            if (!string.IsNullOrEmpty(rt) && !string.IsNullOrEmpty(rw))
            {
                rtRwCombined = $"RT {rt}/RW {rw}";
            }
            else if (!string.IsNullOrEmpty(rt))
            {
                rtRwCombined = $"RT {rt}";
            }
            else if (!string.IsNullOrEmpty(rw))
            {
                rtRwCombined = $"RW {rw}";
            }
            if (!string.IsNullOrEmpty(rtRwCombined)) addressParts.Add(rtRwCombined);

            if (!string.IsNullOrEmpty(administrativeVillage)) addressParts.Add(administrativeVillage);
            if (!string.IsNullOrEmpty(subDistrict)) addressParts.Add(subDistrict);
            if (!string.IsNullOrEmpty(district)) addressParts.Add(district);
            if (!string.IsNullOrEmpty(region)) addressParts.Add(region);
            if (!string.IsNullOrEmpty(postalCode)) addressParts.Add(postalCode);

            var fullAddress = string.Join(", ", addressParts);

            ViewData["Addresss"] = HttpUtility.HtmlEncode(fullAddress);
            //Coba KTP Address Fatih

            // Coba Domicile Address Fatih 17 June 2025
            ViewData["domicileProvince"] = HttpUtility.HtmlEncode(profileData?.DomicileRegion ?? "");
            ViewData["domicileDistrict"] = HttpUtility.HtmlEncode(profileData?.DomicileDistrict ?? "");
            ViewData["domicileSubDistrict"] = HttpUtility.HtmlEncode(profileData?.DomicileSubDistrict ?? "");
            ViewData["domicileAddress"] = HttpUtility.HtmlEncode(profileData?.Domicile ?? "");
            ViewData["domicileAdmVillage"] = HttpUtility.HtmlEncode(profileData?.DomicileAdministrativeVillage ?? "");
            ViewData["domicileRw"] = HttpUtility.HtmlEncode(profileData?.DomicileRw ?? "");
            ViewData["domicileRt"] = HttpUtility.HtmlEncode(profileData?.DomicileRt ?? "");
            ViewData["domicilePostalCode"] = HttpUtility.HtmlEncode(profileData?.DomicilePostalCode ?? "");
            ViewData["domicileResidential"] = HttpUtility.HtmlEncode(profileData?.DomicileResidentialStatus ?? "");
            ViewData["domicileTotalFamily"] = HttpUtility.HtmlEncode(profileData?.DomicileTotalFamily ?? "");

            var domicileAddress = profileData?.Domicile ?? "";
            var domicileRt = profileData?.DomicileRt ?? "";
            var domicileRw = profileData?.DomicileRw ?? "";
            var domicileAdmVillage = profileData?.DomicileAdministrativeVillage ?? "";
            var domicileSubDistrict = profileData?.DomicileSubDistrict ?? "";
            var domicileDistrict = profileData?.DomicileDistrict ?? "";
            var domicileRegion = profileData?.DomicileRegion ?? "";
            var domicilePostalCode = profileData?.DomicilePostalCode ?? "";

            var domicileAddressParts = new List<string>();

            if (!string.IsNullOrEmpty(domicileAddress)) domicileAddressParts.Add(domicileAddress);

            var domicileRtRwCombined = "";
            if (!string.IsNullOrEmpty(domicileRt) && !string.IsNullOrEmpty(domicileRw))
            {
                domicileRtRwCombined = $"RT {domicileRt}/RW {domicileRw}";
            }
            else if (!string.IsNullOrEmpty(domicileRt))
            {
                domicileRtRwCombined = $"RT {domicileRt}";
            }
            else if (!string.IsNullOrEmpty(domicileRw))
            {
                domicileRtRwCombined = $"RW {domicileRw}";
            }
            if (!string.IsNullOrEmpty(domicileRtRwCombined)) domicileAddressParts.Add(domicileRtRwCombined);

            if (!string.IsNullOrEmpty(domicileAdmVillage)) domicileAddressParts.Add(domicileAdmVillage);
            if (!string.IsNullOrEmpty(domicileSubDistrict)) domicileAddressParts.Add(domicileSubDistrict);
            if (!string.IsNullOrEmpty(domicileDistrict)) domicileAddressParts.Add(domicileDistrict);
            if (!string.IsNullOrEmpty(domicileRegion)) domicileAddressParts.Add(domicileRegion);
            if (!string.IsNullOrEmpty(domicilePostalCode)) domicileAddressParts.Add(domicilePostalCode);

            var domicileFullAddress = string.Join(", ", domicileAddressParts);

            ViewData["Domicile"] = HttpUtility.HtmlEncode(domicileFullAddress);
            // Coba Domicile Address Fatih

            //var domicile = ProfileService.GetProfiles(noreg);
            //ViewData["domicile"] = address?.Domicile ?? "";

            ViewData["marCerti"] = HttpUtility.HtmlEncode(profileData?.MarriedCertificate ?? "");

            ViewData["divCerti"] = HttpUtility.HtmlEncode(profileData?.DivorceCertificate ?? "");

            ViewData["marDate"] = HttpUtility.HtmlEncode(profileData?.MarriedDate?.ToString("yyyy-MM-dd") ?? "");

            ViewData["divDate"] = HttpUtility.HtmlEncode(profileData?.DivorceDate?.ToString("yyyy-MM-dd") ?? "");

            ViewData["email"] = HttpUtility.HtmlEncode(profileData?.Email ?? "");

            ViewData["MartalStatus"] = HttpUtility.HtmlEncode(profileData.MartalStatus ?? "");

            ViewData["maritalStatus"] = maritalStatusData.ToList();

            ViewData["religion"] = HttpUtility.HtmlEncode(profileData?.Religion ?? "");

            ViewData["religionList"] = religionListData.ToList();

            ViewData["bg"] = HttpUtility.HtmlEncode(profileData?.BloodType ?? "");

            ViewData["bloodList"] = bloodListData.ToList();

            ViewData["np"] = HttpUtility.HtmlEncode(profileData?.DanaPensiunAstra ?? "");

            ViewData["collageList"] = collageListData.ToList();

            ViewData["bankList"] = bankListData.ToList();

            ViewData["bankSelect"] = bankSelectData; 

            ViewData["eduLevelList"] = educationLevelData.ToList();

            ViewData["residentialList"] = residentialStatusData.ToList();

            //var familydata = ProfileService.GetFamilyMembers(noreg);
            //ViewData["familydata"] = familydata?.FirstOrDefault()?.FamilyTypeCode ?? "";

            ViewData["familyTypeList"] = familyTypeListData.ToList();

            ViewData["noregs"] = HttpUtility.HtmlEncode(profileData?.NoReg ?? "");


            var entryDate = employeeData?.FirstOrDefault()?.EntryDate;

            if (entryDate.HasValue)
            {
                ViewData["ed"] = entryDate.Value.ToString("yyyy-MM-dd");
                ViewData["edDisplay"] = entryDate.Value.ToString("dd/MM/yyyy");
            }
            else
            {
                ViewData["ed"] = "";
            }

            ViewData["subArea"] = HttpUtility.HtmlEncode(profileData?.SubDistrict ?? "");

            ViewData["persArea"] = employeeData?.FirstOrDefault()?.PersonnelArea ?? "";

            ViewData["persAreaList"] = persAreaListData.ToList();

            ViewData["majorList"] = majorListData.ToList();

            ViewData["es"] = statusData ?? "";

            ViewData["workContractList"] = workContractListData.ToList();

            ViewData["pob"] = HttpUtility.HtmlEncode(profileData?.BirthPlace ?? "");

            ViewData["pobList"] = pobListData.ToList();

            pobListData.Select(p => p.ToLower()).ToList(); // Convert each value to lowercase

            ViewData["pobLister"] = pobListData;

            majorListerData.Select(p => p.ToLower()) // Convert each value to lowercase
             .ToList();

            ViewData["majorLister"] = majorListerData;


            ViewData["idNumber"] = HttpUtility.HtmlEncode(profileData?.Nik ?? "");

            ViewData["kk"] = HttpUtility.HtmlEncode(profileData?.KKNumber ?? "");

            ViewData["npwp"] = HttpUtility.HtmlEncode(profileData?.Npwp ?? "");

            ViewData["taxStatus"] = HttpUtility.HtmlEncode(profileData?.TaxStatus ?? "");

            ViewData["taxList"] = taxListData.ToList();

            ViewData["taxSelect"] = taxSelectData;

            ViewData["accountNumber"] = HttpUtility.HtmlEncode(profileData?.AccountNumber ?? "");

            ViewData["accountName"] = HttpUtility.HtmlEncode(profileData?.AccountName ?? "");

            ViewData["cbf"] = cbfData?.FirstOrDefault()?.Branch ?? "";

            ViewData["bpjs"] = HttpUtility.HtmlEncode(profileData?.BpjsNumber ?? "");

            ViewData["bpjsen"] = HttpUtility.HtmlEncode(profileData?.BpjsKetenagakerjaan ?? "");

            ViewData["insuranceNo"] = HttpUtility.HtmlEncode(profileData?.InsuranceNumber ?? "");

            ViewData["simaNumber"] = HttpUtility.HtmlEncode(profileData?.SimANumber ?? "");

            ViewData["simcNumber"] = HttpUtility.HtmlEncode(profileData?.SimCNumber ?? "");

            ViewData["passportNumber"] = HttpUtility.HtmlEncode(profileData?.PassportNumber ?? "");

            ViewData["identityName"] = HttpUtility.HtmlEncode(profileData?.IdentityCardName ?? "");

            //var phonenum = ProfileService.GetProfiles(noreg);
            //ViewData["phoneNum"] = phonenum?.PhoneNumber ?? "";

            //var phoneList = EmployeeService.GetPhoneCode();
            //ViewData["phoneList"] = phoneList.ToList();

            string fullPhone = profileData?.PhoneNumber?.Replace(" ", "") ?? "";

            var phoneList = phoneListData.ToList();
            string selectedCode = phoneList.FirstOrDefault(code => fullPhone.StartsWith(code)) ?? "+62";
            string phoneWithoutCode = fullPhone.StartsWith(selectedCode) ? fullPhone.Substring(selectedCode.Length) : fullPhone;

            ViewData["selectedPhoneCode"] = selectedCode;
            ViewData["phoneNum"] = HttpUtility.HtmlEncode(phoneWithoutCode);
            ViewData["phoneList"] = phoneList;

            //var wanum = ProfileService.GetProfiles(noreg);
            //ViewData["waNum"] = wanum?.WhatsappNumber ?? "";

            string fullWaNum = profileData?.WhatsappNumber?.Replace(" ", "") ?? "";
            string selectedWaCode = phoneList.FirstOrDefault(code => fullWaNum.StartsWith(code)) ?? "+62"; // Default jika tidak ada
            string waWithoutCode = fullWaNum.StartsWith(selectedWaCode) ? fullWaNum.Substring(selectedWaCode.Length) : fullWaNum;

            ViewData["selectedWaCode"] = selectedWaCode;
            ViewData["waNum"] = HttpUtility.HtmlEncode(waWithoutCode);

            ViewData["personalemail"] = HttpUtility.HtmlEncode(profileData?.PersonalEmail ?? "");

            ViewData["EducationData"] = employeeEducationData;

            ViewData["ProfileDatas"] = profileData;
            //var data = MdmService.GetEmployeeOrganizationObjects(noreg);
            ViewData["PersonalData"] = employeeData;
            ViewData["Education"] = profileEducationData;
            ViewData["FamilyMember"] = familyData;


            // Handle phone 1
            string fullPhone1 = emergencyContactData?.EmergencyCallPhoneNumber?.Replace(" ", "") ?? "";
            string selectedCode1 = phoneList.FirstOrDefault(code => fullPhone1.StartsWith(code)) ?? "+62";
            string phoneWithoutCode1 = fullPhone1.StartsWith(selectedCode1) ? fullPhone1.Substring(selectedCode1.Length) : fullPhone1;

            // Handle phone 2
            string fullPhone2 = emergencyContactData?.EmergencyCallPhoneNumber2?.Replace(" ", "") ?? "";
            string selectedCode2 = phoneList.FirstOrDefault(code => fullPhone2.StartsWith(code)) ?? "+62";
            string phoneWithoutCode2 = fullPhone2.StartsWith(selectedCode2) ? fullPhone2.Substring(selectedCode2.Length) : fullPhone2;

            // Set ViewData
            ViewData["ecPhoneList"] = phoneList;
            ViewData["ecPhoneCode"] = selectedCode1;
            ViewData["ecPhone"] = HttpUtility.HtmlEncode(phoneWithoutCode1);
            ViewData["ecPhoneCode2"] = selectedCode2;
            ViewData["ecPhone2"] = HttpUtility.HtmlEncode(phoneWithoutCode2);

            ViewData["emcContact"] = emergencyContactData;
            ViewData["ecName"] = HttpUtility.HtmlEncode(emergencyContactData.EmergencyCallName);
            ViewData["ecRelation"] = HttpUtility.HtmlEncode(emergencyContactData.EmergencyCallRelationshipCode);
            ViewData["ecName2"] = HttpUtility.HtmlEncode(emergencyContactData.EmergencyCallName2);
            ViewData["ecRelation2"] = HttpUtility.HtmlEncode(emergencyContactData.EmergencyCallRelationshipCode2);

            ViewData["OrganizationalAssignment"] = organizationAssignmentsData;
            ViewData["DataTraining"] = trainingData;

            var claimType = benefitData.Select(x => x.AllowanceType).Distinct().ToList();
            ViewData["claimType"] = claimType;
            ViewData["HistoryBenefitClaim"] = benefitData;

            ViewData["Contract"] = statusContractData;

            var photo = GetPhotoUrlByNoReg(noreg);
            ViewData["Photo"] = photo;

            // Return the partial view for modal
            return PartialView("ProfileEdit"); // The partial view name can be changed as needed
        }

        [HttpGet]
        public JsonResult SearchAdmVillage(string term)
        {
            var results = EmployeeService.SearchAdministrativeVillage(term);
            // Format secara manual agar sesuai dengan Select2
            var formatted = results.Select(x => new { id = x, text = x }).ToList();
            return Json(formatted);
        }

        // [Permission(PermissionKey.ViewEmployeeProfileDetail)]
        public IActionResult Profile(string noreg)
            {
                //string Noreg = ServiceProxy.UserClaim.NoReg;

                ViewData["ProfileData"] = ProfileService.GetProfile(noreg);
                //var data = MdmService.GetEmployeeOrganizationObjects(noreg);
                var data = EmployeeService.GetEmployeeOrganizationObjects(noreg);
                ViewData["PersonalData"] = data;
                var edu = ProfileService.GetProfileDataEducation(noreg);
                ViewData["Education"] = edu;
                var fam = ProfileService.GetFamilyMembers(noreg);
                ViewData["FamilyMember"] = fam;
                ViewData["OrganizationalAssignment"] = ProfileService.GetOrganizationalAssignments(noreg);
                var training = ProfileService.GetDataTraining(noreg);
                ViewData["DataTraining"] = training;
                var benefit = ProfileService.GetProfileDataClaimBenefit(noreg);

                var claimType = benefit.Select(x => x.AllowanceType).Distinct().ToList();
                ViewData["claimType"] = claimType;
                ViewData["HistoryBenefitClaim"] = benefit;
                var status = EmployeeService.GetStatus(noreg);
                ViewData["Status"] = status;

                var statusContract = EmployeeService.getContranct(noreg);
                ViewData["Contract"] = statusContract;
                
                var photo = GetPhotoUrlByNoReg(noreg);
                ViewData["Photo"] = photo;

                return View();
            }

            //public IActionResult ProfileEdit(string noreg)
            //{
            //    //string Noreg = ServiceProxy.UserClaim.NoReg;

            //    ViewData["ProfileData"] = ProfileService.GetProfile(noreg);
            //    //var data = MdmService.GetEmployeeOrganizationObjects(noreg);
            //    var data = EmployeeService.GetEmployeeOrganizationObjects(noreg);
            //    ViewData["PersonalData"] = data;
            //    var edu = ProfileService.GetProfileDataEducation(noreg);
            //    ViewData["Education"] = edu;
            //    var fam = ProfileService.GetFamilyMembers(noreg);
            //    ViewData["FamilyMember"] = fam;
            //    ViewData["OrganizationalAssignment"] = ProfileService.GetOrganizationalAssignments(noreg);
            //    var training = ProfileService.GetDataTraining(noreg);
            //    ViewData["DataTraining"] = training;
            //    var benefit = ProfileService.GetProfileDataClaimBenefit(noreg);

            //    var claimType = benefit.Select(x => x.AllowanceType).Distinct().ToList();
            //    ViewData["claimType"] = claimType;
            //    ViewData["HistoryBenefitClaim"] = benefit;
            //    var status = EmployeeService.GetStatus(noreg);
            //    ViewData["Status"] = status;

            //    var statusContract = EmployeeService.getContranct(noreg);
            //    ViewData["Contract"] = statusContract;

            //    var photo = GetPhotoUrlByNoReg(noreg);
            //    ViewData["Photo"] = photo;

            //    return View();
            //}

            [Permission(PermissionKey.ViewSubordinateEmployeeProfile)]
            public IActionResult SubProfile(string noreg)
            {
                ViewData["ProfileData"] = ProfileService.GetProfile(noreg);
                ViewData["PersonalData"] = MdmService.GetEmployeeOrganizationObjects(noreg);
                ViewData["FamilyMember"] = ProfileService.GetFamilyMembers(noreg);
                ViewData["EmployeeLeave"] = EmployeeLeaveService.GetByNoreg(noreg);
                ViewData["BenefitClaimHistories"] = ProfileService.GetProfileDataClaimBenefit(noreg);

                return View();
            }

            
        public string GetPhotoUrlByNoReg(string noReg)
            {
                string noregs = System.Web.HttpUtility.HtmlEncode(noReg);
                // Sesuaikan dengan path folder penyimpanan foto
                PathProvider pathProvider = (PathProvider)HttpContext.RequestServices.GetService(typeof(PathProvider));
                //var filesPath = pathProvider.ContentPath("uploads/avatar");

                var configPath = ConfigService.GetConfig("Photo.Path", true)?.ConfigValue;
                //var configPath1 = ConfigService.GetConfig("Photo.Thumbnail",true)?.ConfigValue;

                var filesPath = !string.IsNullOrEmpty(configPath) ? configPath : pathProvider.ContentPath("uploads/avatar");
                //var filesPath = pathProvider.ContentPath("VSV-C003-016138\\photo");

                // Format nama file
                var fileName = noregs + ".jpg"; // Foto disimpan dengan format NoReg.jpg
            var safeFileName = Path.GetFileName(fileName);
                var fullFilePath = Path.Combine(filesPath, safeFileName);

                // Cek apakah file ada
                if (System.IO.File.Exists(fullFilePath))
                {
                    // Buat URL absolut ke foto
                    var photoUrl = Url.ToAbsoluteContent(CreateCustomUrl(fullFilePath));
                    return photoUrl;
                }

                // Jika file tidak ada, kembalikan null
                return null;
            }

            private string CreateUrl(string localPath)
            {
                var reg = new Regex(@".*\\wwwroot\\");
                var result = reg.Replace(localPath, @"~/");
                result = result.Replace("\\", "/").Trim('"');

                return result;

            }

            private string CreateCustomUrl(string localPath)
            {
                // Mencari posisi indeks folder "avatar"
                int avatarIndex = localPath.IndexOf("\\avatar\\", StringComparison.OrdinalIgnoreCase);

                if (avatarIndex != -1)
                {
                    // Mengambil bagian dari path setelah folder "avatar"
                    string pathAfterAvatar = localPath.Substring(avatarIndex + "\\avatar\\".Length);

                    // Menambahkan tanda tilde (~) di depannya dan "/uploads" sebelumnya
                    string customUrl = "~\\uploads\\avatar\\" + pathAfterAvatar;
                    //string customUrl = "~\\VSV-C003-016138\\photo\\" + pathAfterAvatar;
                    string result =  customUrl.Replace("\\", "/");

                    return result;
                }
                else
                {
                    // Jika folder "avatar" tidak ditemukan, kembalikan input asli
                    return localPath;
                }
            }

            public List<DropDownTreeItemModel> GetDirectorateTree()
            {
                // Get list of one level hierarchy by noreg, position code, and key date.
                var directorates = ServiceProxy.GetService<PayrollReportService>().GetDirectorates();

                // Create new TreeViewItemModel list object.
                var listDropDownTreeItem = new List<DropDownTreeItemModel>();

                // Enumerate through subordinates data.
                foreach (var directorate in directorates)
                {
                    // Create new TreeViewItemModel for each subordinate organization data and add it into the sub-list.
                    listDropDownTreeItem.Add(new DropDownTreeItemModel
                    {
                        Value = string.Format("{0}#{1}", directorate.OrgCode, directorate.ObjectText),
                        Text = directorate.ObjectText,
                        // Set expanded by default.
                        Expanded = false
                    });
                }

                listDropDownTreeItem = listDropDownTreeItem.OrderBy(x => x.Text).ToList();

                // Return partial view with given view model.
                return listDropDownTreeItem;
            }

            public List<DropDownTreeItemModel> GetDivisionTree()
            {
                // Get list of one level hierarchy by noreg, position code, and key date.
                var divisions = ServiceProxy.GetService<PayrollReportService>().GetDivisions();

                // Create new TreeViewItemModel list object.
                var listDropDownTreeItem = new List<DropDownTreeItemModel>();

                // Enumerate through subordinates data.
                foreach (var division in divisions)
                {
                    // Create new TreeViewItemModel for each subordinate organization data and add it into the sub-list.
                    listDropDownTreeItem.Add(new DropDownTreeItemModel
                    {
                        Value = string.Format("{0}#{1}", division.OrgCode, division.ObjectText),
                        Text = division.ObjectText,
                        // Set expanded by default.
                        Expanded = false
                    });
                }

                listDropDownTreeItem = listDropDownTreeItem.OrderBy(x => x.Text).ToList();

                // Return partial view with given view model.
                return listDropDownTreeItem;
            }

            public List<DropDownTreeItemModel> GetClassTree()
            {
                // Get list of one level hierarchy by noreg, position code, and key date.
                var data = ServiceProxy.GetService<EmployeeProfileService>().getClass();

                // Create new TreeViewItemModel list object.
                var listDropDownTreeItem = new List<DropDownTreeItemModel>();

                // Enumerate through subordinates data.
                foreach (var item in data)
                {
                    // Create new TreeViewItemModel for each subordinate organization data and add it into the sub-list.
                    listDropDownTreeItem.Add(new DropDownTreeItemModel
                    {
                        Value = item,
                        Text = item,
                        // Set expanded by default.
                        Expanded = false
                    });
                }

                listDropDownTreeItem = listDropDownTreeItem.OrderBy(x => x.Text).ToList();

                // Return partial view with given view model.
                return listDropDownTreeItem;
            }


    }
    #endregion

}