using System;
using Agit.Common.Attributes;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SF_GET_HR_HIERARCHIES", DatabaseObjectType.TableValued)]
    public class EmployeeHRHierarchiesStoredEntity
    {
        public string NoRegSH { get; set; }
        public string NameSH { get; set; }
        public string PostCodeSH { get; set; }
        public string PostNameSH { get; set; }
        public string JobCodeSH { get; set; }
        public string JobNameSH { get; set; }
        public string NoRegDpH { get; set; }
        public string NameDpH { get; set; }
        public string PostCodeDpH { get; set; }
        public string PostNameDpH { get; set; }
        public string JobCodeDpH { get; set; }
        public string JobNameDpH { get; set; }
        public string NoRegDH { get; set; }
        public string NameDH { get; set; }
        public string PostCodeDH { get; set; }
        public string PostNameDH { get; set; }
        public string JobCodeDH { get; set; }
        public string JobNameDH { get; set; }
    }
}
