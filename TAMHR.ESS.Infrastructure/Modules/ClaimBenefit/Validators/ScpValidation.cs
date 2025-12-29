using FluentValidation;
using TAMHR.ESS.Infrastructure.Validator;
using TAMHR.ESS.Infrastructure.ViewModels;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class ScpValidator : AbstractDocumentValidator<ScpViewModel>
    {
        public ScpValidator()
        {
            RuleFor(x => x.Object.PopulationNumber).NotEmpty().Matches(@"^\d{16}$").WithMessage("Identity Card Number must consist of 16 digits.");
            RuleFor(x => x.Object.TypeCode).NotEmpty();
            RuleFor(x => x.Object.PurchaseTypeCode).NotEmpty();
            RuleFor(x => x.Object.ColorCode).NotEmpty();
            RuleFor(x => x.Object.Model).NotEmpty();
            RuleFor(x => x.Object.PopulationAttachmentPath).NotEmpty();
            When(x => x.Object.PurchaseTypeCode.Equals("Loan"), () =>
            {
                RuleFor(x => x.Object.DocumentNumber).NotEmpty();
            });

            When(x => x.Object.BuyFor.Equals("My Family"), () =>
            {
                RuleFor(x => x.Object.FamilyAttachmentPath).NotEmpty();
            });

            When(x => x.Object.IsInputUnit.Equals(true), () =>
            {
                RuleFor(x => x.Object.DTRRN).NotEmpty();
                RuleFor(x => x.Object.DTMOCD).NotEmpty();
                RuleFor(x => x.Object.DTMOSX).NotEmpty();
                RuleFor(x => x.Object.DTEXTC).NotEmpty();
                RuleFor(x => x.Object.DTPLOD).NotEmpty();
            });

            When(x => x.Object.IsInputStnk.Equals(true), () =>
            {
                RuleFor(x => x.Object.DoDate).NotEmpty();
                RuleFor(x => x.Object.StnkDate).NotEmpty();
            });

            When(x => x.Object.IsKonfirmationPemabyaran.Equals(true), () =>
            {
                RuleFor(x => x.Object.PembayaranAttachmentPath).NotEmpty();
            });

            When(x => x.Object.IsInputJasa.Equals(true), () =>
            {
                RuleFor(x => x.Object.Jasa).NotEmpty();
                RuleFor(x => x.Object.PaymentMethod).NotEmpty();
            });

            When(x => x.Object.IsEditFrameEngine.Equals(true), () =>
            {
                RuleFor(x => x.Object.DTFRNO).NotEmpty();
            });

            When(x => x.Object.IsInputFA.Equals(true), () =>
            {
                RuleFor(x => x.Object.Dealer).NotEmpty();
            });
        }
    }
}
