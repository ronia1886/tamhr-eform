using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_M_PERSONAL_DATA_DOMICILE_ADDRESS")]
    public partial class PersonalDataDomicileAddress : IEntityBase<Guid>
    {
        [Key]
        [Column(TypeName = "uniqueidentifier")]
        public Guid Id { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string NoReg { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string ProvinceCode { get; set; }
        
        [Column(TypeName = "varchar(20)")]
        public string DistrictCode { get; set; }
        
        [Column(TypeName = "varchar(20)")]
        public string SubDistrictCode { get; set; }
        
        [Column(TypeName = "varchar(20)")]
        public string AdministrativeVillageCode { get; set; }
        
        [Column(TypeName = "varchar(20)")]
        public string PostalCode { get; set; }
        
        [Column(TypeName = "varchar(10)")]
        public string RT { get; set; }
        
        [Column(TypeName = "varchar(10)")]
        public string RW { get; set; }

        [Column(TypeName = "varchar(450)")]
        public string AddressDetail { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string DomicileStatusCode { get; set; }

        [Column(TypeName = "date")]
        public DateTime StartDate { get; set; }
        
        [Column(TypeName = "date")]
        public DateTime EndDate { get; set; }
        
        [Column(TypeName = "varchar(20)")]
        public string CreatedBy { get; set; }
        
        [Column(TypeName = "datetime")]
        public DateTime CreatedOn { get; set; }
        
        [Column(TypeName = "varchar(20)")]
        public string ModifiedBy { get; set; }
        
        [Column(TypeName = "datetime")]
        public DateTime? ModifiedOn { get; set; }
        
        [Column(TypeName = "bit")]
        public bool RowStatus { get; set; }

        public string PhoneNumber { get; set; }
 
    }
}
