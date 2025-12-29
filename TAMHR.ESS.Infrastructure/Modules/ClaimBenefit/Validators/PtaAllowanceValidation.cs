using FluentValidation;
using TAMHR.ESS.Infrastructure.Validator;
using TAMHR.ESS.Infrastructure.ViewModels;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class PtaAllowanceValidator : AbstractDocumentValidator<PtaAllowanceViewModel>
    {
        public PtaAllowanceValidator()
        {
           
            //RuleFor(x => x.Object.CateogryPTA).NotEmpty();
            //RuleFor(x => x.Object.date).NotEmpty();
            //RuleFor(x => x.Object.Amouont).NotEmpty();
            //RuleFor(x => x.Object.Departement).NotEmpty();
            When(x => !string.IsNullOrEmpty(x.Object.AccountType), () =>
            {
                When(x => x.Object.AccountType.Equals("rekening"), () => { });
                When(x => x.Object.AccountType.Equals("rekeninglain"), () => {
                    RuleFor(x => x.Object.BankCode).NotEmpty();
                    RuleFor(x => x.Object.AccountNumber).NotEmpty();
                    RuleFor(x => x.Object.AccountName).NotEmpty();
                });
            });
            //RuleFor(x => x.Object.ProposalPath).NotEmpty();
        }
    }
}
