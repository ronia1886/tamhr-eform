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
    public class AnnualBDJKPlanningValidator : AbstractDocumentValidator<AnnualBDJKPlanningViewModel>
    {
        public AnnualBDJKPlanningValidator(AnnualBDJKPlanningService annualBDJKPlanningService, IStringLocalizer<AnnualBDJKPlanningValidator> localizer)
        {
            RuleFor(x => x.Object.AnnualBDJKPlanning.YearPeriod).NotEmpty()
                .WithMessage(localizer["Year Period cannot be empty"]);

            RuleFor(x => x.Object.AnnualBDJKPlanningDetails.Count).GreaterThanOrEqualTo(1)
                .WithMessage(localizer["BDJK Details cannot be empty"].Value);

            RuleForEach(x => x.Object.AnnualBDJKPlanningDetails)
                .SetValidator(vm => new AnnualBDJKPlanningDetailValidator(annualBDJKPlanningService, vm.Object.AnnualBDJKPlanningDetails));
        }
    }
}
