using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_M_LETTER_TEMPLATE")]
    public partial class LetterTemplate : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string LetterKey { get; set; }
        public string LetterContent { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }

        public static LetterTemplate Create(string title, string letterKey, string letterContent, string description = null)
        {
            var letterTemplate = new LetterTemplate
            {
                Title = title,
                LetterKey = letterKey,
                LetterContent = letterContent,
                Description = description
            };

            return letterTemplate;
        }
    }
}
