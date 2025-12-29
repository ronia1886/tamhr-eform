using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_M_PERSONAL_DATA_DOCUMENT")]
    public partial class PersonalDataDocument : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string NoReg { get; set; }

        [NotMapped]
        public string Name { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string DocumentTypeCode { get; set; }

        [Column(TypeName = "varchar(150)")]
        public string DocumentValue { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string CreatedBy { get; set; }

        public DateTime CreatedOn { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string ModifiedBy { get; set; }

        public DateTime? ModifiedOn { get; set; }

        public bool RowStatus { get; set; }

        public static PersonalDataDocument Create(PersonalDataDocument personalData, User user)
        {
            var output = new PersonalDataDocument
            {
                Id = personalData.Id,
                NoReg = personalData.NoReg,
                Name = user.Name,
                DocumentTypeCode = personalData.DocumentTypeCode,
                DocumentValue = personalData.DocumentValue,
                StartDate = personalData.StartDate,
                EndDate = personalData.EndDate,
                CreatedBy = personalData.CreatedBy,
                CreatedOn = personalData.CreatedOn,
                ModifiedBy = personalData.ModifiedBy,
                ModifiedOn = personalData.ModifiedOn,
                RowStatus = personalData.RowStatus
            };

            return output;
        }
    }
}
