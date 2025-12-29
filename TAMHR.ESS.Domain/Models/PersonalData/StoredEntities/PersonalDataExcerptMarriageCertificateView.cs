using Agit.Common;
using Agit.Common.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace TAMHR.ESS.Domain
{
    [Table("VW_GET_PERSONAL_DATA_EXCERPT_OF_MARRIAGE_CERTIFICATE")]
    public class PersonalDataExcerptMarriageCertificateView : IEntityMarker
    {
        public Guid Id { get; set; }
		public string NoReg { get; set; }
		public Guid CommonAttributeId { get; set; }
		public string Name { get; set; }
		public string SideCode { get; set; }
		public string  SideMariage { get; set; }
		public string Bin { get; set; }
		public string BirthPlace { get; set; }
		public DateTime BirthDate { get; set; }
		public string Nik { get; set; }
		public string Nationality { get; set; }
		public string Religion { get; set; }
		public string Work { get; set; }
		public string Address { get; set; }
		//public string WeddingCertificateNumber { get; set; }
		public string WeddingPlace { get; set; }
		public DateTime WeddingDate { get; set; }
		public Guid? CommonFileId { get; set; }
	}
}
