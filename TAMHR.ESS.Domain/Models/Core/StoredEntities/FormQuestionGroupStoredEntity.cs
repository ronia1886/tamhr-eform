using Agit.Common;
using Agit.Common.Attributes;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SF_GET_FORM_QUESTION_GROUP_ANSWER", DatabaseObjectType.TableValued)]
    public partial class FormQuestionGroupStoredEntity : IEntityMarker
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string QuestionTypeCode { get; set; }
        public string GroupAnswer { get; set; }
        public string ParentTitle { get; set; }
        public bool Checked { get; set; }
        public int OrderSequence { get; set; }
    }
}
