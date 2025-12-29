using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_M_LANGUAGE")]
    public class Language : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public string CultureCode { get; set; }
        public string TranslateKey { get; set; }
        public string TranslateValue { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }

        public static Language Create(string cultureCode, string translateKey, string translateValue)
        {
            var language = new Language
            {
                CultureCode = cultureCode,
                TranslateKey = translateKey,
                TranslateValue = translateValue
            };

            return language;
        }
    }
}
