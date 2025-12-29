using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using TAMHR.ESS.Domain;
using System.Linq;
using TAMHR.ESS.Infrastructure.DomainServices;

namespace TAMHR.ESS.Infrastructure.Validators
{
    class AnnualBDJKPlanningDetailValidator : AbstractValidator<AnnualBDJKPlanningDetailView>
    {
        public AnnualBDJKPlanningDetailValidator(AnnualBDJKPlanningService annualBDJKPlanningService, List<AnnualBDJKPlanningDetailView> details)
        {
            RuleFor(i => i.StartDate)
                .NotNull()
                .WithMessage("The start date field is required.");

            RuleFor(i => i.EndDate)
                .NotNull()
                .WithMessage("The end date field is required.");

            RuleFor(i => i.Days)
                .NotNull()
                .WithMessage("The BDJK days must be more than 0.");

           
            RuleFor(i => i.StartDate)
                .Must((model, startDate) => BDJKDateValids(model, startDate, details))
                .WithMessage((model, startDate) => "The start date cannot intersect with other BDJK date");

            RuleFor(i => i.EndDate)
               .Must((model, endDate) => BDJKDateValids(model, endDate, details))
               .WithMessage((model, endDate) => "The end date cannot intersect with other BDJK date");

            //RuleFor(i => i.StartDate)
            //   .Must(startDate => annualBDJKPlanningService.BDJKDateValid(startDate, details))
            //   .WithMessage("The start date cannot intersect with other BDJK date");

            //RuleFor(i => i.EndDate)
            //    .Must(endDate => annualBDJKPlanningService.BDJKDateValid(endDate, details))
            //    .WithMessage("The end date cannot intersect with other BDJK date");
        }

        private bool BDJKDateValids(AnnualBDJKPlanningDetailView vdetail, DateTime checkedDate, List<AnnualBDJKPlanningDetailView> details)
        {
            int numberOfIntersection = details.Where(a => checkedDate >= a.StartDate && checkedDate <= a.EndDate && a.NoReg == vdetail.NoReg).Count();
            return numberOfIntersection == 1;
        }
    }
}
