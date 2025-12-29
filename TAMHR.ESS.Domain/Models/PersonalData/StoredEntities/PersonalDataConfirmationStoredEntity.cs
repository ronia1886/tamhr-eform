using Agit.Common.Attributes;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SP_GENERATE_PERSONAL_DATA_CONFIRMATION", DatabaseObjectType.StoredProcedure)]
    public class PersonalDataConfirmationStoredEntity
    {
        public string Group { get; set; }
        public string Field { get; set; }
        public string Value { get; set; }
    }
}
