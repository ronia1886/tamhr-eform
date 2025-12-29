using System;
using Agit.Common.Attributes;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SP_REQ_DOWNLOAD_MEDICAL", DatabaseObjectType.StoredProcedure)]
    public class ReqDownloadMedicalStoredEntity
    {
        public Guid Id { get; set; }
        public int RowNum { get; set; }
        public string Approver { get; set; }
        public string Requestor { get; set; }
        public string StatusRequest { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        

    }

}

