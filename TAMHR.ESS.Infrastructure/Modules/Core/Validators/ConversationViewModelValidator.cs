using TAMHR.ESS.Infrastructure.ViewModels;
using FluentValidation;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class ConversationViewModelValidator : AbstractValidator<ConversationViewModel>
    {
        public ConversationViewModelValidator()
        {
            RuleFor(x => x.Message).NotEmpty();
        }
    }
}
