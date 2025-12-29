using FluentValidation;
using TAMHR.ESS.Infrastructure.Validator;
using TAMHR.ESS.Infrastructure.ViewModels;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class MisseryAllowanceValidator : AbstractDocumentValidator<MisseryAllowanceViewModel>
    {
        public MisseryAllowanceValidator()
        {
            RuleFor(x => x.Object.MisseryDate).NotEmpty();
            RuleFor(x => x.Object.IsMainFamily).NotEmpty();
            RuleFor(x => x.Object.FamilyCardPath).NotEmpty();
            When(x => !string.IsNullOrEmpty(x.Object.IsMainFamily), () =>
            {
                When(x => x.Object.IsMainFamily.Equals("ya"), () => {
                    RuleFor(x => x.Object.OtherFamilyName).NotEmpty();
                    RuleFor(x => x.Object.OtherFamilyId).NotEmpty();
                });
                When(x => x.Object.IsMainFamily.Equals("tidak"), () => {
                    RuleFor(x => x.Object.NonFamilyRelationship).NotEmpty();
                    RuleFor(x => x.Object.FamilyName).NotEmpty();
                });
            });
        }
    }
}
