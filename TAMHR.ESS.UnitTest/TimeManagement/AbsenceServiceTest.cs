using System;
using System.Text.Json;
using Xunit;
using TAMHR.ESS.Infrastructure.ViewModels;

namespace TAMHR.ESS.UnitTest.TimeManagementTest
{
    /// <summary>
    /// Unit test untuk AbsenceViewModel (tanpa ketergantungan service).
    /// Nama file disarankan: AbsenceServiceTest.cs
    /// </summary>
    public class AbsenceServiceTest
    {
        [Fact]
        public void Defaults_Are_Null_And_Constructible()
        {
            var vm = new AbsenceViewModel();

            Assert.NotNull(vm);
            Assert.Null(vm.StartDate);
            Assert.Null(vm.EndDate);
            Assert.Null(vm.ReasonId);
            Assert.Null(vm.Reason);
            Assert.Null(vm.ReasonType);
            Assert.Null(vm.Description);
            Assert.Null(vm.TotalAbsence);
            Assert.Null(vm.RemainingDaysOff);
            Assert.Null(vm.RemainingLongLeave);
            Assert.Null(vm.MandatoryAttachment);
            Assert.Null(vm.SupportingAttachmentPath);
            Assert.Null(vm.IsPlanning);
            Assert.Null(vm.Remarks);
            Assert.Null(vm.AnnualLeavePlanningDetailId);
            Assert.Null(vm.KategoriPenyakit);
            Assert.Null(vm.SpesifikPenyakit);
        }

        [Fact]
        public void Can_Construct_And_Assign_All_Fields()
        {
            var start = new DateTime(2025, 3, 4);
            var end = new DateTime(2025, 3, 6);

            var vm = new AbsenceViewModel
            {
                StartDate = start,
                EndDate = end,
                ReasonId = Guid.NewGuid(),
                Reason = "Annual Leave",
                ReasonType = "annual-leave",
                Description = "Cuti tahunan 3 hari",
                TotalAbsence = "3",              // string by design
                RemainingDaysOff = 7,
                RemainingLongLeave = 2,
                MandatoryAttachment = true,
                SupportingAttachmentPath = "/files/doctor-note.pdf",
                IsPlanning = true,
                Remarks = "Sudah disetujui atasan",
                AnnualLeavePlanningDetailId = Guid.NewGuid(),
                KategoriPenyakit = "N/A",
                SpesifikPenyakit = "N/A"
            };

            // Assert setiap field
            Assert.Equal(start, vm.StartDate);
            Assert.Equal(end, vm.EndDate);
            Assert.NotNull(vm.ReasonId);
            Assert.Equal("Annual Leave", vm.Reason);
            Assert.Equal("annual-leave", vm.ReasonType);
            Assert.Equal("Cuti tahunan 3 hari", vm.Description);
            Assert.Equal("3", vm.TotalAbsence);
            Assert.Equal(7, vm.RemainingDaysOff);
            Assert.Equal(2, vm.RemainingLongLeave);
            Assert.True(vm.MandatoryAttachment);
            Assert.Equal("/files/doctor-note.pdf", vm.SupportingAttachmentPath);
            Assert.True(vm.IsPlanning);
            Assert.Equal("Sudah disetujui atasan", vm.Remarks);
            Assert.NotNull(vm.AnnualLeavePlanningDetailId);
            Assert.Equal("N/A", vm.KategoriPenyakit);
            Assert.Equal("N/A", vm.SpesifikPenyakit);

            // sanity check relasi tanggal (jika keduanya ada)
            Assert.True(vm.EndDate >= vm.StartDate);
        }

        [Fact]
        public void Json_RoundTrip_Should_Preserve_Important_Fields()
        {
            var vm = new AbsenceViewModel
            {
                StartDate = new DateTime(2025, 5, 1),
                EndDate = new DateTime(2025, 5, 2),
                Reason = "Sick Leave",
                ReasonType = "sick-leave",
                TotalAbsence = "2",
                MandatoryAttachment = false,
                IsPlanning = false,
                Remarks = "Tanpa lampiran",
                KategoriPenyakit = "Flu",
                SpesifikPenyakit = "Influenza A"
            };

            var json = JsonSerializer.Serialize(vm);
            Assert.False(string.IsNullOrWhiteSpace(json));

            var back = JsonSerializer.Deserialize<AbsenceViewModel>(json);
            Assert.NotNull(back);

            Assert.Equal(vm.StartDate, back.StartDate);
            Assert.Equal(vm.EndDate, back.EndDate);
            Assert.Equal(vm.Reason, back.Reason);
            Assert.Equal(vm.ReasonType, back.ReasonType);
            Assert.Equal(vm.TotalAbsence, back.TotalAbsence);
            Assert.Equal(vm.MandatoryAttachment, back.MandatoryAttachment);
            Assert.Equal(vm.IsPlanning, back.IsPlanning);
            Assert.Equal(vm.Remarks, back.Remarks);
            Assert.Equal(vm.KategoriPenyakit, back.KategoriPenyakit);
            Assert.Equal(vm.SpesifikPenyakit, back.SpesifikPenyakit);
        }
    }
}
