using System;
using System.Linq;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.ViewModels;
using TAMHR.ESS.Infrastructure.DomainServices;
using Agit.Common;
using Rotativa.AspNetCore;
using TAMHR.ESS.Domain;

namespace TAMHR.ESS.WebUI.Areas.TimeManagement.Controllers
{
    /// <summary>
    /// Time management form controller.
    /// </summary>
    [Area(ApplicationModule.TimeManagement)]
    public class FormController : FormControllerBase
    {
        #region Constructor
        /// <summary>
        /// Public constructor.
        /// </summary>
        public FormController()
        {
            // Register absence form action.
            RegisterView<AbsenceViewModel>(ApplicationForm.Absence);

            // Register BDJK planning form action.
            RegisterView<BdjkPlanningViewModel>(ApplicationForm.BdjkPlanning);

            // Register maternity leave form action.
            RegisterView<MaternityLeaveViewModel>(ApplicationForm.MaternityLeave);

            // Register shift planning form action.
            RegisterView<ShiftPlanningViewModel>(ApplicationForm.ShiftPlanning);

            // Register SPKL overtime form action.
            RegisterView<SpklOvertimeViewModel>(ApplicationForm.SpklOvertime);

            // Register BDJK report form action.
            RegisterView<BdjkReportViewModel>(ApplicationForm.BdjkReport);

            // Register SPKL report form action.
            RegisterView<SpklReportViewModel>(ApplicationForm.SpklReport);

            // Register shift planning report form action.
            RegisterView<ShiftPlanningReportViewModel>(ApplicationForm.ShiftReport);

            // Register annual leave planning form action.
            RegisterView<AnnualLeavePlanningViewModel>(ApplicationForm.AnnualLeavePlanning);

            // Register annual WFH planning form action.
            RegisterView<AnnualWFHPlanningViewModel>(ApplicationForm.AnnualWFHPlanning);

            // Register annual OT planning form action.
            RegisterView<AnnualOTPlanningViewModel>(ApplicationForm.AnnualOTPlanning);

            // Register annual BDJK planning form action.
            RegisterView<AnnualBDJKPlanningViewModel>(ApplicationForm.AnnualBDJKPlanning);

            // Register annual Abnormality Absence form action.
            RegisterView<AbnormalityAbsenceViewModel>(ApplicationForm.AbnormalityAbsence);

            // Register annual Abnormality OverTime form action.
            RegisterView<AbnormalityOverTimeViewModel>(ApplicationForm.AbnormalityOverTime);

            // Register annual Abnormality BDJK form action.
            RegisterView<AbnormalityBdjkViewModel>(ApplicationForm.AbnormalityBdjk);

            // Register Weekly WFH Planning
            RegisterView<WeeklyWFHPlanningViewModel>(ApplicationForm.WeeklyWFHPlanning);

            // Register abnormality absence download form action.
            RegisterPdfView<AbnormalityAbsenceView>("FormAbnormalityAbsencePdf", ApplicationForm.AbnormalityAbsence);

            // Register abnormality over time download form action.
            RegisterPdfView<AbnormalityOverTimeView>("FormAbnormalityOverTimePdf", ApplicationForm.AbnormalityOverTime);

            // Register abnormality bdjk download form action.
            RegisterPdfView<AbnormalityOverTimeView>("FormAbnormalityBdjkPdf", ApplicationForm.AbnormalityBdjk);

            // Register absence download form action.
            RegisterPdfView<AbsenceViewModel>("FormAbsencePdf", ApplicationForm.Absence);

            // Register maternity leave download form action.
            RegisterPdfView<MaternityLeaveViewModel>("FormPregnancyStatementPdf", ApplicationForm.MaternityLeave);

            // Register SPKL overtime download form action.
            RegisterPdfView<SpklOvertimeViewModel>("FormSpklOvertimePlanPdf", ApplicationForm.SpklOvertime);

            // Register BDJK planning download form action.
            RegisterPdfView<BdjkPlanningViewModel>("FormBdjkLogSheetPdf", ApplicationForm.BdjkPlanning);

            // Register Annual Leave planning download form action.
            RegisterPdfView<AnnualLeavePlanningViewModel>("FormAnnualLeavePlanningPdf", ApplicationForm.AnnualLeavePlanning);

            // Register Annual Leave planning download form action.
            RegisterPdfView<AnnualBDJKPlanningViewModel>("FormAnnualBDJKPlanningPdf", ApplicationForm.AnnualBDJKPlanning);

            // Register Annual WFH planning download form action.
            RegisterPdfView<AnnualWFHPlanningViewModel>("FormAnnualWFHPlanningPdf", ApplicationForm.AnnualWFHPlanning);

            // Register Annual OT planning download form action.
            RegisterPdfView<AnnualWFHPlanningViewModel>("FormAnnualOTPlanningPdf", ApplicationForm.AnnualOTPlanning);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Download SPKL plan by parent document approval id and key date in string format.
        /// </summary>
        /// <param name="parentId">This parent document approval id.</param>
        /// <param name="keyDateStr">This key date in string format.</param>
        public IActionResult DownloadSpklPlan(Guid parentId, string keyDateStr)
        {
            // Parse key date string into date time data type.
            var keyDate = DateTime.ParseExact(keyDateStr, "dd/MM/yyyy", CultureInfo.CurrentCulture);

            // Get and set SPKL service object from DI container.
            var service = ServiceProxy.GetService<SpklService>();

            // Get list of SPKL request details by parent document approval id and key date.
            var output = service.GetSpklRequestDetails(parentId, keyDate);

            // Get the first record.
            var firstRecord = output.FirstOrDefault();

            // Throw an exception if first record is null or empty.
            Assert.ThrowIf(firstRecord == null, $"Download failed, there is no record in {keyDateStr}");

            // Get document approval by first record's document approval id.
            var model = ApprovalService.GetDocumentApprovalById(firstRecord.DocumentApprovalId.Value);

            // Set title.
            var title = "Surat Perintah Kerja Lembur";

            // Set rotative custom switches string.
            var customSwitches = string.Format(
                                     "--title \"{0}\" " +
                                     "--header-html \"{1}\" " +
                                     "--header-spacing \"0\" " +
                                     "--header-font-size \"9\" ",
                                     title, GetHeaderPdfUrl("/core/default/commonheader")
                                 );

            // Return rotativa PDF view.
            return new ViewAsPdf("Pdf/FormSpklOvertimePlanByDatePdf", new PdfViewModel()
            {
                DocumentApproval = model,
                Object = output
            })
            {
                ContentDisposition = Rotativa.AspNetCore.Options.ContentDisposition.Inline,
                FileName = title + ".pdf",
                CustomSwitches = customSwitches,
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageMargins = new Rotativa.AspNetCore.Options.Margins(30, 15, 10, 15)
            };
        }
        #endregion
    }
}