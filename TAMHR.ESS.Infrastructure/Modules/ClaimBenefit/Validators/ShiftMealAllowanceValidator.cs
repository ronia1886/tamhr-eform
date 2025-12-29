using FluentValidation;
using TAMHR.ESS.Infrastructure.Validator;
using TAMHR.ESS.Infrastructure.ViewModels;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class ShiftMealAllowanceValidator : AbstractDocumentValidator<ShiftMealAllowanceViewModel>
    {
        public ShiftMealAllowanceValidator()
        {
            RuleFor(x => x.Object.Division).NotEmpty();
            RuleFor(x => x.Object.Department).NotEmpty();
            RuleFor(x => x.Object.StartPeriod).NotEmpty();
            RuleFor(x => x.Object.EndPeriod).NotEmpty();
        }
    }
}
