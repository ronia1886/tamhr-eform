using System;
using System.Collections.Generic;
using System.Diagnostics;
using Agit.Common.Attributes;

namespace TAMHR.ESS.Infrastructure
{
    public enum ActionType
    {
        Create,
        Update,
        Delete
    }

    #region Mail Status Constants
    /// <summary>
    /// Constanta for mail status.
    /// </summary>
    public static class MailStatus
    {
        /// <summary>
        /// Pending mail status constant.
        /// </summary>
        public const string Pending = "pending";

        /// <summary>
        /// Sent mail status constant.
        /// </summary>
        public const string Sent = "sent";

        /// <summary>
        /// Sent mail status constant.
        /// </summary>
        public const string Failed = "failed";
    }
    #endregion

    #region Built in Roles Constants
    /// <summary>
    /// Constanta for built in roles.
    /// </summary>
    public static class BuiltInRoles
    {
        /// <summary>
        /// Default role constant.
        /// </summary>
        public const string Default = "DEFAULT_USER";

        /// <summary>
        /// External user role constant.
        /// </summary>
        public const string ExternalUser = "EXTERNAL_USER";

        /// <summary>
        /// Expat user role constant.
        /// </summary>
        public const string ExpatUser = "EXPAT_USER";

        /// <summary>
        /// Administrator role constant.
        /// </summary>
        public const string Administrator = "HR_ADMIN";

        /// <summary>
        /// Chief role constant.
        /// </summary>
        public const string Chief = "CHIEF_USER";

        /// <summary>
        /// Expat chief role constant.
        /// </summary>
        public const string ExpatChief = "EXPAT_CHIEF_USER";

        /// <summary>
        /// SHE role constant.
        /// </summary>
        public const string She = "SHE";
    }
    #endregion

    #region Mail Template Constants
    /// <summary>
    /// Constanta for mail template.
    /// </summary>
    public static class MailTemplate
    {
        /// <summary>
        /// Pending tasks mail template key constant.
        /// </summary>
        public const string PendingTasks = "pending-tasks";

        /// <summary>
        /// Weekly WFH Planning reminder mail template key constant.
        /// </summary>
        public const string WeeklyWfhPlanning = "weekly-wfh-planning-reminder";

        /// <summary>
        /// Weekly WFH Planning reminder mail template key constant.
        /// </summary>
        public const string WeeklyWfhPlanningSummary = "weekly-wfh-planning-reminder-summary";

        /// <summary>
        /// Acknowledge Superior Hybrid Work Schedule mail template key constant.
        /// </summary>
        public const string AcknowledgeSuperiorHybrid = "acknowledge-superior-hybrid-work-schedule";

        /// <summary>
        /// Hybrid Work Planning User Submission mail template key constant.
        /// </summary>
        public const string HybridWorkPlanningUserSubmission = "hybrid-work-planning-user-submission";

        /// <summary>
        /// BPKB expired mail template key constant.
        /// </summary>
        public const string BpkbExpired = "bpkb-expired";

        /// <summary>
        /// Get BPKB mail template key constant.
        /// </summary>
        public const string GetBpkb = "get-bpkb";

        /// <summary>
        /// SHE update member mail template key constant.
        /// </summary>
        public const string SheUpdateMember = "she-member-update";
    }
    #endregion

    #region Approval Action Constants
    /// <summary>
    /// Constanta for approval action.
    /// </summary>
    public static class ApprovalAction
    {
        /// <summary>
        /// Initiate approval action code.
        /// </summary>
        public const string Initiate = "initiate";

        /// <summary>
        /// Approve action code.
        /// </summary>
        public const string Approve = "approve";

        /// <summary>
        /// Reject action code.
        /// </summary>
        public const string Reject = "reject";

        /// <summary>
        /// Revise action code.
        /// </summary>
        public const string Revise = "revise";

        /// <summary>
        /// Cancel or drop request action code.
        /// </summary>
        public const string Cancel = "cancel";

        /// <summary>
        /// Complete action code.
        /// </summary>
        public const string Complete = "complete";

        /// <summary>
        /// Acknowledge action code.
        /// </summary>
        public const string Acknowledge = "acknowledge";

        /// <summary>
        /// Approval document tracking status css classes.
        /// </summary>
        public static readonly Dictionary<string, string> TrackingCssClasses = new Dictionary<string, string>
        {
            // Status when initiated.
            [Initiate] = "timeline-success",
            // Status when approved.
            [Approve] = "timeline-success",
            // Status when rejected.
            [Reject] = "timeline-danger",
            // Status when revised.
            [Revise] = "timeline-success",
            // Status when cancelled or dropped.
            [Cancel] = "timeline-danger",
            // Status when completed.
            [Complete] = "timeline-success",
            // Status when acknowledged.
            [Acknowledge] = "timeline-success"
        };

        /// <summary>
        /// Approval action css classes.
        /// </summary>
        public static readonly Dictionary<string, string> ApprovalCssClasses = new Dictionary<string, string>
        {
            // Status when initated.
            [Initiate] = "bg-blue-sharp",
            // Status when approved.
            [Approve] = "bg-green-haze",
            // Status when rejected.
            [Reject] = "bg-red-haze",
            // Status when revised.
            [Revise] = "bg-yellow-crusta",
            // Status when cancelled.
            [Cancel] = "bg-red-haze",
            // Status when completed.
            [Complete] = "bg-purple",
            // Status when acknowledged.
            [Acknowledge] = "bg-danger"
        };
    }
    #endregion

    #region Document Status Constants
    /// <summary>
    /// Constanta for document status.
    /// </summary>
    public static class DocumentStatus
    {
        /// <summary>
        /// Document draft status code.
        /// </summary>
        public const string Draft = "draft";

        /// <summary>
        /// Document expired status code.
        /// </summary>
        public const string Expired = "expired";
        
        /// <summary>
        /// Document in-progress status code.
        /// </summary>
        public const string InProgress = "inprogress";

        /// <summary>
        /// Document completed status code.
        /// </summary>
        public const string Completed = "completed";

        /// <summary>
        /// Document rejected status code.
        /// </summary>
        public const string Rejected = "rejected";

        /// <summary>
        /// Document revised status code.
        /// </summary>
        public const string Revised = "revised";
        
        /// <summary>
        /// Document cancelled status code.
        /// </summary>
        public const string Cancelled = "cancelled";

        /// <summary>
        /// Document locked status code.
        /// </summary>
        public const string Locked = "locked";

        /// <summary>
        /// Document status css classes.
        /// </summary>
        public static readonly Dictionary<string, string> CssClasses = new Dictionary<string, string>
        {
            // Draft document.
            [Draft] = "bg-none",
            // Expired document.
            [Expired] = "bg-dark",
            // In-progress document.
            [InProgress] = "bg-warning",
            // Completed document.
            [Completed] = "bg-success",
            // Rejected document.
            [Rejected] = "bg-danger",
            // Revised document.
            [Revised] = "bg-none",
            // Cancelled document.
            [Cancelled] = "bg-danger"
        };
    }
    #endregion

    #region Application Form Constants
    /// <summary>
    /// Constanta for application form.
    /// </summary>
    public static class ApplicationForm
    {
        #region Personal Data Forms
        /// <summary>
        /// Personal data confirmation form key constant.
        /// </summary>
        public const string PersonalDataConfirmation = "personal-confirmation";

        /// <summary>
        /// Others personal data form key constant.
        /// </summary>
        public const string OthersPersonalData = "others-personal";

        /// <summary>
        /// Address form key constant.
        /// </summary>
        public const string Address = "address";

        /// <summary>
        /// Education form key constant.
        /// </summary>
        public const string Education = "education";

        /// <summary>
        /// Tax status form key constant.
        /// </summary>
        public const string TaxStatus = "tax-status";

        /// <summary>
        /// Bank account form key constant.
        /// </summary>
        public const string BankAccount = "bank-account";

        /// <summary>
        /// Divorce form key constant.
        /// </summary>
        public const string Divorce = "divorce";

        /// <summary>
        /// Condolance form key constant.
        /// </summary>
        public const string Condolance = "condolance";

        /// <summary>
        /// Family registration form key constant.
        /// </summary>
        public const string FamilyRegistration = "family-registration";

        /// <summary>
        /// Marriage status form key constant.
        /// </summary>
        public const string MarriageStatus = "marriage-status";

        /// <summary>
        /// Others contact hobbies form key constant.
        /// </summary>
        public const string ContactHobbies = "contact-hobbies";

        /// <summary>
        /// Others address pengkinian data form key constant.
        /// </summary>
        public const string AddressNew = "address-new";

        /// <summary>
        /// Others pengkinian data form key constant.
        /// </summary>
        public const string PengkinianData = "pengkinian-data";

        /// <summary>
        /// Driver License form key constant.
        /// </summary>
        public const string DriversLicense = "drivers-license";

        /// <summary>
        /// Driver License form key constant.
        /// </summary>
        public const string Passport = "passport";

        /// <summary>
        /// Driver License form key constant.
        /// </summary>
        public const string FamilyRegist = "family-regist";
        #endregion

        #region Time Management Form
        /// <summary>
        /// Absence form key constant.
        /// </summary>
        public const string Absence = "absence";

        /// <summary>
        /// BDJK planning form key constant.
        /// </summary>
        public const string BdjkPlanning = "bdjk-planning";

        /// <summary>
        /// Maternity leave form key constant.
        /// </summary>
        public const string MaternityLeave = "maternity-leave";

        /// <summary>
        /// Shift planning form key constant.
        /// </summary>
        public const string ShiftPlanning = "shift-planning";

        /// <summary>
        /// SPKL overtime form key constant.
        /// </summary>
        public const string SpklOvertime = "spkl-overtime";

        /// <summary>
        /// BDJK report form key constant.
        /// </summary>
        public const string BdjkReport = "bdjk-report";

        /// <summary>
        /// SPKL report form key constant.
        /// </summary>
        public const string SpklReport = "spkl-report";

        /// <summary>
        /// Shift report form key constant.
        /// </summary>
        public const string ShiftReport = "shift-report";

        /// <summary>
        /// Proxy time form key constant.
        /// </summary>
        public const string ProxyTimeForm = "proxy-time-form";

        /// <summary>
        /// Annual Leave Planning form.
        /// </summary>
        public const string AnnualLeavePlanning = "annual-leave-planning";

        /// <summary>
        /// Annual WFH/WFO Planning form.
        /// </summary>
        public const string AnnualWFHPlanning = "annual-wfh-planning";

        /// <summary>
        /// Annual OT Planning form.
        /// </summary>
        public const string AnnualOTPlanning = "annual-ot-planning";

        /// <summary>
        /// Annual BDJK Planning form.
        /// </summary>
        public const string AnnualBDJKPlanning = "annual-bdjk-planning";

        /// <summary>
        /// Abnormality Absence form.
        /// </summary>
        public const string AbnormalityAbsence = "abnormality-absence";

        /// <summary>
        /// Abnormality Over Time form.
        /// </summary>
        public const string AbnormalityOverTime = "abnormality-over-time";

        /// <summary>
        /// Abnormality Over Time form.
        /// </summary>
        public const string AbnormalityBdjk = "abnormality-bdjk";

        /// <summary>
        /// Weekly WFH Planning form.
        /// </summary>
        public const string WeeklyWFHPlanning = "weekly-wfh-planning";
        #endregion

        #region Others Forms
        /// <summary>
        /// Reference letter form key constant.
        /// </summary>
        public const string ReferenceLetter = "reference-letter";

        /// <summary>
        /// Lost and return form key constant.
        /// </summary>
        public const string LostAndReturn = "lost-and-return";

        /// <summary>
        /// Data request form key constant.
        /// </summary>
        public const string DataRequest = "data-request";

        /// <summary>
        /// Notebook request form key constant.
        /// </summary>
        public const string NotebookRequest = "notebook-request";

        /// <summary>
        /// KAI loan form key constant.
        /// </summary>
        public const string KaiLoan = "kai-loan";

        /// <summary>
        /// Complaint request form key constant.
        /// </summary>
        public const string ComplaintRequest = "complaint-request";

        /// <summary>
        /// Health declaration form key constant.
        /// </summary>
        public const string HealthDeclaration = "health-declaration";

        /// <summary>
        /// Vaccine form key constant.
        /// </summary>
        public const string Vaccine = "vaccine";

        /// <summary>
        /// Termination form key constant.
        /// </summary>
        public const string Termination = "termination";
        #endregion

        #region Claim & Benefit Forms
        /// <summary>
        /// Ayo sekolah form key constant.
        /// </summary>
        public const string AyoSekolah = "ayo-sekolah";

        /// <summary>
        /// Reimbursement form key constant.
        /// </summary>
        public const string Reimbursement = "reimbursement";

        /// <summary>
        /// PTA allowance form key constant.
        /// </summary>
        public const string PtaAllowance = "pta-allowance";

        /// <summary>
        /// Vacation allowance form key constant.
        /// </summary>
        public const string VacationAllowance = "vacation-allowance";

        /// <summary>
        /// Meal allowance form key constant.
        /// </summary>
        public const string MealAllowance = "meal-allowance";

        /// <summary>
        /// Distressed form key constant.
        /// </summary>
        public const string DistressedAllowance = "distressed-allowance";

        /// <summary>
        /// COP fuel allowance form key constant.
        /// </summary>
        public const string CopFuelAllowance = "cop-fuel-allowance";

        /// <summary>
        /// Get BPKB COP form key constant.
        /// </summary>
        public const string GetBpkbCop = "get-bpkb-cop";

        /// <summary>
        /// KB allowance form key constant.
        /// </summary>
        public const string KbAllowance = "kb-allowance";

        /// <summary>
        /// Marriage allowance form key constant.
        /// </summary>
        public const string MarriageAllowance = "marriage-allowance";

        /// <summary>
        /// COP form key constant.
        /// </summary>
        public const string Cop = "cop";

        /// <summary>
        /// CPP form key constant.
        /// </summary>
        public const string Cpp = "cpp";

        /// <summary>
        /// SCP form key constant.
        /// </summary>
        public const string Scp = "scp";

        /// <summary>
        /// Eyeglasses allowance form key constant.
        /// </summary>
        public const string EyeglassesAllowance = "eyeglasses-allowance";

        /// <summary>
        /// Return BPKB COP form key constant.
        /// </summary>
        public const string ReturnBpkbCop = "return-bpkb-cop";

        /// <summary>
        /// Company loan form key constant.
        /// </summary>
        public const string CompanyLoan = "company-loan";

        /// <summary>
        /// Company loan 3-6 form key constant.
        /// </summary>
        public const string CompanyLoan36 = "company-loan36-new";

        /// <summary>
        /// Shift meal allowance form key constant.
        /// </summary>
        public const string ShiftMealAllowance = "shift-meal-allowance";

        /// <summary>
        /// Letter of guarantee form key constant.
        /// </summary>
        public const string LetterOfGuarantee = "letter-of-guarantee";

        /// <summary>
        /// Concept idea allowance form key constant.
        /// </summary>
        public const string ConceptIdeaAllowance = "concept-idea-allowance";

        /// <summary>
        /// DPA register form key constant.
        /// </summary>
        public const string DpaRegister = "dpa-register";

        /// <summary>
        /// DPA change form key constant.
        /// </summary>
        public const string DpaChange = "dpa-change";

        /// <summary>
        /// Condolance allowance form key constant.
        /// </summary>
        public const string CondolanceAllowance = "condolance-allowance";
        #endregion
    }
    #endregion

    #region User Hash Constants
    /// <summary>
    /// Constanta for user hash.
    /// </summary>
    public static class HashType
    {
        /// <summary>
        /// Payslip hash constant.
        /// </summary>
        public const string Payslip = "hash-payslip";

        /// <summary>
        /// SPT hash constant.
        /// </summary>
        public const string Spt = "hash-spt";


        /// <summary>
        /// BUPOT hash constant.
        /// </summary>
        public const string Bupot = "hash-bupot";
    }
    #endregion

    #region User Activity Log Constants
    /// <summary>
    /// Constanta for user activity log.
    /// </summary>
    public static class ActivityLog
    {
        /// <summary>
        /// Payslip log constant.
        /// </summary>
        public const string Payslip = "log-payslip";

        /// <summary>
        /// SPT log constant.
        /// </summary>
        public const string Spt = "log-spt";

        /// <summary>
        /// BUPOT log constant.
        /// </summary>
        public const string Bupot = "log-bupot";
    }
    #endregion

    #region Application Module Constants
    /// <summary>
    /// Constanta for application module.
    /// </summary>
    public static class ApplicationModule
    {
        /// <summary>
        /// Core module constant.
        /// </summary>
        public const string Core = "Core";

        /// <summary>
        /// Claim & benefit module constant.
        /// </summary>
        public const string ClaimBenefit = "ClaimBenefit";

        /// <summary>
        /// Others module constant.
        /// </summary>
        public const string Others = "Others";

        /// <summary>
        /// Personal data module constant.
        /// </summary>
        public const string PersonalData = "PersonalData";

        /// <summary>
        /// Time management module constant.
        /// </summary>
        public const string TimeManagement = "TimeManagement";


        /// <summary>
        /// Personal data module constant.
        /// </summary>
        public const string OHS = "OHS";
    }
    #endregion

    public static class Configurations
    {
        #region Payslip Configurations
        public const string PayslipDefaultPassword = "Payslip.DefaultPassword";
        public const string PayslipPath = "Payslip.Path";
        public const string PayslipUseFtp = "Payslip.UseFtp";
        public const string PayslipPasswordFile = "Payslip.PasswordFile";
        public const string PayslipOwnerPassword = "Payslip.OwnerPassword";
        public const string PayslipOffCycleFilename = "Payslip.OffCycleFilename";
        public const string PayslipFilename = "Payslip.Filename";
        public const string PayslipEnablePrinting = "Payslip.EnablePrinting";
        public const string PayslipEnableEditing = "Payslip.EnableEditing";
        public const string PayslipFtpEnableSsl = "Payslip.FtpEnableSsl";
        public const string PayslipFtpUsername = "Payslip.FtpUsername";
        public const string PayslipFtpPassword = "Payslip.FtpPassword";
        public const string ApplicationUrl = "Application.Url";
        #endregion

        #region SPT Configuration
        public const string SptDefaultPassword = "Payslip.DefaultPassword";
        //public const string SptDefaultPassword = "Spt.DefaultPassword";
        public const string SptPath = "Spt.Path";
        public const string SptUseFtp = "Spt.UseFtp";
        public const string SptPasswordFile = "Payslip.PasswordFile";
        public const string SptOwnerPassword = "Payslip.OwnerPassword";
        //public const string SptPasswordFile = "Spt.PasswordFile";
        //public const string SptOwnerPassword = "Spt.OwnerPassword";
        public const string SptFilename = "Spt.Filename";
        public const string SptEnablePrinting = "Spt.EnablePrinting";
        public const string SptEnableEditing = "Spt.EnableEditing";
        public const string SptFtpEnableSsl = "Spt.FtpEnableSsl";
        public const string SptFtpUsername = "Spt.FtpUsername";
        public const string SptFtpPassword = "Spt.FtpPassword";
        public const string SptFtpPort = "Spt.FtpPort";
        #endregion


        #region Bupot Configurations
        public const string BupotDefaultPassword = "Payslip.DefaultPassword";
        public const string BupotPath = "BUPOT.Path";
        public const string BupotUseFtp = "BUPOT.UseFtp";
        public const string BupotPasswordFile = "Payslip.PasswordFile";
        public const string BupotOwnerPassword = "Payslip.OwnerPassword";
        public const string BupotOffCycleFilename = "BUPOT.OffCycleFilename";
        public const string BupotFilename = "BUPOT.Filename";
        public const string BupotEnablePrinting = "BUPOT.EnablePrinting";
        public const string BupotEnableEditing = "BUPOT.EnableEditing";
        public const string BupotFtpEnableSsl = "BUPOT.FtpEnableSsl";
        public const string BupotFtpUsername = "BUPOT.FtpUsername";
        public const string BupotFtpPassword = "BUPOT.FtpPassword";
        public const string BupotLocalPath = "BUPOT.LocalPath";
        #endregion

        #region Bupot Configurations
        public const string EmailPassword = "Email.Password";
        public const string DigitalAttendanceKey = "DigitalAttendanceKey";
        #endregion
    }

    #region General Application Constants
    /// <summary>
    /// General constanta class for this application.
    /// </summary>
    public static partial class ApplicationConstants
    {
        /// <summary>
        /// Maximum length for code value input.
        /// </summary>
        public const int SmallInputMaxLength = 20;

        /// <summary>
        /// Maximum length for short title value input.
        /// </summary>
        public const int MediumInputMaxLength = 50;

        /// <summary>
        /// Maximum length for title value input.
        /// </summary>
        public const int MediumLongInputMaxLength = 150;

        /// <summary>
        /// Maximum length for long title value input.
        /// </summary>
        public const int CommonMediumInputMaxLength = 250;

        /// <summary>
        /// Maximum length for file path value input.
        /// </summary>
        public const int CommonInputMaxLength = 500;

        /// <summary>
        /// Maximum length for remarks value input.
        /// </summary>
        public const int RemarksMaxLength = 1000;

        /// <summary>
        /// Maximum length for medium title value input.
        /// </summary>
        public const int SmallVarcharMaxLength = 100;

        /// <summary>
        /// Maximum length for long remarks value input.
        /// </summary>
        public const int VarcharMaxLength = 4000;

        /// <summary>
        /// Default culture key.
        /// </summary>
        public const string IndoCultureInfo = "id-ID";

        /// <summary>
        /// Permission key enum.
        /// </summary>
        public enum PermissionKey
        {
            [StringValue("Spt.View")]
            ViewSpt,
            [StringValue("Payslip.View")]
            ViewPayslip,
            [StringValue("Bupot.View")]
            ViewBupot,
            [StringValue("Bupot.Download")]
            DownloadBupot,
            [StringValue("Payslip.Download")]
            DownloadPayslip,
            [StringValue("Core.ViewReportingStructure")]
            ViewReportingStructure,
            [StringValue("Core.ViewMyDashboard")]
            ViewMyDashboard,
            [StringValue("Core.FindEmployee")]
            FindEmployee,
            [StringValue("Core.ViewOnlineLetter")]
            ViewOnlineLetter,
            [StringValue("Core.ManageOnlineLetter")]
            ManageOnlineLetter,
            [StringValue("Core.ViewArsMatrix")]
            ViewArsMatrix,
            [StringValue("Core.ViewOrganization")]
            ViewOrganization,
            [StringValue("Core.ViewAllEmployeeProfile")]
            ViewAllEmployeeProfile,
            [StringValue("Core.ViewSubordinateEmployeeProfile")]
            ViewSubordinateEmployeeProfile,
            [StringValue("Core.ViewTasks")]
            ViewTasks,
            [StringValue("Core.ViewNotifications")]
            ViewNotifications,
            [StringValue("Core.ViewNotices")]
            ViewNotices,
            [StringValue("Core.ApplicationLog.View")]
            ViewApplicationLog,
            [StringValue("Core.SyncLog.View")]
            ViewSyncLog,
            [StringValue("Core.SapIntegration.View")]
            ViewSapIntegration,
            [StringValue("Reporting.TimeEvaluation.Running")]
            RunningTimeEvaluation,
            [StringValue("Reporting.TimeEvaluation.View")]
            ViewTimeEvaluation,
            [StringValue("Reporting.ManagerDashboard.View")]
            ViewManagerDashboard,
            [StringValue("Reporting.MailQueue.View")]
            ViewMailQueue,
            [StringValue("Reporting.SyncLog.View")]
            ViewSyncLogReport,
            [StringValue("Reporting.DocumentApprovalStatus.View")]
            ViewDocumentApprovalReport,
            [StringValue("Core.Menu.View")]
            ViewMenu,
            [StringValue("Core.Menu.Manage")]
            ManageMenu,
            [StringValue("Core.Permission.View")]
            ViewPermission,
            [StringValue("Core.Permission.Manage")]
            ManagePermission,
            [StringValue("Core.ProxyLog.View")]
            ViewProxyLog,
            [StringValue("Core.ProxyLog.Manage")]
            ManageProxyLog,
            [StringValue("Core.User.View")]
            ViewUser,
            [StringValue("Core.User.Manage")]
            ManageUser,
            [StringValue("Core.Role.View")]
            ViewRole,
            [StringValue("Core.Role.Manage")]
            ManageRole,
            [StringValue("Core.EmailTemplate.View")]
            ViewEmailTemplate,
            [StringValue("Core.EmailTemplate.Manage")]
            ManageEmailTemplate,
            [StringValue("Core.UserImpersonation.View")]
            ViewUserImpersonation,
            [StringValue("Core.UserImpersonation.Manage")]
            ManageUserImpersonation,
            [StringValue("Core.News.View")]
            ViewNews,
            [StringValue("Core.News.Manage")]
            ManageNews,
            [StringValue("Core.News.Detail")]
            ViewNewsDetail,
            [StringValue("Core.News.List")]
            ListNews,
            [StringValue("Core.UserHash.List")]
            ListHash,
            [StringValue("Core.Config.View")]
            ViewConfig,
            [StringValue("Core.Config.Manage")]
            ManageConfig,
            [StringValue("Core.GeneralCategory.View")]
            ViewGeneralCategory,
            [StringValue("Core.GeneralCategory.Manage")]
            ManageGeneralCategory,
            [StringValue("Core.Language.View")]
            ViewLanguage,
            [StringValue("Core.Language.Manage")]
            ManageLanguage,
            [StringValue("Core.FormLog.View")]
            ViewFormLog,
            [StringValue("Core.FormLog.Manage")]
            ManageFormLog,
            [StringValue("Core.Form.View")]
            ViewForm,
            [StringValue("Core.Form.Manage")]
            ManageForm,
            [StringValue("Core.ContactHobbies.Manage")]
            ManageContactHobbies,
            [StringValue("Core.DriversLicense.Manage")]
            ManageDriversLicense,
            [StringValue("Core.Passport.Manage")]
            ManagePassport,
            [StringValue("Core.Address.Manage")]
            ManageAddress,
            [StringValue("Core.FamilyRegist.Manage")]
            ManageFamilyRegist,
            [StringValue("Core.AccessRole.View")]
            ViewAccessRole,
            [StringValue("Core.AccessRole.Manage")]
            ManageAccessRole,
            [StringValue("Core.ApprovalMatrix.View")]
            ViewApprovalMatrix,
            [StringValue("Core.ApprovalMatrix.Manage")]
            ManageApprovalMatrix,
            [StringValue("Core.Guideline.View")]
            ViewGuideline,
            [StringValue("Core.Guideline.Manage")]
            ManageGuideline,
            [StringValue("Core.Guideline.List")]
            ListGuideline,
            [StringValue("Core.EventsCalendar.View")]
            ViewEventsCalendar,
            [StringValue("Core.EventsCalendar.Manage")]
            ManageEventsCalendar,
            [StringValue("Core.EventsCalendar.List")]
            ListEventsCalendar,
            [StringValue("Core.GenericDataUploader.View")]
            ViewGenericDataUploader,
            [StringValue("Core.Administration.View")]
            ViewAdministration,
            [StringValue("Core.MasterData.View")]
            ViewMasterData,
            [StringValue("Core.ViewAllDocumentApprovals")]
            ViewAllDocumentApprovals,
            [StringValue("Core.ChangeFavouriteMenu")]
            ChangeFavouriteMenu,
            [StringValue("Core.ChangeLanguage")]
            ChangeLanguage,
            [StringValue("Core.ProxyAs")]
            ProxyAs,
            [StringValue("Core.ViewProfile")]
            ViewProfile,
            [StringValue("Core.SetLanguage")]
            SetLanguage,
            [StringValue("Core.Termination.View")]
            ViewTerminationMenu,
            [StringValue("PersonalData.She.View")]
            PersonalDataViewShe,
            [StringValue("PersonalData.She.Manage")]
            PersonalDataManageShe,
            [StringValue("Form.ReferenceLetter.UploadAttachment")]
            OthersUploadAttachment,
            [StringValue("Form.ReferenceLetter.View")]
            ViewReferenceLetter,
            [StringValue("Form.ReferenceLetter.Create")]
            CreateReferenceLetter,
            [StringValue("Form.LostAndReturn.View")]
            ViewLostAndReturn,
            [StringValue("Form.LostAndReturn.Create")]
            CreateLostAndReturn,
            [StringValue("Form.KaiLoan.View")]
            ViewKaiLoan,
            [StringValue("Form.KaiLoan.Create")]
            CreateKaiLoan,
            [StringValue("Form.Termination.Create")]
            CreateTerminationRequest,
            [StringValue("Form.Termination.View")]
            ViewTerminationForm,
            [StringValue("Form.Termination.ViewMenu")]
            ViewTerminationMenuForm,
            [StringValue("Form.HealthDeclaration.View")]
            ViewHealthDeclaration,
            [StringValue("Form.HealthDeclaration.Create")]
            CreateHealthDeclaration,
            [StringValue("Form.HealthDeclaration.ViewReport")]
            ViewHealthDeclarationReport,
            [StringValue("Form.NotebookRequest.View")]
            ViewNotebookRequest,
            [StringValue("Form.NotebookRequest.Create")]
            CreateNotebookRequest,
            [StringValue("Form.OthersPersonal.View")]
            ViewOthersPersonalDataRequest,
            [StringValue("Form.OthersPersonal.Create")]
            CreateOthersPersonalDataRequest,
            [StringValue("Form.ContactHobbies.Create")]
            CreateContactHobbiesDataRequest,
            [StringValue("Form.AddressNew.Create")]
            CreateAddressNewDataRequest,
            [StringValue("Form.DriversLicense.Create")]
            CreateDriversLicenseDataRequest,
            [StringValue("Form.PengkinianData.Create")]
            CreatePengkinianDataDataRequest,
            [StringValue("Form.PersonalConfirmation.View")]
            ViewPersonalDataConfirmationRequest,
            [StringValue("Form.PersonalConfirmation.Create")]
            CreatePersonalDataConfirmationRequest,
            [StringValue("Form.DataRequest.View")]
            ViewDataRequest,
            [StringValue("Form.DataRequest.Create")]
            CreateDataRequest,
            [StringValue("Form.ComplaintRequest.View")]
            ViewComplaintRequest,
            [StringValue("Form.ComplaintRequest.Create")]
            CreateComplaintRequest,
            [StringValue("Form.ComplaintRequest.ManageSolutions")]
            ManageSolutions,
            [StringValue("Form.ProxyTimeForm.View")]
            ViewProxyTimeForm,
            [StringValue("Form.ProxyTimeForm.Create")]
            CreateProxyTimeForm,
            [StringValue("MasterData.Bank.View")]
            ViewBank,
            [StringValue("MasterData.Bank.Manage")]
            ManageBank,
            [StringValue("MasterData.FormQuestion.View")]
            ViewFormQuestion,
            [StringValue("MasterData.FormQuestion.Manage")]
            ManageFormQuestion,
            [StringValue("MasterData.JsonCategory.View")]
            ViewJsonCategory,
            [StringValue("MasterData.JsonCategory.Manage")]
            ManageJsonCategory,
            [StringValue("MasterData.Vehicle.View")]
            ViewVehicle,
            [StringValue("MasterData.Vehicle.Manage")]
            ManageVehicle,
            [StringValue("MasterData.PersonalDataDocument.View")]
            ViewPersonalDataDocument,
            [StringValue("MasterData.PersonalDataDocument.Manage")]
            ManagePersonalDataDocument,
            [StringValue("MasterData.Insurance.View")]
            ViewInsurance,
            [StringValue("MasterData.Insurance.Manage")]
            ManageInsurance,
            [StringValue("MasterData.Bpjs.View")]
            ViewBpjs,
            [StringValue("MasterData.Bpjs.Manage")]
            ManageBpjs,
            [StringValue("MasterData.Spkl.View")]
            ViewSpklMasterData,
            [StringValue("MasterData.Spkl.Manage")]
            ManageSpklMasterData,
            [StringValue("MasterData.Bdjk.View")]
            ViewBdjkMasterData,
            [StringValue("MasterData.Bdjk.Manage")]
            ManageBdjkMasterData,
            [StringValue("MasterData.DailyWorkSchedule.View")]
            ViewDailyWorkSchedule,
            [StringValue("MasterData.DailyWorkSchedule.Manage")]
            ManageDailyWorkSchedule,
            [StringValue("MasterData.WorkSchedule.View")]
            ViewWorkSchedule,
            [StringValue("MasterData.WorkSchedule.Manage")]
            ManageWorkSchedule,
            [StringValue("MasterData.EmployeeWorkSchedule.View")]
            ViewEmployeeWorkSchedule,
            [StringValue("MasterData.EmployeeWorkSchedule.Manage")]
            ManageEmployeeWorkSchedule,
            [StringValue("MasterData.EmployeeWorkPlan.View")]
            ViewEmployeeWorkPlan,
            [StringValue("MasterData.EmployeeWorkPlan.Manage")]
            ManageEmployeeWorkPlan,
            [StringValue("MasterData.SapGeneralCategoryMap.View")]
            ViewSapGeneralCategoryMap,
            [StringValue("MasterData.SapGeneralCategoryMap.Manage")]
            ManageSapGeneralCategoryMap,
            [StringValue("MasterData.PrintoutMatrix.View")]
            ViewPrintoutMatrix,
            [StringValue("MasterData.PrintoutMatrix.Manage")]
            ManagePrintoutMatrix,
            [StringValue("MasterData.LetterTemplate.View")]
            ViewLetterTemplate,
            [StringValue("MasterData.LetterTemplate.Manage")]
            ManageLetterTemplate,
            [StringValue("MasterData.Hospital.View")]
            ViewHospital,
            [StringValue("MasterData.PengkinianData.View")]
            ViewPengkinianData,
            [StringValue("MasterData.Hospital.Manage")]
            ManageHospital,
            [StringValue("MasterData.Bpkb.View")]
            ViewBpkb,
            [StringValue("MasterData.Bpkb.Manage")]
            ManageBpkb,
            [StringValue("MasterData.ProxyTime.View")]
            ViewProxyTime,
            [StringValue("MasterData.ProxyTime.Manage")]
            ManageProxyTime,
            [StringValue("MasterData.AllowanceDetail.View")]
            ViewAllowanceDetail,
            [StringValue("MasterData.AllowanceDetail.Manage")]
            ManageAllowanceDetail,
            [StringValue("MasterData.Faskes.View")]
            ViewFaskes,
            [StringValue("MasterData.Faskes.Manage")]
            ManageFaskes,
            [StringValue("MasterData.EmployeeLeave.View")]
            ViewEmployeeLeave,
            [StringValue("MasterData.EmployeeLeave.Manage")]
            ManageEmployeeLeave,
            [StringValue("MasterData.EmployeeAnnualLeave.View")]
            ViewEmployeeAnnualLeave,
            [StringValue("MasterData.EmployeeAnnualLeave.Manage")]
            ManageEmployeeAnnualLeave,
            [StringValue("MasterData.Absence.View")]
            ViewAbsence,
            [StringValue("MasterData.Absence.Manage")]
            ManageAbsence,
            [StringValue("MasterData.BdjkDetail.View")]
            ViewBdjkDetail,
            [StringValue("MasterData.BdjkDetail.Manage")]
            ManageBdjkDetail,
            [StringValue("Form.SpklReport.View")]
            ViewSpklReport,
            [StringValue("Form.BdjkReport.View")]
            ViewBdjkReport,
            [StringValue("Form.AbsenceReport.View")]
            ViewAbsenceReport,
            [StringValue("Form.ShiftPlanningReport.View")]
            ViewShiftPlanningReport,
            [StringValue("Form.AyoSekolahReport.View")]
            ViewAyoSekolahReport,
            [StringValue("Form.MaternityLeaveAdmin.View")]
            ViewMaternityLeaveAdmin,
            [StringValue("Form.MaternityLeaveAdmin.Manage")]
            ManageMaternityLeaveAdmin,
            [StringValue("Core.Vaccine.View")]
            ViewVaccine,
            [StringValue("Core.Vaccine.Manage")]
            ManageVaccine,
            [StringValue("Core.Vaccine.ViewReport")]
            ViewVaccineReport,
            [StringValue("MasterData.VaccineSchedule.View")]
            ViewVaccineSchedule,
            [StringValue("MasterData.VaccineSchedule.Manage")]
            ManageVaccineSchedule,
            [StringValue("MasterData.VaccineHospital.View")]
            ViewVaccineHospital,
            [StringValue("MasterData.VaccineHospital.Manage")]
            ManageVaccineHospital,
            [StringValue("PersonalData.DriversLicense.Manage")]
            ManageDriversLicensePersonalData,
            [StringValue("PersonalData.Passport.Manage")]
            ManagePassportPersonalData,
            [StringValue("PersonalData.FamilyRegist.Manage")]
            ManageFamilyRegistPersonalData,
            [StringValue("Form.AbnormalityOverTime.View")]
            ViewAbnormalityOT,
            [StringValue("Form.AbnormalityOverTime.Manage")]
            ManageAbnormalityOT,
            [StringValue("Form.AbnormalityAbsence.View")]
            ViewAbnormalityAbsence,
            [StringValue("Form.AbnormalityAbsence.Manage")]
            ManageAbnormalityAbsence,
            [StringValue("Form.AbnormalityBDJK.View")]
            ViewAbnormalityBDJK,
            [StringValue("Form.AbnormalityBDJK.Manage")]
            ManageAbnormalityBDJK,
            [StringValue("TimeManagement.AbnormalityBDJK.ViewReport")]
            ReportAbnormalityBDJK,
            [StringValue("Other.TerminationReportView")]
            ViewTerminationReport,
            [StringValue("Form.WeeklyWFHPlanning.View")]
            ViewWeeklyWFHPlanning,
            [StringValue("Form.WeeklyWFHPlanning.Manage")]
            ManageWeeklyWFHPlanning,
            [StringValue("Reporting.EmployeeProfile.View")]
            ViewEmployeeProfile,
            [StringValue("Reporting.EmployeeProfileDetail.View")]
            ViewEmployeeProfileDetail,
            [StringValue("TimeManagement.MonitoringReport.View")]
            ViewMonitoringReport,
            [StringValue("ClaimBenefit.ClaimBenefit.View")]
            ViewClaimBenefit,
            [StringValue("TimeManagement.AnnualPlanning")]
            ReportAnnualPlanning,
            [StringValue("ClaimBenefit.MealAllowance.View")]
            ViewMealAllowance,
            [StringValue("OHS - TanyaOHS - Karyawan")]
            TanyaOhsKaryawan,
            [StringValue("OHS - TanyaOHS - PIC Asuransi")]
            TanyaOhsPicAsuransi,
            [StringValue("OHS - TanyaOHS - PIC Kesehatan")]
            TanyaOhsPicKesehatan,
            [StringValue("OHS TanyaOHS Admin")]
            TanyaOhsAdmin,
            [StringValue("OHS - Area Activity")]
            AreaActivityView,
            [StringValue("OHS - Medical record")]
            MedicalrecordView,
            [StringValue("OHS - UPKK")]
            OHSUPKKView,
            [StringValue("Master Data - OHS")]
            MasterDataOHSView,
            [StringValue("OHS - Approver")]
            OHSApproverDownloadView

        }

        public enum LogType
        {
            Success,
            Error,
            Warning
        }
    }
    #endregion
}
