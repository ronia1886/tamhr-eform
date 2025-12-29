using System.Linq;
using TAMHR.ESS.Infrastructure.Validator;
using TAMHR.ESS.Infrastructure.ViewModels;
using FluentValidation;
using Microsoft.Extensions.Localization;
using TAMHR.ESS.Infrastructure.DomainServices;
using System;

namespace TAMHR.ESS.Infrastructure.Validators
{
    public class TerminationValidator : AbstractDocumentValidator<TerminationViewModel>
    {
        public TerminationValidator(TerminationService terminationService, IStringLocalizer<TerminationValidator> localizer)
        {
            //RuleFor(x => x.Object.NoReg)
            //.NotEmpty()
            //.WithMessage(localizer["Employee cannot be empty"]);

            //RuleFor(x => x.Object.EndDate)
            //.NotEmpty()
            //.WithMessage(localizer["End date cannot be empty"]);

            //RuleFor(x => x.Object.EndDate)
            //.Must((item, x) =>
            //{
            //    if(item.Object.EndDate == null)
            //    {
            //        return true;
            //    }
            //    else
            //    {
            //        bool offDate = terminationService.GetListWorkSchEmp(item.Object.NoReg, item.Object.EndDate, item.Object.EndDate);
            //        return offDate ? false : true;
            //    }
            //})
            //.WithMessage(localizer["End date cannot be on weekends or holidays"]);

            //RuleFor(x => x.Object.Reason).NotEmpty()
            //.WithMessage(localizer["Reason cannot be empty"]);

            //RuleFor(x => x.Object.TerminationTypeId).NotEmpty()
            //.WithMessage(localizer["Termination Type cannot be empty"]);

            //RuleFor(x => x.Object.AttachmentCommonFile).NotEmpty()
            //.WithMessage(localizer["Attachment cannot be empty"]);

            //RuleFor(x => x.Object.AttachmentCommonFile)
            //.Must((item, x) =>
            //{
            //    if (item.Attachments == null)
            //    {
            //        return true;
            //    }
            //    else
            //    {
            //        bool sizeValidation = terminationService.AttachmentValidation(item, "filesize");
            //        return sizeValidation;
            //    }
            //})
            //.WithMessage(localizer["Attachment size more than 5 Mb, maximum size is 5 Mb"]);

            //RuleFor(x => x.Object.AttachmentCommonFile)
            //.Must((item, x) =>
            //{
            //    if (item.Attachments == null)
            //    {
            //        return true;
            //    }
            //    else
            //    {
            //        bool typeValidation = terminationService.AttachmentValidation(item, "filetype");
            //        return typeValidation;
            //    }
            //})
            //.WithMessage(localizer["Attachment is invalid, allowed extensions are: .pdf"]);
        }
    }
}
