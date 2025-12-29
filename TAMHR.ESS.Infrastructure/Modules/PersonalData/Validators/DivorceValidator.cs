using FluentValidation;
using TAMHR.ESS.Infrastructure.Validator;
using TAMHR.ESS.Infrastructure.ViewModels;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class DivorceValidator : AbstractDocumentValidator<DivorceViewModel>
    {
        public DivorceValidator()
        {
            When(x => !x.Object.IsDraft, () =>
            {
                RuleFor(x => x.Object.PartnerName).NotEmpty();
                RuleFor(x => x.Object.DivorceDate).NotEmpty();
                RuleFor(x => x.Object.DivorceCertificatePath).NotEmpty();

            });
           
        }
    }
}
