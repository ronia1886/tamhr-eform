using FluentValidation;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Validator;
using TAMHR.ESS.Infrastructure.ViewModels;

namespace TAMHR.ESS.Infrastructure.Validators
{
    
    public class ReimbursementPointValidator : AbstractValidator<ReimbursementPointViewModel>
    {
        public ReimbursementPointValidator(ApprovalService approvalService)
        {
            RuleFor(x => x.TotalClaim).GreaterThanOrEqualTo(0);
            RuleFor(x => x.TotalCompanyClaim).GreaterThanOrEqualTo(0).Must((x, i) =>
            {
                var obj = approvalService.GetDocumentRequestDetail<ReimbursementViewModel>(x.DocumentApprovalId);

                return x.TotalCompanyClaim <= obj.Cost - x.TotalClaim;
            });
        }
    }
}
