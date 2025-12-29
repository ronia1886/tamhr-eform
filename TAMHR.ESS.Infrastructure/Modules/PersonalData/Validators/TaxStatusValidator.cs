using FluentValidation;
using TAMHR.ESS.Infrastructure.Validator;
using TAMHR.ESS.Infrastructure.ViewModels;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class TaxStatusValidator : AbstractDocumentValidator<TaxStatusViewModel>
    {
        public TaxStatusValidator()
        {
            RuleFor(x => x.Object.NPWPNumber).NotEmpty();
            RuleFor(x => x.Object.StatusTax).NotEmpty();
            RuleFor(x => x.Object.SupportingAttachmentPath).NotEmpty();

        }
    }
}
