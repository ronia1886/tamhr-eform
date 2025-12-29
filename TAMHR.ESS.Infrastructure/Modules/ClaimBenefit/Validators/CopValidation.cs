using FluentValidation;
using Microsoft.Extensions.Localization;
using System.Linq;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Validator;
using TAMHR.ESS.Infrastructure.ViewModels;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class CopValidator : AbstractDocumentValidator<CopViewModel>
    {
        public CopValidator(CoreService coreService, IStringLocalizer<CopValidator> localizer)
        {
            RuleFor(x => x.Object.SubmissionCode).NotEmpty()
                .WithMessage(localizer["COP Type should not be empty"]);
            RuleFor(x => x.Object.Model).NotEmpty()
                .WithMessage(localizer["Model should not be empty"]);
            RuleFor(x => x.Object.TypeCode).NotEmpty()
                .WithMessage(localizer["Type should not be empty"]);
            RuleFor(x => x.Object.ColorCode).NotEmpty()
                .WithMessage(localizer["Color should not be empty"]);
            RuleFor(x => x.Object.PopulationAttachmentPath).NotEmpty()
                .WithMessage(localizer["Identitity Card Attachment should not be empty"]);
          /*  RuleFor(x => x.Object.AttachmentCop)
                .Must((data, i) =>
                {
                    var noreg = data.Requester;
                    var vehicleId = data.Object.VechicleId;
                    var checkClass = coreService.GetVehicleCopByNameTypeCode(noreg, vehicleId).ToList();
                    bool output = true;
               if(checkClass.Count() == 0 && data.Object.AttachmentCop == string.Empty)
                    {
                        output = false;
                    }
                    return output;
                }).WithMessage(localizer["Attachment should not be empty"]);*/
            When(x => x.Object.IsInputUnit.Equals(true), () =>
            {
                RuleFor(x => x.Object.DTRRN).NotEmpty();
                RuleFor(x => x.Object.DTMOCD).NotEmpty();
                RuleFor(x => x.Object.DTMOSX).NotEmpty();
                RuleFor(x => x.Object.DTEXTC).NotEmpty();
                RuleFor(x => x.Object.DTPLOD).NotEmpty();
                //RuleFor(x => x.Object.DTFRNO).NotEmpty();
                //RuleFor(x => x.Object.Engine).NotEmpty();
                RuleFor(x => x.Object.DataUnitColorName).NotEmpty();
            });

            When(x => x.Object.IsEditFrameEngine.Equals(true), () =>
            {
                RuleFor(x => x.Object.DTFRNO).NotEmpty();
                RuleFor(x => x.Object.Engine).NotEmpty();
            });

            When(x => x.Object.IsInputFA.Equals(true), () =>
            {
                RuleFor(x => x.Object.Dealer).NotEmpty();
            });

            When(x => x.Object.IsInputStnk.Equals(true), () =>
            {
                RuleFor(x => x.Object.DoDate).NotEmpty();
                RuleFor(x => x.Object.StnkDate).NotEmpty();
                RuleFor(x => x.Object.LisencePlat).NotEmpty();
            });

            When(x => x.Object.IsKonfirmationPemabyaran.Equals(true), () =>
            {
                RuleFor(x => x.Object.PembayaranAttachmentPath).NotEmpty()
                .WithMessage(localizer["Attachment should not be empty"]);
                //.Must((data, i) =>
                //{
                //    var noreg = data.Requester;
                //    var vehicleId = data.Object.VechicleId;                  
                //    var checkClass = coreService.GetVehicleCopByNameTypeCode(noreg, vehicleId).FirstOrDefault();
                //    var IsUpgrade = coreService.GetVehicleCopByNameTypeCode1(checkClass, vehicleId).FirstOrDefault();
                //    bool output = true;
                //    if (data.Object.PembayaranAttachmentPath == string.Empty /*&& IsUpgrade == true*/)
                //    {
                //        output = false;
                //    }
                //    return output;
                //}).WithMessage(localizer["Attachment should not be empty"]);

            });

            When(x => x.Object.IsInputJasa.Equals(true), () =>
            {
                RuleFor(x => x.Object.Jasa).NotEmpty();
            });

            When(x => x.Object.IsPaymentMethod.Equals(true), () =>
            {
                RuleFor(x => x.Object.PaymentMethod).NotEmpty();
                
            });

            When(x => x.Object.IsAttachmentVld1.Equals(true), () =>
            {
                RuleFor(x => x.Object.AttachmentVld1Path).NotEmpty();
            });
        }
    }
}
