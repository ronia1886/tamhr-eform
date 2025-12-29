using FluentValidation;
using TAMHR.ESS.Infrastructure.Validator;
using TAMHR.ESS.Infrastructure.ViewModels;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class KbAllowanceValidator : AbstractDocumentValidator<KbAllowanceViewModel>
    {
        public KbAllowanceValidator()
        {
            RuleFor(x => x.Object.FamilyRelationship).NotEmpty();
            RuleFor(x => x.Object.PassienName).NotEmpty();
            RuleFor(x => x.Object.Hospital).NotEmpty();
            RuleFor(x => x.Object.HospitalAddress).NotEmpty();
            RuleFor(x => x.Object.ActionKBDate).NotEmpty();
            RuleFor(x => x.Object.Cost).NotEmpty();
            RuleFor(x => x.Object.SupportingAttachmentPath).NotEmpty();
        }
    }
}
