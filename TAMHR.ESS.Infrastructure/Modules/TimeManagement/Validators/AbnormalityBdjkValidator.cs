using Agit.Common.Utility;
using FluentValidation;
using Microsoft.Extensions.Localization;
using System;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Validator;
using TAMHR.ESS.Infrastructure.ViewModels;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class AbnormalityBdjkValidator : AbstractValidator<AbnormalityBdjk>
    {
        public AbnormalityBdjkValidator(ConfigService configService, IStringLocalizer<AbnormalityBdjk> localizer)
        {
            var now = DateTime.Now.Date;

            When(x => x.Id == default(Guid), () =>
            {
                RuleFor(x => x.WorkingDate).LessThanOrEqualTo(now)
                    .WithMessage(string.Format(localizer["Overtime date must be less than or equal to {0:dd/MM/yyyy}"].Value, now));
            });

            RuleFor(x => x.WorkingTimeIn).NotEmpty()
                .WithMessage(localizer["Proxy in hour must be filled"].Value)
                .Must((data, val) => (data.WorkingTimeOut > data.WorkingTimeIn && data.WorkingTimeIn > data.WorkingDate))
                .WithMessage(localizer["Proxy in hour must be greater than overtime date"].Value);

            RuleFor(x => x.WorkingTimeOut).NotEmpty()
                .WithMessage(localizer["Proxy out hour must be filled"].Value)
                .Must((data, val) => (data.WorkingTimeOut > data.WorkingTimeIn))
                .WithMessage(localizer["Proxy out hour must be greater than overtime hour in"].Value);

            RuleFor(x => x.ActivityCode).NotEmpty()
               .WithMessage(localizer["Activity must be filled"].Value);

            RuleFor(x => x.BDJKCode).NotEmpty()
              .WithMessage(localizer["BDJK Code must be filled"].Value);

            RuleFor(x => x.NoReg).NotEmpty()
             .WithMessage(localizer["Employee must be filled"].Value);

            RuleFor(x => x.BDJKReason).NotEmpty()
                .WithMessage(localizer["Overtime reason must be filled"].Value)
                .MaximumLength(ApplicationConstants.CommonMediumInputMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.CommonMediumInputMaxLength));
        }
    }
}
