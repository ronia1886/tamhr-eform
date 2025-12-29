using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;
using TAMHR.ESS.Domain;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class AddressViewModel
    {
        public string FamilyCardNumber { get; set; }
        public string FamilyCardPath { get; set; }
        public string PopulationNumber { get; set; }
        public string PopulationPath { get; set; }
        public string Provice { get; set; }
        public string City { get; set; }
        public string DistrictCode { get; set; }
        public string PostalCode { get; set; }
        public string SubDistrictCode { get; set; }
        public string CompleteAddress { get; set; }
        public string RT { get; set; }
        public string RW { get; set; }
        public string Remarks { get; set; }
        public string Nik { get; set; }
        public string KKNumber { get; set; }
        public string Address { get; set; }
        public string NoReg { get; set; }
    }
}
