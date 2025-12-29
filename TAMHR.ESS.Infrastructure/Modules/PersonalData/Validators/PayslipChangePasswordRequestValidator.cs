using System.Linq;
using Microsoft.Extensions.Localization;
using TAMHR.ESS.Infrastructure.Requests;
using Microsoft.AspNetCore.Http;
using TAMHR.ESS.Infrastructure.DomainServices;
using FluentValidation;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class PayslipChangePasswordRequestValidator : AbstractValidator<PayslipChangePasswordRequest>
    {
        public PayslipChangePasswordRequestValidator(IHttpContextAccessor httpContextAccessor, CoreService coreService, IStringLocalizer<PayslipChangePasswordRequestValidator> localizer)
        {
            var minLength = 6;
            var noreg = httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "NoReg").Value;

            RuleFor(x => x.OldPassword).NotEmpty()
                .WithMessage(localizer["This field cannot be empty"].Value)
                .Must((data, x) => coreService.ValidHash(noreg, HashType.Payslip, data.OldPassword, Configurations.PayslipDefaultPassword))
                .WithMessage(localizer["You input the wrong password"].Value);

            RuleFor(x => x.NewPassword).NotEmpty()
                .WithMessage(localizer["This field cannot be empty"].Value)
                .MinimumLength(minLength)
                .WithMessage(localizer[string.Format("Minimum {0} character long", minLength)].Value);
        }
    }
}
