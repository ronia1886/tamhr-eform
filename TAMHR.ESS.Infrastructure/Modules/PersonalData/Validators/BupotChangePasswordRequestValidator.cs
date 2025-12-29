using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Requests;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class BupotChangePasswordRequestValidator : AbstractValidator<BupotChangePasswordRequest>
    {
        public BupotChangePasswordRequestValidator(IHttpContextAccessor httpContextAccessor, CoreService coreService, IStringLocalizer<BupotChangePasswordRequestValidator> localizer)
        {
            var minLength = 6;
            var noreg = httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "NoReg").Value;

            RuleFor(x => x.OldPassword).NotEmpty()
                .WithMessage(localizer["This field cannot be empty"].Value)
                .Must((data, x) => coreService.ValidHash(noreg, HashType.Bupot, data.OldPassword, Configurations.BupotDefaultPassword))
                .WithMessage(localizer["You input the wrong password"].Value);

            RuleFor(x => x.NewPassword).NotEmpty()
                .WithMessage(localizer["This field cannot be empty"].Value)
                .MinimumLength(minLength)
                .WithMessage(localizer[string.Format("Minimum {0} character long", minLength)].Value);
        }
    }
}