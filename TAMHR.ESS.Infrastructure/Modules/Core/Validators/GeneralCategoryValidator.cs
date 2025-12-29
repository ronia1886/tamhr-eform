using Microsoft.Extensions.Localization;
using TAMHR.ESS.Domain;
using FluentValidation;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class GeneralCategoryValidator : AbstractValidator<GeneralCategory>
    {
        public GeneralCategoryValidator(IStringLocalizer<GeneralCategoryValidator> localizer)
        {
            RuleFor(x => x.Code).NotEmpty()
                .WithMessage(localizer["Code must be filled"].Value)
                .MaximumLength(ApplicationConstants.SmallInputMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.SmallInputMaxLength));

            RuleFor(x => x.Category).NotEmpty()
                .WithMessage(localizer["Category must be filled"].Value)
                .MaximumLength(ApplicationConstants.MediumInputMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.MediumInputMaxLength));

            RuleFor(x => x.Description).NotEmpty()
                .WithMessage(localizer["Description must be filled"].Value)
                .MaximumLength(ApplicationConstants.MediumLongInputMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.MediumLongInputMaxLength));

            RuleFor(x => x.Name).NotEmpty()
                .WithMessage(localizer["Name must be filled"].Value)
                .MaximumLength(ApplicationConstants.MediumLongInputMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.MediumLongInputMaxLength));

            RuleFor(x => x.OrderSequence).NotEmpty()
                .WithMessage(localizer["Order sequence must be filled"].Value);
        }
    }
}
