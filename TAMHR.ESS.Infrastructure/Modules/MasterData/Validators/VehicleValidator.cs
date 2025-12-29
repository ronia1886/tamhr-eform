using Microsoft.Extensions.Localization;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;
using FluentValidation;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class VehicleValidator : AbstractValidator<Vehicle>
    {
        public VehicleValidator(ConfigService configService, IStringLocalizer<VehicleValidator> localizer)
        {
            RuleFor(x => x.ModelCode).NotEmpty()
                .WithMessage(localizer["Model code must be filled"].Value)
                .MaximumLength(ApplicationConstants.SmallInputMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.SmallInputMaxLength));

            RuleFor(x => x.Type).NotEmpty()
                .WithMessage(localizer["Type must be filled"].Value)
                .MaximumLength(ApplicationConstants.MediumInputMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.MediumInputMaxLength));

            RuleFor(x => x.TypeName).NotEmpty()
                .WithMessage(localizer["Type name must be filled"].Value)
                .MaximumLength(ApplicationConstants.MediumInputMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.MediumInputMaxLength));

            RuleFor(x => x.Suffix).NotEmpty()
                .WithMessage(localizer["Suffix must be filled"].Value)
                .MaximumLength(ApplicationConstants.SmallInputMaxLength - 10)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.SmallInputMaxLength - 10));

            RuleFor(x => x.Colors).NotEmpty()
                .WithMessage(localizer["Colors must be filled"].Value);
        }
    }
}
