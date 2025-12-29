using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Domain;
using TAMHR.ESS.WebUI.Areas.OHS.Controllers;
using Xunit;

namespace TAMHR.ESS.UnitTest.OHS
{
    public class MedicalRecordApiController_ValidationTests
    {
        private readonly MedicalRecordApiController _controller;

        public MedicalRecordApiController_ValidationTests()
        {
            _controller = new MedicalRecordApiController
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
        }

        [Fact]
        public async Task CreateMcu_ThrowsException_WhenLokasiMcuIsEmpty()
        {
            var data = new PersonalDataMedicalHistory
            {
                LokasiMCU = "",                 // sengaja kosong -> harusnya kena validasi lokasi
                HealthEmployeeStatus = "OK",
                NoregMcu = "12345"
            };

            // pastikan validasi “Tahun MCU” sudah terpenuhi dulu
            SatisfyYearOrDateMcuValidation(data);

            var ex = await Assert.ThrowsAsync<Exception>(() => _controller.CreateMcu(data));

            // asersi longgar agar tidak rapuh pada kapitalisasi/typo
            Assert.Contains("lokasi", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("mcu", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("must fill", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task CreateMcu_ThrowsException_WhenHealthEmployeeStatusIsEmpty()
        {
            var data = new PersonalDataMedicalHistory
            {
                LokasiMCU = "Jakarta",
                HealthEmployeeStatus = "",      // sengaja kosong -> harusnya kena validasi hasil
                NoregMcu = "12345"
            };

            // pastikan validasi “Tahun MCU” sudah terpenuhi dulu
            SatisfyYearOrDateMcuValidation(data);

            var ex = await Assert.ThrowsAsync<Exception>(() => _controller.CreateMcu(data));

            // asersi longgar agar tidak rapuh (pesan aktual: “Satus Hasil MCU Must Fill”)
            Assert.Contains("hasil", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("mcu", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("must fill", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Mengisi properti terkait Tahun/Tanggal MCU pada model agar lolos validasi awal,
        /// tanpa harus tahu nama field persisnya:
        /// - properti mengandung “tahun”/“year” -> isi tahun berjalan (int/int?/string/long).
        /// - properti tanggal yang namanya mengandung “mcu” dan “tanggal”/“date” -> isi DateTime.Today/string.
        /// </summary>
        private static void SatisfyYearOrDateMcuValidation(PersonalDataMedicalHistory target)
        {
            var props = target.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

            foreach (var p in props)
            {
                var name = p.Name.ToLowerInvariant();
                var t = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;

                // isi field Tahun/Year jika ada
                if (name.Contains("tahun") || name.Contains("year"))
                {
                    if (t == typeof(int)) p.SetValue(target, DateTime.Now.Year);
                    else if (t == typeof(string)) p.SetValue(target, DateTime.Now.Year.ToString());
                    else if (t == typeof(long)) p.SetValue(target, (long)DateTime.Now.Year);
                }

                // isi field Tanggal/Date khusus MCU jika ada
                if (name.Contains("mcu") && (name.Contains("tanggal") || name.Contains("date")))
                {
                    if (t == typeof(DateTime)) p.SetValue(target, DateTime.Today);
                    else if (t == typeof(string)) p.SetValue(target, DateTime.Today.ToString("yyyy-MM-dd"));
                }
            }
        }
    }
}
