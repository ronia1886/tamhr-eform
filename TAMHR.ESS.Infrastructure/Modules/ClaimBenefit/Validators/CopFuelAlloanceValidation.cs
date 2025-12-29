using FluentValidation;
using TAMHR.ESS.Infrastructure.Validator;
using TAMHR.ESS.Infrastructure.ViewModels;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class CopFuelAlloanceValidation : AbstractDocumentValidator<CopFuelAllowanceViewModel>
    {
        public CopFuelAlloanceValidation()
        {
            //RuleFor(x => x.Object.Date).NotEmpty();
            //RuleFor(x => x.Object.Destination).NotEmpty();
            //RuleFor(x => x.Object.Start).NotEmpty();
            //RuleFor(x => x.Object.StartAttachmentPath).NotEmpty();
            //RuleFor(x => x.Object.Back).NotEmpty();
            //RuleFor(x => x.Object.BackAttachmentPath).NotEmpty();
            //RuleFor(x => x.Object.Necessity).NotEmpty();

        }
    }
}
