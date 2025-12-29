using Microsoft.Extensions.Localization;
using TAMHR.ESS.Domain;
using FluentValidation;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class JsonCategoryValidator : AbstractValidator<JsonCategory>
    {
        public JsonCategoryValidator(IStringLocalizer<JsonCategoryValidator> localizer)
        {
            RuleFor(x => x.Category).NotEmpty()
                .WithMessage(localizer["Category must be filled"].Value)
                .MaximumLength(ApplicationConstants.MediumLongInputMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.MediumLongInputMaxLength));

            RuleFor(x => x.Title).NotEmpty()
                .WithMessage(localizer["Title must be filled"].Value)
                .MaximumLength(ApplicationConstants.MediumLongInputMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.MediumLongInputMaxLength));

            RuleFor(x => x.Description).MaximumLength(ApplicationConstants.CommonInputMaxLength - 50)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.CommonInputMaxLength - 50));

            RuleFor(x => x.JsonValues).NotEmpty()
                .WithMessage(localizer["JSON values must be filled"].Value);
        }
    }
}
