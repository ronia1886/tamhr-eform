using FluentValidation;
using TAMHR.ESS.Infrastructure.Validator;
using TAMHR.ESS.Infrastructure.ViewModels;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class LetterOfGuaranteeValidator : AbstractDocumentValidator<LetterOfGuaranteeViewModel>
    {
        public LetterOfGuaranteeValidator()
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

            RuleFor(x => x.Object.DateOfBirth).NotEmpty();
            RuleFor(x => x.Object.StartDateOfCare).NotEmpty();
            RuleFor(x => x.Object.EndDateOfCare).NotEmpty();
            RuleFor(x => x.Object.CriteriaControl).NotEmpty();
            RuleFor(x => x.Object.Hospital).NotEmpty();
            RuleFor(x => x.Object.HospitalAddress).NotEmpty();
            RuleFor(x => x.Object.HospitalCity).NotEmpty();
            RuleFor(x => x.Object.ControlDate).NotEmpty();
            RuleFor(x => x.Object.CheckUpCount).Must(x =>
            {
                var num = int.Parse(x);

                return num > 0 && num <= 3;
            });

            When(x => !string.IsNullOrEmpty(x.Object.CriteriaControl), () =>
            {
                When(x => x.Object.CriteriaControl.Equals("2"), () => {
                    RuleFor(x => x.Object.Diagnosa).NotEmpty();
                    RuleFor(x => x.Object.CheckUpCount).NotEmpty();
                });
            });

            When(x => !string.IsNullOrEmpty(x.Object.CheckUpCount), () =>
            {
                When(x => x.Object.CheckUpCount.Equals("1"), () => {
                    RuleFor(x => x.Object.SupportingAttachmentPath).NotEmpty();
                    RuleFor(x => x.Object.DoctorAgreementPath).NotEmpty();
                });

                When(x => x.Object.CheckUpCount.Equals("2") || x.Object.CheckUpCount.Equals("3"), () => {
                    RuleFor(x => x.Object.DoctorAgreementPath).NotEmpty();
                });

                When(x => x.Object.CheckUpCount != "1" && x.Object.CheckUpCount != "2" && x.Object.CheckUpCount != "3", () => {
                    RuleFor(x => x.Object.TreatmentResumePath).NotEmpty();
                });
            });
        }
    }
}
