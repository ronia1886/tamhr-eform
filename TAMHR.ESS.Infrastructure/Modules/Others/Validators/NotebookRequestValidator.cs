using System;
using Microsoft.Extensions.Localization;
using TAMHR.ESS.Infrastructure.Validator;
using TAMHR.ESS.Infrastructure.ViewModels;
using FluentValidation;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class NotebookRequestValidator : AbstractDocumentValidator<NotebookRequestViewModel>
    {
        public NotebookRequestValidator(IStringLocalizer<NotebookRequestValidator> localizer)
        {
            RuleFor(x => x.Object.AssetNumber).NotEmpty()
                .WithMessage(localizer["Asset number must be filled"].Value);

            RuleFor(x => x.Object.StartDate).NotEmpty()
                .WithMessage(localizer["Start date must be filled"].Value)
                .GreaterThanOrEqualTo(DateTime.Now.Date)
                .WithMessage(localizer["Start date must be greater than or equal to now"].Value);

            RuleFor(x => x.Object.EndDate).NotEmpty()
                .WithMessage(localizer["End date must be filled"].Value)
                .GreaterThan(x => x.Object.StartDate)
                .WithMessage(localizer["End date must be greater than start date"].Value);

            RuleFor(x => x.Object.Reason).MaximumLength(MaximumLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, MaximumLength));

            RuleFor(x => x.Object.Remarks).MaximumLength(MaximumLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, MaximumLength));
        }
    }
}
