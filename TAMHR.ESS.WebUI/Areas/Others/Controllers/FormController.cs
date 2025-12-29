using System;
using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.ViewModels;
using TAMHR.ESS.Infrastructure.DomainServices;
using Rotativa.AspNetCore;

namespace TAMHR.ESS.WebUI.Areas.Others.Controllers
{
    /// <summary>
    /// Others form controller.
    /// </summary>
    [Area(ApplicationModule.Others)]
    public class FormController : FormControllerBase
    {
        #region Constructor
        /// <summary>
        /// Public constructor.
        /// </summary>
        public FormController()
        {
            // Register form view action.
            RegisterView();

            // Register pdf view action.
            RegisterPdfView();
        }
        #endregion

        #region Partial Views
        /// <summary>
        /// Load complaint request form.
        /// </summary>
        /// <param name="viewModel">This <see cref="ComplaintRequestViewModel"/> object.</param>
        public IActionResult LoadComplaintRequest(ComplaintRequestViewModel viewModel)
        {
            // return complaint request form with view model.
            return PartialView("_ComplaintRequestForm", viewModel);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Download reference letter document by document approval id.
        /// </summary>
        /// <param name="id">This document approval id.</param>
        [HttpGet]
        public IActionResult DownloadReferenceLetterDocument(Guid id)
        {
            var model = ApprovalService.GetDocumentApprovalById(id);
            var obj = ApprovalService.GetDocumentRequestDetailViewModel<ReferenceLetterViewModel>(id, ServiceProxy.UserClaim.NoReg);

            var printoutMatrix = CoreService.GetPrintOut(id);
            var org = ServiceProxy.GetService<MdmService>().GetActualOrganizationStructure(model.CreatedBy);
            var orgObj = ServiceProxy.GetService<MdmService>().GetEmployeeOrganizationObjects(model.CreatedBy);
            var title = $"Reference Letter Document for {org.Name}";
            

            var customSwitches = string.Format("--title \"{0}\" ", title);

            try
            {
                var data= new ViewAsPdf("pdf/ReferenceLetterDownloadPdf", new PdfViewModel()
                {
                    OrgInfo = org,
                    OrgObjects = orgObj,
                    PrintoutMatrix = printoutMatrix,
                    Object = obj.Object
                })
                {
                    CustomSwitches = customSwitches,
                    ContentDisposition = Rotativa.AspNetCore.Options.ContentDisposition.Inline,
                    FileName = title + ".pdf",
                    PageSize = Rotativa.AspNetCore.Options.Size.A4,
                    PageMargins = new Rotativa.AspNetCore.Options.Margins(20, 25, 10, 25)
                };

                return data;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

           
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Register form view action.
        /// </summary>
        private void RegisterView()
        {
            // Register reference letter form action.
            RegisterView<ReferenceLetterViewModel>(ApplicationForm.ReferenceLetter);

            // Register lost and return form action.
            RegisterView<LostAndReturnViewModel>(ApplicationForm.LostAndReturn);

            // Register data request form action.
            RegisterView<DataRequestViewModel>(ApplicationForm.DataRequest);

            // Register notebook request form action.
            RegisterView<NotebookRequestViewModel>(ApplicationForm.NotebookRequest);

            // Register KAI loan form action.
            RegisterView<KaiLoanViewModel>(ApplicationForm.KaiLoan);

            // Register complaint request form action.
            RegisterView<ComplaintRequestViewModel>(ApplicationForm.ComplaintRequest);

            // Register health declaration form action.
            RegisterView<HealthDeclarationViewModel>(ApplicationForm.HealthDeclaration);

            // Register vaccine form action.
            RegisterView<VaccineViewModel>(ApplicationForm.Vaccine);

            // Register annual WFH planning form action.
            RegisterView<TerminationViewModel>(ApplicationForm.Termination);
        }

        /// <summary>
        /// Register pdf view action.
        /// </summary>
        private void RegisterPdfView()
        {
            // Register reference letter download form action.
            RegisterPdfView<ReferenceLetterViewModel>("ReferenceLetterPdf", ApplicationForm.ReferenceLetter);

            // Register notebook request download form action.
            RegisterPdfView<NotebookRequestViewModel>("NotebookRequestPdf", ApplicationForm.NotebookRequest);

            // Register complaint request download form action.
            RegisterPdfView<ComplaintRequestViewModel>("ComplaintRequestPdf", ApplicationForm.ComplaintRequest);

            // Register data request download form action.
            RegisterPdfView<DataRequestViewModel>("DataRequestPdf", ApplicationForm.DataRequest);

            // Register lost & return download form action.
            RegisterPdfView<LostAndReturnViewModel>("LostAndReturnPdf", ApplicationForm.LostAndReturn);

            // Register termination download form action.
            RegisterPdfView<TerminationViewModel>("TerminationPdf", ApplicationForm.Termination);
        }
        #endregion
    }
}