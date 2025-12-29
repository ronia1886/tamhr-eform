using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_M_PERSONAL_DATA_OTHER_INFORMATION")]
    public partial class PersonalDataOtherInformation : IEntityBase<Guid>
    {
        [Key]
        [Column(TypeName = "uniqueidentifier")]
        public Guid Id { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string NoReg { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string PhoneNumber1 { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string PhoneNumber2 { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string PhoneNumber3 { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string HomePhoneNumber { get; set; }

        [Column(TypeName = "varchar(450)")]
        public string PersonalEmail { get; set; }

        [Column(TypeName = "varchar(450)")]
        public string TransportationCodes { get; set; }

        [Column(TypeName = "varchar(450)")]
        public string Hobbies { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string EmergencyCallRelationshipCode { get; set; }

        [Column(TypeName = "varchar(150)")]
        public string EmergencyCallName { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string EmergencyCallPhoneNumber { get; set; }

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
    }
}
