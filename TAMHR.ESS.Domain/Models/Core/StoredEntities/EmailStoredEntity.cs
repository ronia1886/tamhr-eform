using Agit.Common.Attributes;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SF_GET_EMAILS_BY", DatabaseObjectType.TableValued)]
    public partial class EmailStoredEntity
    {
        public string NoReg { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
    }
}
