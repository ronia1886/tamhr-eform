using Agit.Common.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace TAMHR.ESS.Domain
{ 
    [DatabaseObject("SF_GET_AYO_SEKOLAH", DatabaseObjectType.TableValued)]
    public class AyoSekolahStoredEntity
    {
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        public string Name { get; set; }
        public string PostCode { get; set; }
        public string PostName { get; set; }
        public string JobCode { get; set; }
        public string JobName { get; set; }
        public string DocumentNumber { get; set; }
        public int? Progress { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? SubmitOn { get; set; }
        public string EmployeeSubgroup { get; set; }
        public string EmployeeSubgroupText { get; set; }
        public string DocumentStatusCode { get; set; }
        public string DocumentStatusTitle { get; set; }
        public bool EnableDocumentAction { get; set; }
    }
}
