using Microsoft.Extensions.Localization;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;
using FluentValidation;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class EmployeeLeaveValidator : AbstractValidator<EmployeeLeave>
    {
        public EmployeeLeaveValidator(ConfigService configService, IStringLocalizer<EmployeeLeaveValidator> localizer)
        {
            RuleFor(x => x.NoReg).NotEmpty();
        }
    }
}
