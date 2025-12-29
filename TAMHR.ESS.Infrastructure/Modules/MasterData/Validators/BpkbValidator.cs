using Microsoft.Extensions.Localization;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;
using FluentValidation;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class BpkbValidator : AbstractValidator<Bpkb>
    {
        public BpkbValidator(ConfigService configService, IStringLocalizer<BpkbValidator> localizer)
        {
            RuleFor(x => x.NoBPKB).NotEmpty()
                .WithMessage(localizer["BPKB number must be filled"].Value)
                .MaximumLength(ApplicationConstants.SmallInputMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.SmallInputMaxLength));

            RuleFor(x => x.LicensePlat).NotEmpty()
                .WithMessage(localizer["License plat must be filled"].Value)
                .MaximumLength(ApplicationConstants.SmallInputMaxLength - 10)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.SmallInputMaxLength - 10));

            RuleFor(x => x.VINNo).NotEmpty()
                .WithMessage(localizer["VIN number must be filled"].Value)
                .MaximumLength(ApplicationConstants.SmallInputMaxLength + 10)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.SmallInputMaxLength + 10));

            RuleFor(x => x.Model).NotEmpty()
                .WithMessage(localizer["Model must be filled"].Value)
                .MaximumLength(ApplicationConstants.SmallInputMaxLength + 10)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.SmallInputMaxLength + 10));

            RuleFor(x => x.Type).NotEmpty()
                .WithMessage(localizer["Type must be filled"].Value)
                .MaximumLength(ApplicationConstants.SmallVarcharMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.SmallVarcharMaxLength));

            RuleFor(x => x.EngineNo).NotEmpty()
                .WithMessage(localizer["Engine number must be filled"].Value)
                .MaximumLength(ApplicationConstants.SmallInputMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.SmallInputMaxLength));

            RuleFor(x => x.VehicleOwner).NotEmpty()
                .WithMessage(localizer["Vehicle owner must be filled"].Value)
                .MaximumLength(ApplicationConstants.MediumInputMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.MediumInputMaxLength));

            RuleFor(x => x.Color).NotEmpty()
                .WithMessage(localizer["Color must be filled"].Value)
                .MaximumLength(ApplicationConstants.SmallInputMaxLength + 15)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.SmallInputMaxLength + 15));

            RuleFor(x => x.Address).NotEmpty()
                .WithMessage(localizer["Address must be filled"].Value)
                .MaximumLength(ApplicationConstants.RemarksMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.RemarksMaxLength));

            RuleFor(x => x.CreatedYear).NotEmpty();
        }
    }
}
