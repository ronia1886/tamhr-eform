using Microsoft.Extensions.Localization;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;
using FluentValidation;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class FaskesValidator : AbstractValidator<Faskes>
    {
        public FaskesValidator(ConfigService configService, IStringLocalizer<Faskes> localizer)
        {
            RuleFor(x => x.FaskesCode).NotEmpty()
                .WithMessage(localizer["Faskes code must be filled"].Value)
                .MaximumLength(ApplicationConstants.SmallInputMaxLength - 10)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.SmallInputMaxLength - 10));

            RuleFor(x => x.FaskesName).NotEmpty()
                .WithMessage(localizer["Faskes name must be filled"].Value)
                .MaximumLength(ApplicationConstants.MediumInputMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.MediumInputMaxLength));

            RuleFor(x => x.FaskesCity).NotEmpty()
                .WithMessage(localizer["Faskes city must be filled"].Value)
                .MaximumLength(ApplicationConstants.SmallInputMaxLength + 10)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.SmallInputMaxLength + 10));

            RuleFor(x => x.FaskesAddress).NotEmpty()
                .WithMessage(localizer["Faskes address must be filled"].Value)
                .MaximumLength(ApplicationConstants.MediumInputMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.MediumInputMaxLength));
        }
    }
}
