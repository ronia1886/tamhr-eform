using FluentValidation;
using System;
using System.Collections.Generic;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.ViewModels;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class GenericUploadViewModel
    {
        public Guid Id { get; set; }
        public Guid DocumentApprovalId { get; set; }
        public string FormKey { get; set; }
        public string AttachmentFilePath { get; set; }
        public IEnumerable<DocumentApprovalFile> Attachments { get; set; }
    }
}
