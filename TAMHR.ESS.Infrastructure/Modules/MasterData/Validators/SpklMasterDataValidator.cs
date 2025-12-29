using System;
using Microsoft.Extensions.Localization;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;
using FluentValidation;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class SpklMasterDataValidator : AbstractValidator<TimeManagementSpkl>
    {
        public SpklMasterDataValidator(ConfigService configService, IStringLocalizer<SpklMasterDataValidator> localizer)
        {
            var now = DateTime.Now.Date;

            When(x => x.Id == default(Guid), () =>
            {
                RuleFor(x => x.OvertimeDate).LessThanOrEqualTo(now)
                    .WithMessage(string.Format(localizer["Overtime date must be less than or equal to {0:dd/MM/yyyy}"].Value, now));

                RuleFor(x => x.NoReg).NotEmpty()
                    .WithMessage(localizer["Employee must be selected"].Value);
            });

            RuleFor(x => x.OvertimeInPlan).NotEmpty()
                .WithMessage(localizer["Overtime hour in plan must be filled"].Value)
                .Must((data, val) => (data.OvertimeDate == data.OvertimeInPlan.Date && data.OvertimeOutPlan > data.OvertimeInPlan))
                .WithMessage(localizer["Overtime plan hour out must be greater than overtime plan hour in"].Value);

            RuleFor(x => x.OvertimeInAdjust).NotEmpty()
                .WithMessage(localizer["Overtime hour in adjust must be filled"].Value)
                .Must((data, val) => (data.OvertimeDate == data.OvertimeInAdjust.Date && data.OvertimeOutAdjust > data.OvertimeInAdjust))
                .WithMessage(localizer["Overtime adjust hour out must be greater than overtime adjust hour in"].Value);

            RuleFor(x => x.OvertimeBreakPlan).GreaterThanOrEqualTo(0)
                .WithMessage(localizer["Value must be greater than or equal to 0"].Value)
                .LessThanOrEqualTo(1000)
                .WithMessage(localizer["Value must be less than or equal to 1000"].Value);

            RuleFor(x => x.OvertimeBreakAdjust).GreaterThanOrEqualTo(0)
                .WithMessage(localizer["Value must be greater than or equal to 0"].Value)
                .LessThanOrEqualTo(1000)
                .WithMessage(localizer["Value must be less than or equal to 1000"].Value);

            RuleFor(x => x.DurationPlan).GreaterThanOrEqualTo(0)
                .WithMessage(localizer["Value must be greater than or equal to 0"].Value)
                .LessThanOrEqualTo(24)
                .WithMessage(localizer["Value must be less than or equal to 24"].Value);

            RuleFor(x => x.DurationAdjust).GreaterThanOrEqualTo(0)
                .WithMessage(localizer["Value must be greater than or equal to 0"].Value)
                .LessThanOrEqualTo(24)
                .WithMessage(localizer["Value must be less than or equal to 24"].Value);

            RuleFor(x => x.OvertimeCategoryCode).NotEmpty()
                .WithMessage(localizer["Please select category"].Value)
                .Must(x => configService.ValueInCategories(x, "CategorySPKL"))
                .WithMessage(localizer["Value is not in category"].Value);

            RuleFor(x => x.OvertimeReason).NotEmpty()
                .WithMessage(localizer["Overtime reason must be filled"].Value)
                .MaximumLength(ApplicationConstants.CommonMediumInputMaxLength)
                .WithMessage(string.Format(localizer["Maximum length is {0}"].Value, ApplicationConstants.CommonMediumInputMaxLength));
        }
    }
}
