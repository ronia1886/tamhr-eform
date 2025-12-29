using Microsoft.Extensions.Localization;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;
using FluentValidation;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class VaccineScheduleValidator : AbstractValidator<VaccineSchedule>
    {
        public VaccineScheduleValidator(ConfigService configService, IStringLocalizer<VaccineScheduleValidator> localizer)
        {
            RuleFor(x => x.StartDateTime).NotEmpty()
                .WithMessage(localizer["Start Date must be filled"].Value);

            RuleFor(x => x.EndDateTime).NotEmpty()
                .WithMessage(localizer["End Date must be filled"].Value)
                .GreaterThanOrEqualTo(x => x.StartDateTime)
                .WithMessage(localizer["End Date must greater than Start Date"]);

            RuleFor(x => x.VaccineNumber).NotEmpty()
                .WithMessage(localizer["Vaccine Number must be filled"].Value);
        }
    }
}
