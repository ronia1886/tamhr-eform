using Microsoft.Extensions.Localization;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;
using FluentValidation;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class ProxyTimeValidator : AbstractValidator<TimeManagement>
    {
        public ProxyTimeValidator(DailyWorkScheduleService dailyWorkScheduleService, IStringLocalizer<ProxyTimeValidator> localizer)
        {
            RuleFor(x => x.NoReg).NotEmpty();

            RuleFor(x => x.WorkingTimeIn).Must((item, x) =>
            {
                return x.HasValue ? x.Value.Date == item.WorkingDate.Date : true;
            });

            RuleFor(x => x.ShiftCode).NotEmpty()
                .WithMessage(localizer["Please select shift"].Value)
                .Must(x => dailyWorkScheduleService.Contains(x))
                .WithMessage(localizer["Value is not in category"].Value);

            RuleFor(x => x.WorkingTimeOut).GreaterThanOrEqualTo(x => x.WorkingTimeIn).LessThanOrEqualTo(x => x.WorkingDate.AddDays(2));
        }
    }
}
