using Microsoft.Extensions.Localization;
using TAMHR.ESS.Domain;
using FluentValidation;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class EmployeeWorkScheduleValidator : AbstractValidator<EmployeeWorkSchedule>
    {
        public EmployeeWorkScheduleValidator(IStringLocalizer<EmployeeWorkSchedule> localizer)
        {
            RuleFor(x => x.WorkScheduleRule).NotEmpty()
                .WithMessage(localizer["Work schedule rule must be filled"].Value)
                .MaximumLength(ApplicationConstants.SmallInputMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.SmallInputMaxLength));
        }
    }
}
