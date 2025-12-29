using Microsoft.Extensions.Localization;
using TAMHR.ESS.Domain;
using FluentValidation;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class GuidelineValidator : AbstractValidator<Guideline>
    {
        public GuidelineValidator(IStringLocalizer<GuidelineValidator> localizer)
        {
            RuleFor(x => x.Title).NotEmpty()
                .WithMessage(localizer["Title must be filled"].Value);

            RuleFor(x => x.StartDate).NotEmpty()
                .WithMessage(localizer["Start date must be filled"].Value);

            RuleFor(x => x.EndDate).NotEmpty()
                .WithMessage(localizer["End date must be filled"].Value)
                .GreaterThanOrEqualTo(x => x.StartDate)
                .WithMessage(localizer["End date must be greater than or equal to start date"].Value);

            RuleFor(x => x.Description).NotEmpty()
                .WithMessage(localizer["Description must be filled"].Value)
                .MaximumLength(ApplicationConstants.CommonInputMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.CommonInputMaxLength));
        }
    }
}
