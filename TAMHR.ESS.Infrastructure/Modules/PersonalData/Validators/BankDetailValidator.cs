using FluentValidation;
using TAMHR.ESS.Infrastructure.Validator;
using TAMHR.ESS.Infrastructure.ViewModels;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class BankDetailValidator : AbstractDocumentValidator<BankDetailViewModel>
    {
        public BankDetailValidator()
        {
            RuleFor(x => x.Object.BankName).NotEmpty();
            RuleFor(x => x.Object.BranchBank).NotEmpty();
            RuleFor(x => x.Object.LocationBank).NotEmpty();
            RuleFor(x => x.Object.KeyBank).NotEmpty();
            RuleFor(x => x.Object.AccountNumber).NotEmpty();
            RuleFor(x => x.Object.AccountName).NotEmpty();
            RuleFor(x => x.Object.SupportingAttachmentPath).NotEmpty();

        }
    }
}
