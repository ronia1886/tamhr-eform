using FluentValidation;
using TAMHR.ESS.Infrastructure.Validator;
using TAMHR.ESS.Infrastructure.ViewModels;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class DpaRegisterValidation : AbstractDocumentValidator<DpaRegisterViewModel>
    {
    
        public DpaRegisterValidation()
        {


            RuleFor(x => x.Object.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Object.MobilePhoneNumber).NotEmpty();
            When(x => !string.IsNullOrEmpty(x.Object.AccountType), () =>
            {
                When(x => x.Object.AccountType.Equals("rekening"), () => { });
                When(x => x.Object.AccountType.Equals("rekeninglain"), () => {
                    RuleFor(x => x.Object.BankCode).NotEmpty();
                    RuleFor(x => x.Object.AccountNumber).NotEmpty();
                    RuleFor(x => x.Object.AccountName).NotEmpty();
                    RuleFor(x => x.Object.Branch).NotEmpty();
                });
            });
        }
    }
}
