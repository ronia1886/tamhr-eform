using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;
using TAMHR.ESS.Domain;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class BupotRequest
    {
        public string Password { get; set; }
        public int Month { get; set; }
        public int Period { get; set; }
    }
}
