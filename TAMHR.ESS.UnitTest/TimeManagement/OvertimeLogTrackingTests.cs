using System;
using System.Linq;
using System.Reflection;
using Xunit;

namespace TAMHR.ESS.UnitTest.TimeManagementTest
{
    public class OvertimeLogTrackingSmokeTest
    {
        // ===== Helpers: cari tipe by simple name di assembly yg diload test =====
        private static Type FindType(string simpleName)
        {
            return AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch { return Array.Empty<Type>(); }
                })
                .FirstOrDefault(t => string.Equals(t.Name, simpleName, StringComparison.OrdinalIgnoreCase));
        }

        private static PropertyInfo FindProp(Type t, string name) =>
            t?.GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

        private static bool TrySet(object target, string propName, object value)
        {
            var p = FindProp(target.GetType(), propName);
            if (p == null || !p.CanWrite) return false;

            object converted = ConvertFor(p.PropertyType, value);
            p.SetValue(target, converted);
            return true;
        }

        private static object ConvertFor(Type dst, object value)
        {
            if (value == null) return null!;
            var t = Nullable.GetUnderlyingType(dst) ?? dst;

            if (t == typeof(string)) return Convert.ToString(value);
            if (t == typeof(Guid)) return value is Guid g ? g : Guid.Parse(value.ToString());
            if (t == typeof(int)) return Convert.ToInt32(value);
            if (t == typeof(long)) return Convert.ToInt64(value);
            if (t == typeof(decimal)) return Convert.ToDecimal(value);
            if (t == typeof(double)) return Convert.ToDouble(value);
            if (t == typeof(float)) return Convert.ToSingle(value);
            if (t == typeof(bool)) return Convert.ToBoolean(value);
            if (t == typeof(DateTime)) return value is DateTime dt ? dt : DateTime.Parse(value.ToString());
            if (t == typeof(TimeSpan)) return value is TimeSpan ts ? ts : TimeSpan.Parse(value.ToString());

            return value;
        }

        // ===== Tests =====

        [Fact]
        public void StoredEntity_Type_Exists()
        {
            var t = FindType("OvertimeLogTrackingStoredEntity");
            Assert.NotNull(t);
        }

        [Fact]
        public void StoredEntity_Can_Construct_And_Assign_Common_Fields_If_Available()
        {
            var t = FindType("OvertimeLogTrackingStoredEntity");
            Assert.NotNull(t);

            // pastikan ada ctor default
            var ctor = t.GetConstructor(Type.EmptyTypes);
            Assert.NotNull(ctor);

            var model = Activator.CreateInstance(t);
            Assert.NotNull(model);

            // isi beberapa properti umum JIKA ada di tipe tersebut (aman tanpa tahu schema pasti)
            TrySet(model, "NoReg", "10001");
            TrySet(model, "PostCode", "HR01");
            TrySet(model, "WorkingDate", new DateTime(2025, 2, 1));
            TrySet(model, "CreatedDate", new DateTime(2025, 2, 1, 8, 30, 0));
            TrySet(model, "Action", "UPSERT");
            TrySet(model, "Description", "unit-test");

            // minimal: ada SATU properti identitas yg terisi/tersedia
            bool hasIdentityProp =
                FindProp(t, "NoReg") != null ||
                FindProp(t, "Id") != null ||
                FindProp(t, "DocumentNumber") != null;

            Assert.True(hasIdentityProp);
        }

        [Fact]
        public void LogTrackingService_Has_Expected_GetOvertimeLogTracking_Signature()
        {
            var serviceType = FindType("LogTrackingService");
            Assert.NotNull(serviceType);

            var method = serviceType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(m => m.Name == "GetOvertimeLogTracking" && m.GetParameters().Length == 5);

            Assert.NotNull(method);

            var ps = method.GetParameters();
            // Expected parameter types: DateTime, DateTime, string, string, DateTime?
            Assert.Equal("DateTime", ps[0].ParameterType.Name);
            Assert.Equal("DateTime", ps[1].ParameterType.Name);
            Assert.Equal("String", ps[2].ParameterType.Name);
            Assert.Equal("String", ps[3].ParameterType.Name);

            var p4 = ps[4].ParameterType;
            var isNullableDateTime =
                p4.IsGenericType &&
                p4.GetGenericTypeDefinition() == typeof(Nullable<>) &&
                p4.GetGenericArguments()[0].Name == "DateTime";

            Assert.True(isNullableDateTime);
        }
    }
}
