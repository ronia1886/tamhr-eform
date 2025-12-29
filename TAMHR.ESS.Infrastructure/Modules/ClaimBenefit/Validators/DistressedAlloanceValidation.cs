using FluentValidation;
using TAMHR.ESS.Infrastructure.Validator;
using TAMHR.ESS.Infrastructure.ViewModels;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class DistressedAlloanceValidator : AbstractDocumentValidator<DistressedAllowanceViewModel>
    {
        public DistressedAlloanceValidator()
        {
            RuleFor(x => x.Object.Description).NotEmpty();
            RuleFor(x => x.Object.SupportingAttachmentPath).NotEmpty();
            RuleFor(x => x.Object.DateDistressed).NotEmpty();

        }
    }
}
