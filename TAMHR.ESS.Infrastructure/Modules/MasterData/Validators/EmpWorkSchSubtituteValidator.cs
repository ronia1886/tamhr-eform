using Microsoft.Extensions.Localization;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;
using FluentValidation;
using System;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class EmpWorkSchSubtituteValidator : AbstractValidator<EmpWorkSchSubtitute>
    {
        public EmpWorkSchSubtituteValidator(DailyWorkScheduleService dailyWorkScheduleService, IStringLocalizer<EmpWorkSchSubtituteValidator> localizer)
        {
            When(x => x.Id == default(Guid), () =>
            {
                RuleFor(x => x.NoReg).NotEmpty()
                    .WithMessage(localizer["Please select employee"].Value);

                RuleFor(x => x.Date).NotEmpty();
            });

            RuleFor(x => x.ShiftCodeUpdate).NotEmpty()
                .WithMessage(localizer["Please select shift"].Value)
                .Must(x => dailyWorkScheduleService.Contains(x))
                .WithMessage(localizer["Value is not in category"].Value);
        }
    }
}
