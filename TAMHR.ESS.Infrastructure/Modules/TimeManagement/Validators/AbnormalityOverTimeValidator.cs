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
    public class AbnormalityOverTimeValidator : AbstractValidator<AbnormalityOverTimeView>
    {
        public AbnormalityOverTimeValidator(ConfigService configService, IStringLocalizer<AbnormalityOverTimeValidator> localizer)
        {
            var now = DateTime.Now.Date;

            When(x => x.Id == default(Guid), () =>
            {
                RuleFor(x => x.OvertimeDate).LessThanOrEqualTo(now)
                    .WithMessage(string.Format(localizer["Overtime date must be less than or equal to {0:dd/MM/yyyy}"].Value, now));
            });

            RuleFor(x => x.ProxyIn).GreaterThan(DateTime.MinValue).NotNull().Must(BeAValidDate)
                .WithMessage(localizer["Proxy in hour must be filled"].Value)
                .Must((data, val) => (data.ProxyOut > data.ProxyIn.Date && data.ProxyIn > data.OvertimeDate))
                .WithMessage(localizer["Proxy in hour must be greater than overtime date"].Value);

            RuleFor(x => x.ProxyOut).GreaterThan(DateTime.MinValue).NotNull().Must(BeAValidDate)
                .WithMessage(localizer["Proxy out hour must be filled"].Value)
                .Must((data, val) => (data.ProxyOut > data.ProxyIn))
                .WithMessage(localizer["Proxy out hour must be greater than overtime hour in"].Value);

            RuleFor(x => x.OvertimeIn).GreaterThan(DateTime.MinValue)
                .NotNull().Must(BeAValidDate)
                .WithMessage(localizer["Overtime in hour must be filled"].Value)
                .Must((data, val) => (data.OvertimeOut > data.OvertimeIn.Date && data.OvertimeIn > data.OvertimeDate))
                .WithMessage(localizer["Overtime in hour must be greater than overtime date"].Value);

            RuleFor(x => x.OvertimeOut).GreaterThan(DateTime.MinValue)
                .NotNull().Must(BeAValidDate)
                .WithMessage(localizer["Overtime out hour must be filled"].Value)
                .Must((data, val) => (data.OvertimeOut > data.OvertimeIn))
                .WithMessage(localizer["Overtime out hour must be greater than overtime hour in"].Value);

            RuleFor(x => x.OvertimeBreak).GreaterThanOrEqualTo(0)
                .WithMessage(localizer["Value must be greater than or equal to 0"].Value)
                .LessThanOrEqualTo(1000)
                .WithMessage(localizer["Value must be less than or equal to 1000"].Value);

            RuleFor(x => x.Duration).GreaterThan(0)
               .WithMessage(localizer["Value must be greater than or equal to 0"].Value)
               .LessThanOrEqualTo(1000)
               .WithMessage(localizer["Value must be less than or equal to 1000"].Value);

            RuleFor(x => x.OvertimeCategoryCode).NotEmpty()
                .WithMessage(localizer["Please select category"].Value)
                .Must(x => configService.ValueInCategories(x, "CategorySPKL"))
                .WithMessage(localizer["Value is not in category"].Value);

            RuleFor(x => x.OvertimeReason).NotEmpty()
                .WithMessage(localizer["Overtime reason must be filled"].Value)
                .MaximumLength(ApplicationConstants.CommonMediumInputMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.CommonMediumInputMaxLength));
        }
        private bool BeAValidDate(DateTime date)
        {
            return !(date.Hour == 0 && date.Minute == 0);
        }
    }
}
