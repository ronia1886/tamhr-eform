using Microsoft.Extensions.Localization;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;
using FluentValidation;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class AbsenceMasterValidator : AbstractValidator<Absence>
    {
        public AbsenceMasterValidator(ConfigService configService, IStringLocalizer<AbsenceMasterValidator> localizer)
        {
            RuleFor(x => x.Code).NotEmpty()
                .WithMessage(localizer["Code must be filled"].Value)
                .MaximumLength(ApplicationConstants.SmallInputMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.SmallInputMaxLength));

            RuleFor(x => x.CodePresensi).NotEmpty()
                .WithMessage(localizer["Presence code must be filled"].Value)
                .GreaterThan(0)
                .WithMessage(localizer["Value must be greater than 0"].Value);

            RuleFor(x => x.AbsenceType).NotEmpty()
                .WithMessage(localizer["Absence Type must be filled"].Value);
        }
    }
}
