using FluentValidation;
using TAMHR.ESS.Infrastructure.Validator;
using TAMHR.ESS.Infrastructure.ViewModels;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class IdeBerkonsepValidator : AbstractDocumentValidator<IdeBerkonsepViewModel>
    {
        public IdeBerkonsepValidator()
        {
            RuleFor(x => x.Object.CriretiaCode).NotEmpty();
            RuleFor(x => x.Object.Title).NotEmpty();
            RuleFor(x => x.Object.Value).NotEmpty();
            RuleFor(x => x.Object.Amount).NotEmpty();
            RuleFor(x => x.Object.ProposalPath).NotEmpty();
        }
    }
}
