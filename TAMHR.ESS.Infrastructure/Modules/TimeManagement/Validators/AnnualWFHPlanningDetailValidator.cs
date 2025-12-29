using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using TAMHR.ESS.Domain;
using System.Linq;
using TAMHR.ESS.Infrastructure.DomainServices;

namespace TAMHR.ESS.Infrastructure.Validators
{
    class AnnualWFHPlanningDetailValidator : AbstractValidator<AnnualWFHPlanningDetailView>
    {
        public AnnualWFHPlanningDetailValidator(AnnualWFHPlanningService annualWFHPlanningService, List<AnnualWFHPlanningDetailView> details)
        {
            RuleFor(i => i.StartDate)
                .NotNull()
                .WithMessage("The start date field is required.");

            RuleFor(i => i.EndDate)
                .NotNull()
                .WithMessage("The end date field is required.");

            RuleFor(i => i.Days)
                .NotNull()
                .WithMessage("The WFH days must be more than 0.");

           
            RuleFor(i => i.StartDate)
                .Must((model, startDate) => WFHDateValids(model, startDate, details))
                .WithMessage((model, startDate) => "The start date cannot intersect with other WFH date");

            RuleFor(i => i.EndDate)
               .Must((model, endDate) => WFHDateValids(model, endDate, details))
               .WithMessage((model, endDate) => "The end date cannot intersect with other WFH date");

            //RuleFor(i => i.StartDate)
            //   .Must(startDate => annualWFHPlanningService.WFHDateValid(startDate, details))
            //   .WithMessage("The start date cannot intersect with other WFH date");

            //RuleFor(i => i.EndDate)
            //    .Must(endDate => annualWFHPlanningService.WFHDateValid(endDate, details))
            //    .WithMessage("The end date cannot intersect with other WFH date");
        }

        private bool WFHDateValids(AnnualWFHPlanningDetailView vdetail, DateTime checkedDate, List<AnnualWFHPlanningDetailView> details)
        {
            int numberOfIntersection = details.Where(a => checkedDate >= a.StartDate && checkedDate <= a.EndDate && a.NoReg == vdetail.NoReg).Count();
            return numberOfIntersection == 1;
        }
    }
}
