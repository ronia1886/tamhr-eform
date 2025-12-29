using FluentValidation;
using TAMHR.ESS.Infrastructure.Validator;
using TAMHR.ESS.Infrastructure.ViewModels;

namespace TAMHR.ESS.Infrastructure.Validators
{
    
    public class ReimbursementValidator : AbstractDocumentValidator<ReimbursementViewModel>
    {
        public ReimbursementValidator()
        {
            RuleFor(x => x.Object.FamilyRelationship).NotEmpty();

            When(x => !string.IsNullOrEmpty(x.Object.FamilyRelationship), () =>
            {
                When(x => x.Object.FamilyRelationship.Equals("rsanak"), () => {
                    RuleFor(x => x.Object.PatientChildName).NotEmpty();
                });

                When(x => x.Object.FamilyRelationship.Equals("rskaryawan") || x.Object.FamilyRelationship.Equals("rspasangan"), () => {
                    RuleFor(x => x.Object.PatientName).NotEmpty();
                });
            });

            When(x => x.Object.AccountType == "rekeninglain", () =>
            {
                RuleFor(x => x.Object.AccountName).NotEmpty();
                RuleFor(x => x.Object.AccountNumber).NotEmpty();
                RuleFor(x => x.Object.BankCode).NotEmpty();
            });

            RuleFor(x => x.Object.BirthDate).NotEmpty();

            When(x => x.Object.IsOtherHospital.Equals(true), () =>
            {
                RuleFor(x => x.Object.OtherHospital).NotEmpty();
            });

            When(x => x.Object.IsOtherHospital.Equals(false), () =>
            {
                RuleFor(x => x.Object.Hospital).NotEmpty();
            });
            
            RuleFor(x => x.Object.DateOfEntry).NotEmpty();
            RuleFor(x => x.Object.DateOfOut).NotEmpty();
            RuleFor(x => x.Object.InPatient).NotEmpty();
            RuleFor(x => x.Object.PhoneNumber).NotEmpty();

            When(x => x.Object.IsInputTotalClaim.Equals(true), () => {
                RuleFor(x => x.Object.TotalClaim).GreaterThanOrEqualTo(0);
            });

            When(x => x.Object.IsInputCompanyClaim.Equals(true), () => {
                RuleFor(x => x.Object.TotalCompanyClaim).GreaterThanOrEqualTo(0).LessThanOrEqualTo(x => x.Object.Cost - x.Object.TotalClaim);
            });

            When(x => x.Object.IsInputTotalClaim.Equals(false), () => {
                RuleFor(x => x.Object.Cost).NotEmpty();
            });
        }
    }
}
