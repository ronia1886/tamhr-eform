using Microsoft.Extensions.Localization;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;
using FluentValidation;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class VaccineHospitalValidator : AbstractValidator<VaccineHospital>
    {
        public VaccineHospitalValidator(ConfigService configService, IStringLocalizer<VaccineHospitalValidator> localizer)
        {
            RuleFor(x => x.Name).NotEmpty()
                .WithMessage(localizer["Name must be filled"].Value)
                .MaximumLength(ApplicationConstants.MediumInputMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.MediumInputMaxLength));

            RuleFor(x => x.Province).NotEmpty()
                .WithMessage(localizer["Province must be filled"].Value)
                .MaximumLength(ApplicationConstants.SmallInputMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.SmallInputMaxLength));

            RuleFor(x => x.City).NotEmpty()
                .WithMessage(localizer["City must be filled"].Value)
                .MaximumLength(ApplicationConstants.SmallInputMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.SmallInputMaxLength));

            RuleFor(x => x.Address).NotEmpty()
                .WithMessage(localizer["Address must be filled"].Value)
                .MaximumLength(ApplicationConstants.SmallVarcharMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.SmallVarcharMaxLength));
        }
    }
}
