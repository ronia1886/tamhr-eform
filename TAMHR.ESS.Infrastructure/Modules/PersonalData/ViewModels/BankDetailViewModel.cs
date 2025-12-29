using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;
using TAMHR.ESS.Domain;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class BankDetailViewModel
    {
        public string BankName { get; set; }
        public string BranchBank { get; set; }
        public string LocationBank { get; set; }
        public string KeyBank { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string SupportingAttachmentPath { get; set; }
        public string Remarks { get; set; }
    }
}
