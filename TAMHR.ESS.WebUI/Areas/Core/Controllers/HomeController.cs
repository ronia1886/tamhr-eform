using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using TAMHR.ESS.Infrastructure.Web.Extensions;
using System.Linq;
using System.Web;
using TAMHR.ESS.Infrastructure.Web.Runtime.Providers;
using TAMHR.ESS.Infrastructure.Web.ViewComponents;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;

namespace TAMHR.ESS.WebUI.Areas.Core.Controllers
{
    /// <summary>
    /// Common controller
    /// </summary>
    [Area("Core")]
    public class HomeController : MvcControllerBase
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

        /// <summary>
        /// Profile service object
        /// </summary>
        protected ProfileService ProfileService { get { return ServiceProxy.GetService<ProfileService>(); } }

        /// <summary>
        /// Master data management service object
        /// </summary>
        protected MdmService MdmService { get { return ServiceProxy.GetService<MdmService>(); } }

        protected EmployeeProfileService EmployeeService { get { return ServiceProxy.GetService<EmployeeProfileService>(); } }

        public ClaimBenefitService claimBenefitService => ServiceProxy.GetService<ClaimBenefitService>();
        #endregion

        /// <summary>
        /// View user dashboard
        /// </summary>
        /// <returns>User Dashboard Page</returns>
        [Permission(PermissionKey.ViewMyDashboard)]
        public IActionResult MyDashboard()
        {
            return View();
        }

        /// <summary>
        /// View subordinate employee and their profile
        /// </summary>
        /// <returns>Subordinates Page</returns>
        [Permission(PermissionKey.ViewSubordinateEmployeeProfile)]
        public IActionResult Subordinates()
        {
            return View();  
        }

        /// <summary>
        /// Find all employee
        /// </summary>
        /// <returns>Find Page</returns>
        [Permission(PermissionKey.FindEmployee)]
        public IActionResult FindEmployee()
        {
            return View();
        }

        /// <summary>
        /// View user tasks and task histories
        /// </summary>
        /// <returns>Task Page</returns>
        [Permission(PermissionKey.ViewTasks)]
        public IActionResult Tasks()
        {
            return View();
        }

        /// <summary>
        /// View notification page
        /// </summary>
        /// <returns>Notification Page</returns>
        [Permission(PermissionKey.ViewNotifications)]
        public async Task<IActionResult> Notifications()
        {
            await CoreService.ReadNotifications(ServiceProxy.UserClaim.NoReg);

            return View();
        }

        /// <summary>
        /// View notices page
        /// </summary>
        /// <returns>Notices Page</returns>
        [Permission(PermissionKey.ViewNotices)]
        public async Task<IActionResult> Notices()
        {
            await CoreService.ReadNotifications(ServiceProxy.UserClaim.NoReg, "notice");

            return View();
        }

        /// <summary>
        /// View user profile
        /// </summary>
        /// <returns>User Profile</returns>
        [Permission(PermissionKey.ViewProfile)]
        public IActionResult Profile()
        {
            string noreg = ServiceProxy.UserClaim.NoReg;

            //ViewData["ProfileData"] = ProfileService.GetProfile(Noreg);
            //ViewData["PersonalData"] = MdmService.GetEmployeeOrganizationObjects(Noreg);
            //ViewData["Education"] = ProfileService.GetProfileDataEducation(Noreg);
            //ViewData["FamilyMember"] = ProfileService.GetFamilyMembers(Noreg);
            //ViewData["OrganizationalAssignment"] = ProfileService.GetOrganizationalAssignments(Noreg);
            //ViewData["DataTraining"] = ProfileService.GetDataTraining(Noreg);
            //ViewData["TrainingHistory"] = ProfileService.GetHistoryTraining(Noreg);
            //ViewData["HistoryBenefitClaim"] = ProfileService.GetProfileDataClaimBenefit(Noreg);
            ////ViewData["Voucher"] = ProfileService.GetProfileDataVoucher(Noreg);

            //var benefit = ProfileService.GetProfileDataClaimBenefit(Noreg);
            //var claimType = benefit.Select(x => x.AllowanceType).Distinct().ToList();
            //ViewData["claimType"] = claimType;
            //var status = ProfileService.GetStatus(Noreg);
            //ViewData["Status"] = status;

            //var statusContract = ProfileService.getContranct(Noreg);
            //ViewData["Contract"] = statusContract;

            //var photo = GetPhotoUrlByNoReg(Noreg);
            //ViewData["Photo"] = photo;

            var employeeData = EmployeeService.GetEmployeeOrganizationObjects(noreg);
            var subgrupData = EmployeeService.GetPersonalSubArea();
            var statusData = ProfileService.GetStatus(noreg);
            var profileData= ProfileService.GetProfiles(noreg);
            var collageData = EmployeeService.GetCollage();
            var majorsData = EmployeeService.GetMajors();
            var bankSelectedData = EmployeeService.SelectedBank(noreg);
            var educationLevelData = EmployeeService.GetEducationLevel();
            var familyTypeData = EmployeeService.GetFamilyType();
            var taxSelectedData = EmployeeService.SelectedTax(noreg);
            var bankData = claimBenefitService.GetDataBank(noreg, DateTime.Now);
            var personalEducationData = EmployeeService.GetPersonalDataEducation(noreg);
            var emergencyContactData = ProfileService.GetEmergencyContact(noreg);
            var profileEducationData = ProfileService.GetProfileDataEducation(noreg);
            var familyData = ProfileService.GetFamilyMembers(noreg);
            var organizationAssignmentsData= ProfileService.GetOrganizationalAssignments(noreg);
            var historyTrainingData = ProfileService.GetHistoryTraining(noreg);
            var benefitData = ProfileService.GetProfileDataClaimBenefit(noreg);
            var statusContractData = EmployeeService.getContranct(noreg);


            ViewData["NameUser"] = HttpUtility.HtmlEncode(employeeData?.FirstOrDefault()?.Name ?? "");

            ViewData["Kelas"] = HttpUtility.HtmlEncode(employeeData?.FirstOrDefault()?.Kelas ?? "");

            ViewData["subgrup"] = HttpUtility.HtmlEncode(employeeData?.FirstOrDefault()?.EmployeeSubGroupText ?? "");

            ViewData["subgrupList"] = subgrupData;

            ViewData["Expr1"] = statusData ?? "";

            ViewData["Nationality"] = HttpUtility.HtmlEncode(profileData?.Nationality ?? "");


            var datebirth = profileData?.BirthDate;

            if (datebirth.HasValue)
            {
                HttpUtility.HtmlEncode(ViewData["dob"] = datebirth.Value.ToString("yyyy-MM-dd"));

            }
            else
            {
                HttpUtility.HtmlEncode(ViewData["dob"] = "");
            }

            ViewData["gender"] = HttpUtility.HtmlEncode(profileData?.Gender ?? "");

            //ViewData["address"] = profileData?.Address ?? "";
            //Coba KTP Address Fatih 17 june 2025

            ViewData["adrProvince"] = profileData?.Region ?? "";
            ViewData["adrDistrict"] = profileData?.District ?? "";
            ViewData["adrSubDistrict"] = profileData?.SubDistrict ?? "";
            ViewData["addressLine"] = profileData?.Address ?? "";
            ViewData["adrAdmVillage"] = profileData?.AdministrativeVillage ?? "";
            ViewData["adrRw"] = profileData?.Rw ?? "";
            ViewData["adrRt"] = profileData?.Rt ?? "";
            ViewData["adrPostalCode"] = profileData?.PostalCode ?? "";

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

            ViewData["Addresss"] = fullAddress;
            //Coba KTP Address Fatih

            // Coba Domicile Address Fatih 17 June 2025
            ViewData["domicileProvince"] = profileData?.DomicileRegion ?? "";
            ViewData["domicileDistrict"] = profileData?.DomicileDistrict ?? "";
            ViewData["domicileSubDistrict"] = profileData?.DomicileSubDistrict ?? "";
            ViewData["domicileAddress"] = profileData?.Domicile ?? "";
            ViewData["domicileAdmVillage"] = profileData?.DomicileAdministrativeVillage ?? "";
            ViewData["domicileRw"] = profileData?.DomicileRw ?? "";
            ViewData["domicileRt"] = profileData?.DomicileRt ?? "";
            ViewData["domicilePostalCode"] = profileData?.DomicilePostalCode ?? "";
            ViewData["domicileResidential"] = profileData?.DomicileResidentialStatus ?? "";
            ViewData["domicileTotalFamily"] = profileData?.DomicileTotalFamily ?? "";

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

            ViewData["Domicile"] = domicileFullAddress;
            // Coba Domicile Address Fatih

            ViewData["marCerti"] = HttpUtility.HtmlEncode(profileData?.MarriedCertificate ?? "");

            ViewData["divCerti"] = HttpUtility.HtmlEncode(profileData?.DivorceCertificate ?? "");

            ViewData["marDate"] = HttpUtility.HtmlEncode(profileData?.MarriedDate?.ToString("yyyy-MM-dd") ?? "");

            ViewData["divDate"] = HttpUtility.HtmlEncode(profileData?.DivorceDate?.ToString("yyyy-MM-dd") ?? "");

            //ViewData["domicile"] = profileData?.Domicile ?? "";

            ViewData["email"] = HttpUtility.HtmlEncode(profileData?.Email ?? "");

            ViewData["MartalStatus"] = profileData.MartalStatus ?? "";

            ViewData["religion"] = HttpUtility.HtmlEncode(profileData?.Religion ?? "");

            ViewData["bg"] = HttpUtility.HtmlEncode(profileData?.BloodType ?? "");

            ViewData["np"] = HttpUtility.HtmlEncode(profileData?.DanaPensiunAstra ?? "");

            ViewData["collageList"] = collageData;

            ViewData["bankSelect"] = bankSelectedData;

            ViewData["eduLevelList"] = educationLevelData;

            ViewData["familyTypeList"] = familyTypeData;

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

            ViewData["majorList"] = majorsData;

            ViewData["es"] = statusData ?? "";

            ViewData["pob"] = HttpUtility.HtmlEncode(profileData?.BirthPlace ?? "");

            ViewData["majorLister"] = majorsData.Select(p => p.ToLower()); // Convert each value to lowercase

            ViewData["idNumber"] = HttpUtility.HtmlEncode(profileData?.Nik ?? "");

            ViewData["kk"] = HttpUtility.HtmlEncode(profileData?.KKNumber ?? "");

            ViewData["npwp"] = HttpUtility.HtmlEncode(profileData?.Npwp ?? "");

            ViewData["taxStatus"] = HttpUtility.HtmlEncode(profileData?.TaxStatus ?? "");

            ViewData["taxSelect"] = taxSelectedData;

            ViewData["accountNumber"] = HttpUtility.HtmlEncode(profileData?.AccountNumber ?? "");
            ViewData["accountName"] = HttpUtility.HtmlEncode(profileData?.AccountName ?? "");

            ViewData["cbf"] = bankData?.FirstOrDefault()?.Branch ?? "";

            ViewData["bpjs"] = HttpUtility.HtmlEncode(profileData?.BpjsNumber ?? "");

            ViewData["bpjsen"] = HttpUtility.HtmlEncode(profileData?.BpjsKetenagakerjaan ?? "");

            ViewData["insuranceNo"] = HttpUtility.HtmlEncode(profileData?.InsuranceNumber ?? "");

            ViewData["simaNumber"] = HttpUtility.HtmlEncode(profileData?.SimANumber ?? "");

            ViewData["simcNumber"] = HttpUtility.HtmlEncode(profileData?.SimCNumber ?? "");

            ViewData["passportNumber"] = HttpUtility.HtmlEncode(profileData?.PassportNumber ?? "");

            ViewData["identityName"] = HttpUtility.HtmlEncode(profileData?.IdentityCardName ?? "");

            ViewData["phoneNum"] = HttpUtility.HtmlEncode(profileData?.PhoneNumber ?? "");

            ViewData["waNum"] = HttpUtility.HtmlEncode(profileData?.WhatsappNumber ?? "");

            ViewData["personalemail"] = HttpUtility.HtmlEncode(profileData?.PersonalEmail ?? "");

            ViewData["EducationData"] = personalEducationData;


            ViewData["emcContact"] = emergencyContactData;
            ViewData["ecName"] = HttpUtility.HtmlEncode(emergencyContactData.EmergencyCallName);
            ViewData["ecPhone"] = HttpUtility.HtmlEncode(emergencyContactData.EmergencyCallPhoneNumber);
            ViewData["ecRelation"] = HttpUtility.HtmlEncode(emergencyContactData.EmergencyCallRelationshipCode);
            ViewData["ecName2"] = HttpUtility.HtmlEncode(emergencyContactData.EmergencyCallName2);
            ViewData["ecPhone2"] = HttpUtility.HtmlEncode(emergencyContactData.EmergencyCallPhoneNumber2);
            ViewData["ecRelation2"] = HttpUtility.HtmlEncode(emergencyContactData.EmergencyCallRelationshipCode2);

            ViewData["ProfileDatas"] = profileData;
            //var data = MdmService.GetEmployeeOrganizationObjects(noreg);

            ViewData["PersonalData"] = employeeData;
            ViewData["Education"] = profileEducationData;
            ViewData["FamilyMember"] = familyData;
            ViewData["OrganizationalAssignment"] = organizationAssignmentsData;
            ViewData["DataTraining"] = historyTrainingData;

            var claimType = benefitData.Select(x => x.AllowanceType).Distinct();
            ViewData["claimType"] = claimType;
            ViewData["HistoryBenefitClaim"] = benefitData;

            ViewData["Contract"] = statusContractData;

            var photo = GetPhotoUrlByNoReg(noreg);
            ViewData["Photo"] = photo;

            return View();
        }

        public IActionResult ProfileEdit()
        {
            string Noreg = ServiceProxy.UserClaim.NoReg;

            var benefitData = ProfileService.GetProfileDataClaimBenefit(Noreg);

            ViewData["ProfileData"] = ProfileService.GetProfile(Noreg);
            ViewData["PersonalData"] = MdmService.GetEmployeeOrganizationObjects(Noreg);
            ViewData["Education"] = ProfileService.GetProfileDataEducation(Noreg);
            ViewData["FamilyMember"] = ProfileService.GetFamilyMembers(Noreg);
            ViewData["OrganizationalAssignment"] = ProfileService.GetOrganizationalAssignments(Noreg);
            ViewData["DataTraining"] = ProfileService.GetDataTraining(Noreg);
            ViewData["TrainingHistory"] = ProfileService.GetHistoryTraining(Noreg);
            ViewData["HistoryBenefitClaim"] = benefitData;
            //ViewData["Voucher"] = ProfileService.GetProfileDataVoucher(Noreg);

            var claimType = benefitData.Select(x => x.AllowanceType).Distinct();
            ViewData["claimType"] = claimType;
            var status = ProfileService.GetStatus(Noreg);
            ViewData["Status"] = status;

            var statusContract = ProfileService.getContranct(Noreg);
            ViewData["Contract"] = statusContract;

            var photo = GetPhotoUrlByNoReg(Noreg);
            ViewData["Photo"] = photo;

            return View();
        }

        public string GetPhotoUrlByNoReg(string noReg)
        {
            // Sesuaikan dengan path folder penyimpanan foto
            var noreg = User.GetClaim("NoReg");
            PathProvider pathProvider = (PathProvider)HttpContext.RequestServices.GetService(typeof(PathProvider));
            //var filesPath = pathProvider.ContentPath("uploads/avatar");

            var configPath = ConfigService.GetConfig("Photo.Path", true)?.ConfigValue;

            var filesPath = !string.IsNullOrEmpty(configPath) ? configPath : pathProvider.ContentPath("uploads/avatar");

            // Format nama file
            var fileName = noreg + ".jpg"; // Foto disimpan dengan format NoReg.jpg
            var fullFilePath = Path.Combine(filesPath, fileName);

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
                string result = customUrl.Replace("\\", "/");

                return result;
            }
            else
            {
                // Jika folder "avatar" tidak ditemukan, kembalikan input asli
                return localPath;
            }
        }

        /// <summary>
        /// View user profile
        /// </summary>
        /// <returns>User Profile</returns>
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

        /// <summary>
        /// View document change trackers
        /// </summary>
        /// <returns>Change Tracker View Component</returns>
        [Permission(PermissionKey.FindEmployee)]
        public IActionResult ChangeTracker(Guid id)
        {
            return ViewComponent(typeof(ChangeTracker), new { id });
        }

        /// <summary>
        /// View reporting structure
        /// </summary>
        /// <returns>Reporting Structure View Component</returns>
        [Permission(PermissionKey.ViewReportingStructure)]
        public IActionResult ReportingStructure(string noreg, string postCode)
        {
            return ViewComponent(typeof(ReportingStructure), new { noreg, postCode });
        }

        /// <summary>
        /// View application guidelines
        /// </summary>
        /// <returns>Application Guidelines View Component</returns>
        [Permission(PermissionKey.ListGuideline)]
        public IActionResult Guideline()
        {
            return ViewComponent(typeof(Guideline));
        }

        /// <summary>
        /// View current leave
        /// </summary>
        /// <returns>Leave View Component</returns>
        public IActionResult Leave(string type)
        {
            var noreg = ServiceProxy.UserClaim.NoReg;

            return ViewComponent(typeof(Leave), new { noreg, type });
        }

        /// <summary>
        /// View events calendar
        /// </summary>
        /// <returns>Events Calendar View Component</returns>
        [Permission(PermissionKey.ListEventsCalendar)]
        public IActionResult Calendar()
        {
            var noreg = ServiceProxy.UserClaim.NoReg;
            var period = DateTime.Now.Year;

            return ViewComponent(typeof(Calendar), new { noreg, period });
        }

        /// <summary>
        /// View events calendar
        /// </summary>
        /// <returns>Events Calendar View Component</returns>
        [Permission(PermissionKey.ListEventsCalendar)]
        public IActionResult EventsCalendar(int period)
        {
            return ViewComponent(typeof(Calendar), new { period, editMode = true });
        }

        /// <summary>
        /// View events calendar by work schedule rule
        /// </summary>
        /// <param name="period">Period</param>
        /// <param name="workScheduleRule">Work Schedule Rule</param>
        /// <returns>Events Calendar View Component</returns>
        [Permission(PermissionKey.ViewWorkSchedule)]
        public IActionResult WorkSchedule(int period, string workScheduleRule)
        {
            return ViewComponent(typeof(Calendar), new { period, workScheduleRule });
        }

        /// <summary>
        /// View generic data uploader
        /// </summary>
        /// <returns>Generic Data Uploader View Component</returns>
        public IActionResult GenericDataUploader(string url)
        {
            return ViewComponent(typeof(GenericDataUploader), new { url });
        }

        /// <summary>
        /// View user impersonation
        /// </summary>
        /// <returns>User Impersonation View Component</returns>
        public IActionResult Impersonator()
        {
            return ViewComponent(typeof(Impersonator), new { noreg = ServiceProxy.UserClaim.NoReg });
        }

        /// <summary>
        /// View concurrent position
        /// </summary>
        /// <returns>Concurrent Position View Component</returns>
        public IActionResult ConcurrentPosition()
        {
            return ViewComponent(typeof(ConcurrentPosition), new { noreg = ServiceProxy.UserClaim.NoReg, currentPostCode = ServiceProxy.UserClaim.PostCode });
        }

        /// <summary>
        /// View document tracking approval
        /// </summary>
        /// <param name="id">Document Approval Id</param>
        /// <returns>Document Tracking Approval View Component</returns>
        public IActionResult TrackingApproval(Guid id)
        {
            return ViewComponent(typeof(TrackingApproval), new { id });
        }

        /// <summary>
        /// View document approval histories
        /// </summary>
        /// <param name="id">Document Approval Id</param>
        /// <returns>Document Approval Histories View Component</returns>
        public IActionResult DocumentApprovalHistories(Guid id)
        {
            return ViewComponent(typeof(DocumentApprovalHistory), new { id });
        }

        /// <summary>
        /// View fixed menu
        /// </summary>
        /// <returns>Fixed Menu View Component</returns>
        public IActionResult FixedMenu()
        {
            var noreg = ServiceProxy.UserClaim.NoReg;
            var roles = ServiceProxy.UserClaim.Roles;

            return ViewComponent(typeof(FixedMenu), new { noreg, roles });
        }
    }
}