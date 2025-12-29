using FluentValidation;
using TAMHR.ESS.Infrastructure.Validator;
using TAMHR.ESS.Infrastructure.ViewModels;
namespace TAMHR.ESS.Infrastructure.Modules.ClaimBenefit.Validators
{
    
    public class DpaChangeValidation : AbstractDocumentValidator<DpaChangeViewModel>
    {

        public DpaChangeValidation()
        {
            RuleFor(x => x.Object.AccountNumber).NotEmpty();
            RuleFor(x => x.Object.AccountName).NotEmpty();
            RuleFor(x => x.Object.Email).NotEmpty();
            RuleFor(x => x.Object.HouseNumber).NotEmpty();
            RuleFor(x => x.Object.MobilePhoneNumber).NotEmpty();
            RuleFor(x => x.Object.Name).NotEmpty();
            RuleFor(x => x.Object.Remarks).NotEmpty();

        }
    }
}
