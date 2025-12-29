using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    public class Period
    {
        public int id { get; set; }
        public int Years { get; set; }
        public string Month { get; set; }
    }
}
