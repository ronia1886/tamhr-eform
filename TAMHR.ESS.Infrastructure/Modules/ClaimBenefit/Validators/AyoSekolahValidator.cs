using FluentValidation;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Validator;
using TAMHR.ESS.Infrastructure.ViewModels;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class AyoSekolahValidator : AbstractDocumentValidator<AyoSekolahViewModel>
    {
        public AyoSekolahValidator(FormService formService)
        {
            RuleFor(x => x.Object.RegNumber).NotEmpty();
            RuleFor(x => x.Object.Name).NotEmpty();
            RuleFor(x => x.Object.Class).NotEmpty();
            RuleFor(x => x.Object.RegNumber).Custom((data, context) =>
            {
                formService.ValidateCreateForm(ApplicationForm.AyoSekolah);

            });
        }
    }
}
