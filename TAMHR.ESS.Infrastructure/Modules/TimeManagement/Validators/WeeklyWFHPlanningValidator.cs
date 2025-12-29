using System;
using FluentValidation;
using Microsoft.Extensions.Localization;
using System.Collections.Generic;
using System.Text;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Validator;
using TAMHR.ESS.Infrastructure.ViewModels;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class WeeklyWFHPlanningValidator : AbstractDocumentValidator<WeeklyWFHPlanningViewModel>
    {
        public WeeklyWFHPlanningValidator(WeeklyWFHPlanningService weeklyWFHPlanningService, IStringLocalizer<WeeklyWFHPlanningValidator> localizer)
        {
            RuleFor(x => x.Object.WeeklyWFHPlanning.StartDate).NotEmpty()
                .WithMessage(localizer["Start Date cannot be empty"]);

            RuleFor(x => x.Object.WeeklyWFHPlanning.EndDate).NotEmpty()
                .WithMessage(localizer["End Date cannot be empty"]);

            RuleFor(x => x.Object.WeeklyWFHPlanningDetails.Count).GreaterThanOrEqualTo(1)
                .WithMessage(localizer["WFH Details cannot be empty"].Value);

            RuleForEach(x => x.Object.WeeklyWFHPlanningDetails)
                .SetValidator(vm => new WeeklyWFHPlanningDetailValidator(weeklyWFHPlanningService, vm.Object.WeeklyWFHPlanningDetails));
        }
    }
}
