using Microsoft.Extensions.Localization;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;
using FluentValidation;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class LetterTemplateValidator : AbstractValidator<LetterTemplate>
    {
        public LetterTemplateValidator(ConfigService configService, IStringLocalizer<LetterTemplate> localizer)
        {
            RuleFor(x => x.Title).NotEmpty()
                .WithMessage(localizer["Title must be filled"].Value)
                .MaximumLength(ApplicationConstants.MediumLongInputMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.MediumLongInputMaxLength));

            RuleFor(x => x.LetterKey).NotEmpty()
                .WithMessage(localizer["Letter key must be filled"].Value)
                .MaximumLength(ApplicationConstants.SmallInputMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.SmallInputMaxLength));

            RuleFor(x => x.LetterContent).NotEmpty()
                .WithMessage(localizer["Letter content must be filled"].Value);

            RuleFor(x => x.Description).NotEmpty()
                .WithMessage(localizer["Description must be filled"].Value)
                .MaximumLength(ApplicationConstants.CommonMediumInputMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.CommonMediumInputMaxLength));
        }
    }
}
