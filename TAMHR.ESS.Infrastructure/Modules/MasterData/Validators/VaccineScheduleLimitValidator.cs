using Microsoft.Extensions.Localization;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;
using FluentValidation;
using System.Data;
namespace TAMHR.ESS.Infrastructure.Validators
{
    public class VaccineScheduleLimitValidator : AbstractValidator<VaccineScheduleLimit>
    {
        public VaccineScheduleLimitValidator(ConfigService configService,VaccineScheduleService vaccineScheduleService, IStringLocalizer<VaccineScheduleLimitValidator> localizer)
        {

            RuleFor(x => x.VaccineDate).NotEmpty()
                .WithMessage(localizer["Vaccine Date must be filled"].Value)
                .Must((data, context) =>
                {
                    var result = true;
                    if (data.VaccineDate != null && data.VaccineHospitalId != System.Guid.Empty && data.Id == System.Guid.Empty)
                    {
                        var data2 = vaccineScheduleService.GetVaccineScheduleLimitByDateHospital(data.VaccineDate.Value, data.VaccineHospitalId);
                        if (data2.Count >0)
                        {
                            result = false;
                        }
                    }

                    return result;

                })
                .WithMessage(localizer["Hospital and Vaccine Date is exists"].Value);

            RuleFor(x => x.VaccineHospitalId).NotEmpty()
                .WithMessage(localizer["Hospital must be filled"].Value);

            RuleFor(x => x.Qty).NotEmpty()
                .WithMessage(localizer["Quota must be filled"].Value);
        }
    }
}
