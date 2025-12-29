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
    public class AnnualLeavePlanningValidator : AbstractDocumentValidator<AnnualLeavePlanningViewModel>
    {
        public AnnualLeavePlanningValidator(AnnualLeavePlanningService annualLeavePlanningService, IStringLocalizer<AnnualLeavePlanningValidator> localizer)
        {
            RuleFor(x => x.Object.AnnualLeavePlanning.YearPeriod).NotEmpty()
                .WithMessage(localizer["Year Period cannot be empty"]);

            RuleFor(x => x.Object.AnnualLeavePlanningDetails.Count).GreaterThanOrEqualTo(1)
                .WithMessage(localizer["Leave Details cannot be empty"].Value);

            RuleFor(x => x.Object.TotalLeave).NotEmpty()
                .WithMessage(localizer["Total leave cannot be empty"].Value);

            RuleForEach(x => x.Object.AnnualLeavePlanningDetails)
                .SetValidator(vm => new AnnualLeavePlanningDetailValidator(annualLeavePlanningService, vm.Object.AnnualLeavePlanningDetails));
        }
    }
}
