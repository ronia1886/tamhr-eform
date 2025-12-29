using System;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("VW_DATABASE_OBJECT_SCHEMA")]
    public partial class DatabaseObjectSchemaView : IEntityMarker
    {
        public Guid Id { get; set; }
        public string Schema { get; set; }
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public string ColumnDefault { get; set; }
        public bool IsNullable { get; set; }
        public string DataType { get; set; }
        public int? CharacterMaximumLength { get; set; }
        public string ColumnDefinition { get; set; }
    }
}
