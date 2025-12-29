using System;
using System.Collections.Generic;
using System.Text;

namespace TAMHR.ESS.Infrastructure.Helpers
{
    public static class  EnumHelper
    {
        public enum eDocumentAction
        {
            draft,
            completed,
            revised,
            inprogress,
            rejected,
        }

        public enum FamilyTypeCode
        {
            //pasangan,
            suamiistri,
            anakkandung
        }

        public enum MaritalStatus
        {
            janda,
            menikah,
            belummenikah,
            duda
        }

        public enum objectDescription
        {
            Division,
            Directorate,
            Company,
            Section,
            Department
        }
    }
}
