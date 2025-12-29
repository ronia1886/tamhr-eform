using Agit.Common;
using Agit.Common.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace TAMHR.ESS.Domain
{
    [Table("VW_GET_PERSONAL_DATA_DOMICILE")]
    public class PersonalDataDomicileView : IEntityMarker
    {
        public Guid Id { get; set; }
		public string NoReg { get; set; }
		public string ProvinceCode { get; set; }
		public string Province { get; set; }
		public string DistrictCode { get; set; }
		public string District { get; set; }
		public string SubDistrictCode { get; set; }
		public string SubDistrict { get; set; }
		public string AdministrativeVillageCode { get; set; }
		public string AdministrativeVillage { get; set; }
		public string PostalCode { get; set; }
		public string RT { get; set; }
		public string RW { get; set; }
		public string AddressDetail { get; set; }
		public string DomicileStatusCode { get; set; }
		public string DomicileStatus { get; set; }
		public string PhoneNumber { get; set; }
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		public bool RowStatus { get; set; }
	}
}
