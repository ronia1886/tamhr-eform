using FluentValidation;
using TAMHR.ESS.Infrastructure.Validator;
using TAMHR.ESS.Infrastructure.ViewModels;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class Loan36Validation : AbstractDocumentValidator<Loan36ViewModel>
    {
        public Loan36Validation()
        {
            RuleFor(x => x.Object.LoanType).NotEmpty();
           
            RuleFor(x => x.Object.CostNeeds).NotEmpty();
            RuleFor(x => x.Object.LoanAmount).NotEmpty();
            RuleFor(x => x.Object.SupportAttachmentPath).NotEmpty();

            When(x => string.IsNullOrEmpty(x.Object.Brand) && x.Object.ClassRequester >= 7, () =>
            {
                RuleFor(x => x.Object.Province).NotEmpty();
                RuleFor(x => x.Object.City).NotEmpty();
                RuleFor(x => x.Object.District).NotEmpty();
                RuleFor(x => x.Object.PostalCode).NotEmpty();
                RuleFor(x => x.Object.Address).NotEmpty();
                RuleFor(x => x.Object.SubDistrictCode).NotEmpty();
            });

            When(x => string.IsNullOrEmpty(x.Object.Address) && x.Object.ClassRequester >= 7, () =>
            {
                RuleFor(x => x.Object.Brand).NotEmpty();
            });
        }
    }
}
