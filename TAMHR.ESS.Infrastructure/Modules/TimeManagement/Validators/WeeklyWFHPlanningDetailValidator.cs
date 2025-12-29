using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using TAMHR.ESS.Domain;
using System.Linq;
using TAMHR.ESS.Infrastructure.DomainServices;

namespace TAMHR.ESS.Infrastructure.Validators
{
    class WeeklyWFHPlanningDetailValidator : AbstractValidator<WeeklyWFHPlanningDetailView>
    {
        public WeeklyWFHPlanningDetailValidator(WeeklyWFHPlanningService weeklyWFHPlanningService, List<WeeklyWFHPlanningDetailView> details)
        {
            RuleFor(i => i.WorkingDate)
                .NotNull()
                .WithMessage("The working date field is required.");
           
            //RuleFor(i => i.WorkingDate)
            //    .Must((model, startDate) => WFHDateValids(model, startDate, details))
            //    .WithMessage((model, startDate) => "The start date cannot intersect with other WFH date");

            //RuleFor(i => i.StartDate)
            //   .Must(startDate => weeklyWFHPlanningService.WFHDateValid(startDate, details))
            //   .WithMessage("The start date cannot intersect with other WFH date");

            //RuleFor(i => i.EndDate)
            //    .Must(endDate => weeklyWFHPlanningService.WFHDateValid(endDate, details))
            //    .WithMessage("The end date cannot intersect with other WFH date");
        }

        //private bool WFHDateValids(WeeklyWFHPlanningDetailView vdetail, DateTime checkedDate, List<WeeklyWFHPlanningDetailView> details)
        //{
        //    int numberOfIntersection = details.Where(a => checkedDate >= a.StartDate && checkedDate <= a.EndDate && a.NoReg == vdetail.NoReg).Count();
        //    return numberOfIntersection == 1;
        //}
    }
}
