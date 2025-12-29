using Microsoft.Extensions.Localization;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;
using FluentValidation;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class VaccineScheduleBackdateValidator : AbstractValidator<VaccineScheduleBackdateStoredEntity>
    {
        public VaccineScheduleBackdateValidator(ConfigService configService, VaccineScheduleBackdateStoredEntity formService, IStringLocalizer<VaccineScheduleBackdateValidator> localizer)
        {
            RuleFor(x => x.VaccineDate1)
                .Must((data, x) =>
                {
                    var output = true;
                    if (data.VaccineDate1 == null || data.VaccineHospital1 == null)
                    {
                        output = false;
                    }

                    return output;
                })
                .WithMessage(localizer["Vaccine Date 1 and Vaccine Hospital 1 cannot be empty"].Value);

            RuleFor(x => x.VaccineHospital1)
                .Must((data, x) =>
                {
                    var output = true;
                    if (data.VaccineDate1 == null || data.VaccineHospital1 == null)
                    {
                        output = false;
                    }

                    return output;
                })
                .WithMessage(localizer["Vaccine Date 1 and Vaccine Hospital 1 cannot be empty"].Value);

            RuleFor(x => x.VaccineDate2)
                .Must((data, x) =>
                {
                    var output = true;
                    if (data.VaccineDate2 == null || data.VaccineHospital2 == null)
                    {
                        output = false;
                    }

                    return output;
                })
                .WithMessage(localizer["Vaccine Date 2 and Vaccine Hospital 2 cannot be empty"].Value);

            RuleFor(x => x.VaccineHospital2)
                .Must((data, x) =>
                {
                    var output = true;
                    if (data.VaccineDate2 == null || data.VaccineHospital2 == null)
                    {
                        output = false;
                    }

                    return output;
                })
                .WithMessage(localizer["Vaccine Date 2 and Vaccine Hospital 2 cannot be empty"].Value);
        }
    }
}
