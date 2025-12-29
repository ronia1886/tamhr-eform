using Microsoft.Extensions.Localization;
using TAMHR.ESS.Infrastructure.ViewModels;
using TAMHR.ESS.Infrastructure.DomainServices;
using FluentValidation;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class MaternityLeaveDobValidator : AbstractValidator<MaternityLeaveDobViewModel>
    {
        public MaternityLeaveDobValidator(TimeManagementService service, IStringLocalizer<MaternityLeaveDobValidator> localizer)
        {
            RuleFor(x => x.Attachments).Must(x => x != null)
                .WithMessage(localizer["Attachments cannot be empty"].Value);

            RuleFor(x => x.BirthCertificatePath).NotEmpty()
                .WithMessage(localizer["Please upload birth certificate files"].Value);

            RuleFor(x => x.DayOfBirth).NotEmpty()
                .WithMessage(localizer["Day of birth must be greater than or equal to start maternity leave and lower than or equal to estimated day of birth"].Value)
                .Must((item, x) =>
                {
                    var obj = service.GetMaternityLeaveViewModel(item.Id);
                    
                    return x >= obj.StartMaternityLeave.Value && x <= obj.EstimatedDayOfBirth.Value.AddDays(45);
                })
                .WithMessage(localizer["Day of birth must be greater than or equal to start maternity leave and lower than or equal to estimated day of birth"].Value);

        }
    }
}
