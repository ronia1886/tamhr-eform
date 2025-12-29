using Agit.Common.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace TAMHR.ESS.Domain
{ 
    [DatabaseObject("SF_GET_COP_REPORT", DatabaseObjectType.TableValued)]
    public class CarPurchasedReportStoredEntity
    {
        public Guid Id { get; set; }
        public string DocumentNumber { get; set; }
        public string FormType { get; set; }
        public string NoReg { get; set; }
        public string Name { get; set; }
        public string PostName { get; set; }
        public string Class { get; set; }
        public string SubmissionType { get; set; }
        public string CarModel { get; set; }
        public string CarModelType { get; set; }
        public string ColorName { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime SubmitOn { get; set; }
        public string BuyFor { get; set; }
    }
}
