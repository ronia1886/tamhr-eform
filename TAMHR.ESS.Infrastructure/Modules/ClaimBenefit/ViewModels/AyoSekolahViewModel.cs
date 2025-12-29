using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;
using TAMHR.ESS.Domain;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class AyoSekolahViewModel
    {
        public string RegNumber { get; set; }
        public string Name { get; set; }
        public string Division { get; set; }
        public string Department { get; set; }
        public string Section { get; set; }
        public string Class { get; set; }
        public string Remarks { get; set; }
        public IEnumerable<EmployeeeOrganizationObjectStoredEntity> OrgObjects { get; set; }

    }
}
