using System;
using FluentValidation;
using TAMHR.ESS.Infrastructure.Validator;
using TAMHR.ESS.Infrastructure.ViewModels;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class VacationAllowanceValidator : AbstractDocumentValidator<VacationAllowanceViewModel>
    {
        public VacationAllowanceValidator()
        {
            RuleFor(x => x.Object.VacationDate).NotEmpty();
            RuleFor(x => x.Object.Location).NotEmpty();
            RuleFor(x => x.Object.ProposalPath).NotEmpty();
            When(x => !string.IsNullOrEmpty(x.Object.AccountType), () =>
            {
                When(x => x.Object.AccountType.Equals("rekening"), () => { });
                When(x => x.Object.AccountType.Equals("rekeninglain"), () => {
                    RuleFor(x => x.Object.BankCode).NotEmpty();
                    RuleFor(x => x.Object.AccountNumber).NotEmpty();
                    RuleFor(x => x.Object.AccountName).NotEmpty();
                });
            });

            //When(x => x.DocumentStatusCode=="inprogress", () =>
            //{
            //    RuleFor(x => x.Object.LocationActual).NotEmpty();
            //    RuleFor(x => x.Object.VacationDateActual).NotEmpty();
            //    RuleFor(x => x.Object.DepartmentActuals).NotEmpty();
            //});
        }
    }
}
