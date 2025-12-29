using System;
using FluentValidation;
using TAMHR.ESS.Infrastructure.Validator;
using TAMHR.ESS.Infrastructure.ViewModels;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class CopLoanBpkbValidation : AbstractDocumentValidator<ReturnBpkbCOPViewModel>
    {
        private static readonly int MaxReturnDate = 7;
        private DateTime GetNextWorkDay(DateTime from, int day)
        {
            DateTime currentDate = from.AddDays(-1);
            int addedDate = 0;
            while (addedDate < MaxReturnDate)
            {
                currentDate = currentDate.AddDays(1);
                if (currentDate.DayOfWeek == DayOfWeek.Saturday || currentDate.DayOfWeek == DayOfWeek.Sunday)
                {
                    continue;
                }
                addedDate++;
            }
            return currentDate;
        }
        public CopLoanBpkbValidation()
        {
            RuleFor(x => x.Object.PoliceNumber).NotEmpty();
            //RuleFor(x => x.Object.Type).NotEmpty();
            //RuleFor(x => x.Object.Model).NotEmpty();
            //RuleFor(x => x.Object.ProductionYear).NotEmpty();
            //RuleFor(x => x.Object.Color).NotEmpty();
            //RuleFor(x => x.Object.FrameNumber).NotEmpty();
            //RuleFor(x => x.Object.MachineNumber).NotEmpty();
            //RuleFor(x => x.Object.Name).NotEmpty();
            //RuleFor(x => x.Object.Address).NotEmpty();
            When(x => x.Object.IsOtherNecessity.Equals(true), () =>
            {
                RuleFor(x => x.Object.OtherNecessity).NotEmpty();
            });
            When(x => x.Object.IsOtherNecessity.Equals(false), () =>
            {
                RuleFor(x => x.Object.NecessityCode).NotEmpty();
            });
            //RuleFor(x => x.Object.Type).NotEmpty();
            RuleFor(x => x.Object.LoanDate).NotEmpty();
            RuleFor(x => x.Object.ReturnDate)
                .NotEmpty()
                .GreaterThanOrEqualTo(x => x.Object.LoanDate)
                .LessThanOrEqualTo(x => GetNextWorkDay(x.Object.LoanDate.Value, MaxReturnDate));
        }
    }
}
