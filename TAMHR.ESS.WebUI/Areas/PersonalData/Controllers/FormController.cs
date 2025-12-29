using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.ViewModels;

namespace TAMHR.ESS.WebUI.Areas.PersonalData.Controllers
{
    /// <summary>
    /// Personal data form controller.
    /// </summary>
    [Area(ApplicationModule.PersonalData)]
    public class FormController : FormControllerBase
    {
        #region Constructor
        /// <summary>
        /// Public constructor.
        /// </summary>
        public FormController()
        {
            // Register marriage status form action.
            RegisterView<MarriageStatusViewModel>(ApplicationForm.MarriageStatus);

            // Register family registration form action.
            RegisterView<BirthRegistrationViewModel>(ApplicationForm.FamilyRegistration);

            // Register condolance form action.
            RegisterView<DismembermentViewModel>(ApplicationForm.Condolance);

            // Register divorce form action.
            RegisterView<DivorceViewModel>(ApplicationForm.Divorce);

            // Register address form action.
            RegisterView<AddressViewModel>(ApplicationForm.Address);

            // Register bank account form action.
            RegisterView<BankDetailViewModel>(ApplicationForm.BankAccount);

            // Register tax status form action.
            RegisterView<TaxStatusViewModel>(ApplicationForm.TaxStatus);

            // Register education form action.
            RegisterView<EducationViewModel>(ApplicationForm.Education);

            // Register others personal data form action.
            RegisterView<OthersPersonalDataViewModel>(ApplicationForm.OthersPersonalData);

            // Register contact hobbies form action.
            RegisterView<OthersPersonalDataViewModel>(ApplicationForm.ContactHobbies);

            // Register personal data confirmation form action.
            RegisterView<PersonalDataConfirmationViewModel>(ApplicationForm.PersonalDataConfirmation);

            // Register marriage status download form action.
            RegisterPdfView<MarriageStatusViewModel>("DataChangePdf", ApplicationForm.MarriageStatus);

            // Register family registration download form action.
            RegisterPdfView<BirthRegistrationViewModel>("DataChangePdf", ApplicationForm.FamilyRegistration);

            // Register condolance download form action.
            RegisterPdfView<DismembermentViewModel>("DataChangePdf", ApplicationForm.Condolance);

            // Register divorce download form action.
            RegisterPdfView<DivorceViewModel>("DataChangePdf", ApplicationForm.Divorce);

            // Register bank account download form action.
            RegisterPdfView<BankDetailViewModel>("DataChangePdf", ApplicationForm.BankAccount);

            // Register education download form action.
            RegisterPdfView<EducationViewModel>("DataChangePdf", ApplicationForm.Education);

            // Register address download form action.
            RegisterPdfView<AddressViewModel>("DataChangePdf", ApplicationForm.Address);

            // Register tax status download form action.
            RegisterPdfView<TaxStatusViewModel>("FormTaxStatusPdf", ApplicationForm.TaxStatus);

            // Register driver license form action.
            RegisterView<DriverLicenseViewModel>(ApplicationForm.DriversLicense);

            // Register passport form action.
            RegisterView<PassportViewModel>(ApplicationForm.Passport);

            // Register family registration form action.
            RegisterView<FamilyRegistrationViewModel>(ApplicationForm.FamilyRegist);
        }
        #endregion

        #region Override Methods
        /// <summary>
        /// Override how to set view data.
        /// </summary>
        /// <returns>This view data.</returns>
        protected override object SetViewData()
        {
            // If form key is address then return personal data common attribute object.
            if (FormKey == ApplicationForm.Address)
            {
                // Get and set personal data common attribute by noreg into output variable.
                var output = PersonalDataService.GetPersonalDataAttribute(ServiceProxy.UserClaim.NoReg);

                // Return the output variable.
                return output;
            }

            // Return base set view data function.
            return base.SetViewData();
        }

        /// <summary>
        /// Override how to set pdf view data.
        /// </summary>
        /// <returns>This pdf view data.</returns>
        protected override object SetPdfViewData()
        {
            // If form key is marriage status then return "Status Kawin" string.
            if (FormKey == ApplicationForm.MarriageStatus)
            {
                // Return "Status Kawin" string.
                return "Status Kawin";
            }

            // If form key is family registration then return anonymous object.
            if (FormKey == ApplicationForm.FamilyRegistration)
            {
                // Return anonymous object.
                return new { Name = "Nama", Age = 10 };
            }

            // Return base set pdf view data function.
            return base.SetPdfViewData();
        }
        #endregion
    }
}