using Microsoft.Extensions.Localization;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;
using FluentValidation;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class BankValidator : AbstractValidator<Bank>
    {
        public BankValidator(ConfigService configService, IStringLocalizer<BankValidator> localizer)
        {
            RuleFor(x => x.BankKey).NotEmpty()
                .WithMessage(localizer["Bank key must be filled"].Value)
                .MaximumLength(ApplicationConstants.SmallInputMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.SmallInputMaxLength));

            RuleFor(x => x.BankName).NotEmpty()
                .WithMessage(localizer["Bank name must be filled"].Value)
                .MaximumLength(ApplicationConstants.MediumInputMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.MediumInputMaxLength));

            RuleFor(x => x.Branch).NotEmpty()
                .WithMessage(localizer["Branch must be filled"].Value)
                .MaximumLength(ApplicationConstants.SmallInputMaxLength + 10)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.SmallInputMaxLength + 10));

            RuleFor(x => x.Address).NotEmpty()
                .WithMessage(localizer["Address must be filled"].Value)
                .MaximumLength(ApplicationConstants.MediumInputMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.MediumInputMaxLength));
        }
    }
}
