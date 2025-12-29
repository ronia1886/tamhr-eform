using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using TAMHR.ESS.Domain;
using System.Linq;
using TAMHR.ESS.Infrastructure.DomainServices;

namespace TAMHR.ESS.Infrastructure.Validators
{
    class AnnualLeavePlanningDetailValidator : AbstractValidator<AnnualLeavePlanningDetailView>
    {
        public AnnualLeavePlanningDetailValidator(AnnualLeavePlanningService annualLeavePlanningService, List<AnnualLeavePlanningDetailView> details)
        {
            RuleFor(i => i.StartDate)
                .NotNull()
                .WithMessage("The start date field is required.");

            RuleFor(i => i.EndDate)
                .NotNull()
                .WithMessage("The end date field is required.");

            RuleFor(i => i.Days)
                .NotNull()
                .WithMessage("The leave days must be more than 0.");

            RuleFor(i => i.AbsentId)
                .NotNull()
                .WithMessage("The reason must be selected.");

            RuleFor(i => i.StartDate)
                .Must(startDate => annualLeavePlanningService.LeaveDateValid(startDate, details))
                .WithMessage("The start date cannot intersect with other leave date");

            RuleFor(i => i.EndDate)
                .Must(endDate => annualLeavePlanningService.LeaveDateValid(endDate, details))
                .WithMessage("The end date cannot intersect with other leave date");
        }
    }
}
