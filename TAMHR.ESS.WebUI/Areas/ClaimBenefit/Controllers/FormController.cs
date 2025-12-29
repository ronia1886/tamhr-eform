using System;
using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.ViewModels;
using TAMHR.ESS.Infrastructure.DomainServices;
using Rotativa.AspNetCore;

namespace TAMHR.ESS.WebUI.Areas.ClaimBenefit.Controllers
{
    /// <summary>
    /// Claim & benefit form controller.
    /// </summary>
    [Area(ApplicationModule.ClaimBenefit)]
    public class FormController : FormControllerBase
    {
        #region Domain Services
        protected ClaimBenefitQueryService ClaimBenefitServicePartial => ServiceProxy.GetService<ClaimBenefitQueryService>();
        #endregion

        #region Constructor
        /// <summary>
        /// Public constructor.
        /// </summary>
        public FormController()
        {
            // Register ayo sekolah form action.
            RegisterView<AyoSekolahViewModel>(ApplicationForm.AyoSekolah);

            // Register reimbursement form action.
            RegisterView<ReimbursementViewModel>(ApplicationForm.Reimbursement);

            // Register PTA allowance form action.
            RegisterView<PtaAllowanceViewModel>(ApplicationForm.PtaAllowance);

            // Register vacation allowance form action.
            RegisterView<VacationAllowanceViewModel>(ApplicationForm.VacationAllowance);

            // Register meal allowance form action.
            RegisterView<MealAllowanceViewModel>(ApplicationForm.MealAllowance);

            // Register distressed allowance form action.
            RegisterView<DistressedAllowanceViewModel>(ApplicationForm.DistressedAllowance);

            // Register COP fuel allowance form action.
            RegisterView<CopFuelAllowanceViewModel>(ApplicationForm.CopFuelAllowance);

            // Register get BPKB COP form action.
            RegisterView<GetBpkbCopViewModel>(ApplicationForm.GetBpkbCop);

            // Register KB allowance form action.
            RegisterView<KbAllowanceViewModel>(ApplicationForm.KbAllowance);

            // Register marriage allowance form action.
            RegisterView<MarriageAllowanceViewModel>(ApplicationForm.MarriageAllowance);

            // Register COP form action.
            RegisterView<CopViewModel>(ApplicationForm.Cop);

            // Register CPP form action.
            RegisterView<CppViewModel>(ApplicationForm.Cpp);

            // Register SCP form action.
            RegisterView<ScpViewModel>(ApplicationForm.Scp);

            // Register eyeglasses allowance form action.
            RegisterView<EyeglassesAllowanceViewModel>(ApplicationForm.EyeglassesAllowance);

            // Register return BPKB COP form action.
            RegisterView<ReturnBpkbCOPViewModel>(ApplicationForm.ReturnBpkbCop);

            // Register company loan form action.
            RegisterView<LoanViewModel>(ApplicationForm.CompanyLoan);

            // Register company loan 3-6 form action.
            RegisterView<Loan36ViewModel>(ApplicationForm.CompanyLoan36);

            // Register shift meal allowance form action.
            RegisterView<ShiftMealAllowanceViewModel>(ApplicationForm.ShiftMealAllowance);

            // Register letter of guarantee form action.
            RegisterView<LetterOfGuaranteeViewModel>(ApplicationForm.LetterOfGuarantee);

            // Register concept idea allowance form action.
            RegisterView<IdeBerkonsepViewModel>(ApplicationForm.ConceptIdeaAllowance);

            // Register DPA register form action.
            RegisterView<DpaRegisterViewModel>(ApplicationForm.DpaRegister);

            // Register condolance allowance form action.
            RegisterView<MisseryAllowanceViewModel>(ApplicationForm.CondolanceAllowance);

            // Register ayo sekolah download form action.
            RegisterPdfView<AyoSekolahViewModel>("FormAyoSekolahPdf", ApplicationForm.AyoSekolah);

            // Register reimbursement download form action.
            RegisterPdfView<ReimbursementViewModel>("FormPenggantianBiayaReimburshmentRSPdf", ApplicationForm.Reimbursement);

            // Register PTA allowance download form action.
            RegisterPdfView<PtaAllowanceViewModel>("FormAllowancePaymentPdf", ApplicationForm.PtaAllowance);

            // Register vacation allowance download form action.
            RegisterPdfView<VacationAllowanceViewModel>("FormAllowancePaymentPdf", ApplicationForm.VacationAllowance);

            // Register meal allowance download form action.
            RegisterPdfView<MealAllowanceViewModel>("FormShiftClassAllowancePdf", ApplicationForm.MealAllowance);

            // Register distressed allowance download form action.
            RegisterPdfView<DistressedAllowanceViewModel>("FormAllowancePaymentPdf", ApplicationForm.DistressedAllowance);

            // Register COP fuel allowance download form action.
            RegisterPdfView<CopFuelAllowanceViewModel>("FormCopFuelAllowancePdf", ApplicationForm.CopFuelAllowance);

            // Register get BPKB COP download form action.
            RegisterPdfView<GetBpkbCopViewModel>("FormTakingCopPdf", ApplicationForm.GetBpkbCop);

            // Register KB allowance download form action.
            RegisterPdfView<KbAllowanceViewModel>("FormAllowancePaymentPdf", ApplicationForm.KbAllowance);

            // Register marriage allowance download form action.
            RegisterPdfView<MarriageAllowanceViewModel>("FormAllowancePaymentPdf", ApplicationForm.MarriageAllowance);

            // Register COP download form action.
            RegisterPdfView<CopViewModel>("FormCopPdf", ApplicationForm.Cop);

            // Register CPP download form action.
            RegisterPdfView<CppViewModel>("FormCPPCashPdf", ApplicationForm.Cpp);

            // Register SCP download form action.
            RegisterPdfView<CppViewModel>("FormCPPCashPdf", ApplicationForm.Scp);

            // Register eyeglasses allowance download form action.
            RegisterPdfView<EyeglassesAllowanceViewModel>("FormAllowancePaymentPdf", ApplicationForm.EyeglassesAllowance);

            // Register return BPKB COP download form action.
            RegisterPdfView<ReturnBpkbCOPViewModel>("FormBorrowBPKBCOPPdf", ApplicationForm.ReturnBpkbCop);

            // Register company loan download form action.
            RegisterPdfView<LoanViewModel>("FormCompanyLoanPdf", ApplicationForm.CompanyLoan);

            // Register company loan 3-6 download form action.
            RegisterPdfView<Loan36ViewModel>("FormPermohonanPinjamanKelas36Pdf", ApplicationForm.CompanyLoan36);

            // Register shift meal allowance download form action.
            RegisterPdfView<ShiftMealAllowanceViewModel>("FormShiftMealAllowancePdf", ApplicationForm.ShiftMealAllowance);

            // Register letter of guarantee download form action.
            RegisterPdfView<LetterOfGuaranteeViewModel>("FormLetterOfGuaranteePdf", ApplicationForm.LetterOfGuarantee);

            // Register concept idea allowance download form action.
            RegisterPdfView<IdeBerkonsepViewModel>("FormAllowancePaymentPdf", ApplicationForm.ConceptIdeaAllowance);

            // Register DPA register download form action.
            RegisterPdfView<DpaRegisterViewModel>("FormDpaPdf", ApplicationForm.DpaRegister);

            // Register condolance allowance download form action.
            RegisterPdfView<MisseryAllowanceViewModel>("FormAllowancePaymentPdf", ApplicationForm.CondolanceAllowance);
        }
        #endregion

        #region Public Methods
        [HttpGet]
        public IActionResult DownloadAgreement(Guid id, int NP, string Type)
        {
            var model = ApprovalService.GetDocumentApprovalById(id);
            var obj = ApprovalService.GetDocumentRequestDetailViewModel<LoanViewModel>(id, ServiceProxy.UserClaim.NoReg);

            var org = ServiceProxy.GetService<MdmService>().GetActualOrganizationStructure(model.CreatedBy);
            var orgObj = ServiceProxy.GetService<MdmService>().GetEmployeeOrganizationObjects(model.CreatedBy);
            var title = $"Document Agreement";

            var customSwitches = string.Format("--title \"{0}\" ", title);
            string url = "";
            if (NP >= 3 && NP <= 6)
            {
                url = "pdf/DownloadAggrementHome36Pdf";
            }
            else if (NP >= 7)
            {
                if (Type == "buyinghouse7up" || Type == "buyingland7up" || Type == "buildhouse7up" || Type == "renovationhouse7up")
                {
                    url = "pdf/DownloadAggrementHome7upPdf";
                }
                else if (Type == "buyingvehicle7up")
                {
                    url = "pdf/DownloadAggrementCarPdf";
                }
            }

            if (!string.IsNullOrEmpty(url))
            {
                return new ViewAsPdf(url, new PdfViewModel()
                {
                    OrgInfo = org,
                    OrgObjects = orgObj,
                    Object = obj.Object,
                    ViewData = id
                })
                {
                    CustomSwitches = customSwitches,
                    ContentDisposition = Rotativa.AspNetCore.Options.ContentDisposition.Inline,
                    FileName = title + ".pdf",
                    PageSize = Rotativa.AspNetCore.Options.Size.A4,
                    PageMargins = new Rotativa.AspNetCore.Options.Margins(20, 25, 10, 25)
                };
            }
            else
            {
                throw new Exception("Template Aggrement Not Found");
            }
        }
        #endregion
    }
}