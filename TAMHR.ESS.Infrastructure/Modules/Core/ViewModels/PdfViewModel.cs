using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TAMHR.ESS.Domain;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class PdfViewModel
    {
        public DocumentApproval DocumentApproval { get; set; }

        public ActualOrganizationStructure OrgInfo { get; set; }

        public IEnumerable<EmployeeeOrganizationObjectStoredEntity> OrgObjects { get; set; }

        public IEnumerable<PrintOutEntity> PrintoutMatrix { get; set; }

        public object Object { get; set; }

        public object ViewData { get; set; }

        public dynamic ViewBag { get; set; }
    }
    
}
