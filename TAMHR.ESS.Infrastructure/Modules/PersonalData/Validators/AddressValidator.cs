using FluentValidation;
using TAMHR.ESS.Infrastructure.Validator;
using TAMHR.ESS.Infrastructure.ViewModels;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class AddressValidator : AbstractDocumentValidator<AddressViewModel>
    {
        public AddressValidator()
        {
            RuleFor(x => x.Object.FamilyCardNumber).NotEmpty();
            RuleFor(x => x.Object.FamilyCardPath).NotEmpty();
            RuleFor(x => x.Object.PopulationNumber).NotEmpty().Matches(@"^\d{16}$").WithMessage("Identity Card Number must consist of 16 digits.");
            RuleFor(x => x.Object.PopulationPath).NotEmpty();
            RuleFor(x => x.Object.Provice).NotEmpty();
            RuleFor(x => x.Object.City).NotEmpty();
            RuleFor(x => x.Object.CompleteAddress).NotEmpty();
            RuleFor(x => x.Object.DistrictCode).NotEmpty();
            RuleFor(x => x.Object.SubDistrictCode).NotEmpty();
            RuleFor(x => x.Object.PostalCode).NotEmpty();
        }
    }
}
