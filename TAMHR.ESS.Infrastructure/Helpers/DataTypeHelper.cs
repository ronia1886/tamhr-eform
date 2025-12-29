using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace TAMHR.ESS.Infrastructure.Helpers
{
    public static class DataTypeHelper
    {
        private static readonly Dictionary<string, Type> _dicts = new Dictionary<string, Type>()
        {
            { "uniqueidentifier", Nullable.GetUnderlyingType(typeof(Guid?)) },
            { "int", Nullable.GetUnderlyingType(typeof(int?)) },
            { "bigint", Nullable.GetUnderlyingType(typeof(long?)) },
            { "char", typeof(char) },
            { "nchar", typeof(char) },
            { "xml", typeof(char) },
            { "varchar", typeof(string) },
            { "nvarchar", typeof(string) },
            { "text", typeof(string) },
            //{ "text", Nullable.GetUnderlyingType(typeof(int?)) },
            { "ntext", Nullable.GetUnderlyingType(typeof(int?)) },
            { "decimal", Nullable.GetUnderlyingType(typeof(decimal?)) },
            { "smallmoney", Nullable.GetUnderlyingType(typeof(decimal?)) },
            { "money", Nullable.GetUnderlyingType(typeof(decimal?)) },
            { "float", Nullable.GetUnderlyingType(typeof(double?)) },
            { "real", Nullable.GetUnderlyingType(typeof(float?)) },
            { "bit", typeof(bool) },
            { "image", typeof(byte[]) },
            { "timestamp", typeof(byte[]) },
            { "varbinary", typeof(byte[]) },
            { "binary", typeof(byte[]) },
            { "datetime", Nullable.GetUnderlyingType(typeof(DateTime?)) },
            { "smalldatetime", Nullable.GetUnderlyingType(typeof(DateTime?)) },
            { "date", Nullable.GetUnderlyingType(typeof(DateTime?)) },
            { "datetime2", Nullable.GetUnderlyingType(typeof(DateTime?)) },
            { "time", Nullable.GetUnderlyingType(typeof(TimeSpan?)) }
        };

        public static Type GetClrType(string sqlType)
        {
            var sqlTypeLower = sqlType.ToLower();
            
            return _dicts.ContainsKey(sqlTypeLower) ? _dicts[sqlTypeLower] : null;
        }

        public static Type GetClrType(SqlDbType sqlType)
        {
            switch (sqlType)
            {
                case SqlDbType.BigInt:
                    return typeof(long?);

                case SqlDbType.Binary:
                case SqlDbType.Image:
                case SqlDbType.Timestamp:
                case SqlDbType.VarBinary:
                    return typeof(byte[]);

                case SqlDbType.Bit:
                    return typeof(bool?);

                case SqlDbType.Char:
                case SqlDbType.NChar:
                case SqlDbType.NText:
                case SqlDbType.NVarChar:
                case SqlDbType.Text:
                case SqlDbType.VarChar:
                case SqlDbType.Xml:
                    return typeof(string);

                case SqlDbType.DateTime:
                case SqlDbType.SmallDateTime:
                case SqlDbType.Date:
                case SqlDbType.Time:
                case SqlDbType.DateTime2:
                    return typeof(DateTime?);

                case SqlDbType.Decimal:
                case SqlDbType.Money:
                case SqlDbType.SmallMoney:
                    return typeof(decimal?);

                case SqlDbType.Float:
                    return typeof(double?);

                case SqlDbType.Int:
                    return typeof(int?);

                case SqlDbType.Real:
                    return typeof(float?);

                case SqlDbType.UniqueIdentifier:
                    return typeof(Guid?);

                case SqlDbType.SmallInt:
                    return typeof(short?);

                case SqlDbType.TinyInt:
                    return typeof(byte?);

                case SqlDbType.Variant:
                case SqlDbType.Udt:
                    return typeof(object);

                case SqlDbType.Structured:
                    return typeof(DataTable);

                case SqlDbType.DateTimeOffset:
                    return typeof(DateTimeOffset?);

                default:
                    throw new ArgumentOutOfRangeException("sqlType");
            }
        }
    }
}
