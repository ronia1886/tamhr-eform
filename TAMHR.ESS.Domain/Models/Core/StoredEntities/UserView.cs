using Agit.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace TAMHR.ESS.Domain.Models.Core.StoredEntities
{
    [Table("VW_USER")]
    public partial class UserView : IEntityMarker
    {
        public string Type { get; set; }
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string NoReg { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Gender { get; set; }
    }
}
