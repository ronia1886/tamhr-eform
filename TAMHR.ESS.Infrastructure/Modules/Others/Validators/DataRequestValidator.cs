using System;
using Microsoft.Extensions.Localization;
using TAMHR.ESS.Infrastructure.Validator;
using TAMHR.ESS.Infrastructure.ViewModels;
using FluentValidation;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class DataRequestValidator : AbstractDocumentValidator<DataRequestViewModel>
    {
        public DataRequestValidator(IStringLocalizer<DataRequestValidator> localizer)
        {
            RuleFor(x => x.Object.PurposeOfUsage).NotEmpty()
                .WithMessage(localizer["Purpose of usage must be filled"].Value)
                .MaximumLength(MaximumLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, MaximumLength));

            RuleFor(x => x.Object.DataDescription).NotEmpty()
                .WithMessage(localizer["Data description must be filled"].Value)
                .MaximumLength(MaximumLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, MaximumLength));

            RuleFor(x => x.Object.RequestDate).NotEmpty()
                .WithMessage(localizer["Request date must be filled"].Value)
                .GreaterThanOrEqualTo(DateTime.Now.Date)
                .WithMessage(localizer["Request date must be greater than or equal to now"].Value);

            RuleFor(x => x.Object.FilePath).NotEmpty()
                .WithMessage(localizer["Please upload a file"].Value);

            RuleFor(x => x.Object.Remarks).MaximumLength(MaximumLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, MaximumLength));
        }
    }
}
