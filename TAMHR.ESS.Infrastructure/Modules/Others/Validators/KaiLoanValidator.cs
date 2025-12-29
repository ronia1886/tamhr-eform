using System;
using Microsoft.Extensions.Localization;
using TAMHR.ESS.Infrastructure.Validator;
using TAMHR.ESS.Infrastructure.ViewModels;
using TAMHR.ESS.Infrastructure.DomainServices;
using FluentValidation;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class KaiLoanValidator : AbstractDocumentValidator<KaiLoanViewModel>
    {
        public KaiLoanValidator(ConfigService configService, IStringLocalizer<KaiLoanValidator> localizer)
        {
            RuleFor(x => x.Object.LoanType).NotEmpty()
                .WithMessage(localizer["Please select loan type"].Value)
                .Must(x => configService.ValueInCategories(x, "KaiLoanType"))
                .WithMessage(localizer["Value is not in category"].Value);

            RuleFor(x => x.Object.Loan).NotEmpty()
                .WithMessage(localizer["Loan cannot be empty"].Value)
                .Must(x => Math.Floor(x) == x);

            RuleFor(x => x.Object.FilePath).NotEmpty()
                .WithMessage(localizer["Please upload a file"].Value);

            RuleFor(x => x.Object.Remarks).MaximumLength(MaximumLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, MaximumLength));
        }
    }
}
