using Microsoft.Extensions.Localization;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;
using FluentValidation;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class BdjkDetailValidator : AbstractValidator<BdjkDetail>
    {
        public BdjkDetailValidator(ConfigService configService, IStringLocalizer<BdjkDetailValidator> localizer)
        {
            RuleFor(x => x.BdjkCode).NotEmpty()
                .WithMessage(localizer["Code must be filled"].Value)
                .MaximumLength(5)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, 5));

            RuleFor(x => x.BdjkValue).NotEmpty()
                .WithMessage(localizer["Code must be filled"].Value)
                .MaximumLength(ApplicationConstants.MediumInputMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.MediumInputMaxLength));

            RuleFor(x => x.ClassFrom).NotEmpty()
                .WithMessage(localizer["Class from must be filled"].Value)
                .MaximumLength(ApplicationConstants.SmallInputMaxLength - 10)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.SmallInputMaxLength - 10));

            RuleFor(x => x.ClassTo).NotEmpty()
                .WithMessage(localizer["Class to must be filled"].Value)
                .MaximumLength(ApplicationConstants.SmallInputMaxLength - 10)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.SmallInputMaxLength - 10));

            RuleFor(x => x.Description).NotEmpty()
                .WithMessage(localizer["Code must be filled"].Value)
                .MaximumLength(ApplicationConstants.MediumInputMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.MediumInputMaxLength));

            RuleFor(x => x.FlagHoliday).NotEmpty()
                .WithMessage(localizer["Flag holiday must be filled"].Value);
        }
    }
}
