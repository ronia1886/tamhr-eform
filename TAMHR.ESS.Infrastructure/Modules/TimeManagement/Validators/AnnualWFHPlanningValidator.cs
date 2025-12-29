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
    public class AnnualWFHPlanningValidator : AbstractDocumentValidator<AnnualWFHPlanningViewModel>
    {
        public AnnualWFHPlanningValidator(AnnualWFHPlanningService annualWFHPlanningService, IStringLocalizer<AnnualWFHPlanningValidator> localizer)
        {
            RuleFor(x => x.Object.AnnualWFHPlanning.YearPeriod).NotEmpty()
                .WithMessage(localizer["Year Period cannot be empty"]);

            RuleFor(x => x.Object.AnnualWFHPlanningDetails.Count).GreaterThanOrEqualTo(1)
                .WithMessage(localizer["WFH Details cannot be empty"].Value);

            RuleForEach(x => x.Object.AnnualWFHPlanningDetails)
                .SetValidator(vm => new AnnualWFHPlanningDetailValidator(annualWFHPlanningService, vm.Object.AnnualWFHPlanningDetails));
        }
    }
}
