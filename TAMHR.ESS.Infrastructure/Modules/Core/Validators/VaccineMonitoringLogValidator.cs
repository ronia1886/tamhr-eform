using Microsoft.Extensions.Localization;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;
using FluentValidation;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class VaccineMonitoringLogValidator : AbstractValidator<VaccineMonitoringLog>
    {
        public VaccineMonitoringLogValidator(ConfigService configService, VaccineService formService, IStringLocalizer<VaccineMonitoringLogValidator> localizer)
        {
            RuleFor(x => x.VaccineId).NotEmpty()
                .WithMessage(localizer["Must select form"].Value);

            RuleFor(x => x.Notes).NotEmpty()
                .WithMessage(localizer["This field cannot be empty"].Value)
                .MaximumLength(ApplicationConstants.VarcharMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0} characters"].Value, ApplicationConstants.VarcharMaxLength));
        }
    }
}
