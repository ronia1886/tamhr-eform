using FluentValidation;
using TAMHR.ESS.Infrastructure.Validator;
using TAMHR.ESS.Infrastructure.ViewModels;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class MealAllowanceValidator : AbstractDocumentValidator<MealAllowanceViewModel>
    {
        public MealAllowanceValidator()
        {
            //RuleFor(x => x.Object.StartHour).NotEmpty();
            //RuleFor(x => x.Object.EndHour).NotEmpty();
            //RuleFor(x => x.Object.AmountDays).NotEmpty();
            //RuleFor(x => x.Object.Date).NotEmpty();
        }
    }
}
