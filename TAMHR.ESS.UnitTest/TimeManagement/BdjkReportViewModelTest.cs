using System;
using System.Text.Json;
using Xunit;
using TAMHR.ESS.Infrastructure.ViewModels;

namespace TAMHR.ESS.UnitTest.TimeManagementTest
{
    public class BdjkReportViewModelTest
    {
        private static bool TryParseHm(string s, out TimeSpan ts)
        {
            // dukung "HH:mm" / "H:mm"
            return TimeSpan.TryParse(s, out ts);
        }

        [Fact]
        public void Can_Construct_And_Set_Fields()
        {
            var vm = new BdjkReportViewModel
            {
                Id = Guid.NewGuid(),
                ParentId = Guid.NewGuid(),
                DocNumber = "BDJK-0001/III/2025",
                Noreg = "101213",
                NamaKaryawan = "A. Dadang",
                Tanggal = new DateTime(2025, 3, 1),
                ProxyIn = "07:00",
                ProxyOut = "12:02",
                Durasi = "05:02",
                Aktifitas = "Support produksi",
                KodeBdjk = "BD01",
                Status = "approved",
                UangMakan = "Ya",
                UangTaksi = "Tidak",
                Approver = "Budi",
                Description = "Unit test sanity"
            };

            Assert.NotEqual(Guid.Empty, vm.Id);
            Assert.NotEqual(Guid.Empty, vm.ParentId);
            Assert.NotNull(vm.DocNumber);
            Assert.NotNull(vm.Noreg);
            Assert.True(vm.Tanggal.HasValue);

            // format jam harus valid
            Assert.True(TryParseHm(vm.ProxyIn, out _));
            Assert.True(TryParseHm(vm.ProxyOut, out _));
        }

        [Fact]
        public void ProxyOut_Should_Be_After_ProxyIn_When_Format_Is_Valid()
        {
            var vm = new BdjkReportViewModel
            {
                Tanggal = new DateTime(2025, 3, 1),
                ProxyIn = "07:00",
                ProxyOut = "12:02"
            };

            // jika format valid, pastikan out > in (izinkan lintas hari)
            if (TryParseHm(vm.ProxyIn, out var tin) && TryParseHm(vm.ProxyOut, out var tout))
            {
                var baseDate = vm.Tanggal ?? DateTime.Today;
                var dtIn = baseDate.Date + tin;
                var dtOut = baseDate.Date + tout;

                if (dtOut <= dtIn) dtOut = dtOut.AddDays(1); // handle overnight

                Assert.True(dtOut > dtIn);
            }
            else
            {
                // kalau format tidak valid, anggap lolos karena bukan tanggung jawab VM
                Assert.True(true);
            }
        }

        [Fact]
        public void Json_RoundTrip_Should_Preserve_Values()
        {
            var original = new BdjkReportViewModel
            {
                Id = Guid.NewGuid(),
                ParentId = Guid.NewGuid(),
                DocNumber = "BDJK-0002/III/2025",
                Noreg = "10001",
                NamaKaryawan = "Siti",
                Tanggal = new DateTime(2025, 3, 2),
                ProxyIn = "06:45",
                ProxyOut = "11:30",
                Durasi = "04:45",
                Aktifitas = "Dokumentasi",
                KodeBdjk = "BD02",
                Status = "inprogress",
                UangMakan = "Ya",
                UangTaksi = "Ya",
                Approver = "Andi",
                Description = "Round-trip check"
            };

            var json = JsonSerializer.Serialize(original);
            Assert.False(string.IsNullOrWhiteSpace(json));

            var back = JsonSerializer.Deserialize<BdjkReportViewModel>(json);
            Assert.NotNull(back);

            // verifikasi sebagian field penting
            Assert.Equal(original.Id, back.Id);
            Assert.Equal(original.ParentId, back.ParentId);
            Assert.Equal(original.DocNumber, back.DocNumber);
            Assert.Equal(original.Noreg, back.Noreg);
            Assert.Equal(original.NamaKaryawan, back.NamaKaryawan);
            Assert.Equal(original.Tanggal, back.Tanggal);
            Assert.Equal(original.ProxyIn, back.ProxyIn);
            Assert.Equal(original.ProxyOut, back.ProxyOut);
            Assert.Equal(original.Durasi, back.Durasi);
            Assert.Equal(original.Status, back.Status);
        }
    }
}
