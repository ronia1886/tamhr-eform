using System.Linq;
using TAMHR.ESS.Infrastructure.Validator;
using TAMHR.ESS.Infrastructure.ViewModels;
using FluentValidation;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class ComplaintRequestsValidator : AbstractDocumentValidator<ComplaintRequestViewModel[]>
    {
        public ComplaintRequestsValidator()
        {
            RuleFor(x => x.Object).Must(x => x.All(y => !string.IsNullOrEmpty(y.Expectation))).WithMessage("Expectation cannot be empty");
            RuleFor(x => x.Object).Must(x => x.All(y => !string.IsNullOrEmpty(y.ComplaintCode))).WithMessage("Complaint code cannot be empty");
            RuleFor(x => x.Object).Must(x => x.All(y => !string.IsNullOrEmpty(y.ComplaintDetail))).WithMessage("Complaint detail cannot be empty");
            RuleFor(x => x.Object).Must(x => x.All(y => !string.IsNullOrEmpty(y.ComplaintSubCode))).WithMessage("Complaint sub code cannot be empty");
        }
    }
}
