using System;
using System.Text.Json;
using Xunit;
using TAMHR.ESS.Infrastructure.ViewModels;

namespace TAMHR.ESS.UnitTest.TimeManagementTest
{
    /// <summary>
    /// Unit test ringan untuk AbsenceReportViewModel (tanpa dependency service).
    /// </summary>
    public class AbsenceReportServiceTest
    {
        [Fact]
        public void Defaults_Are_Null_And_Constructible()
        {
            var vm = new AbsenceReportViewModel();

            Assert.NotNull(vm);
            Assert.Equal(Guid.Empty, vm.Id);
            Assert.Null(vm.Noreg);
            Assert.Null(vm.Name);
            Assert.Null(vm.Reason);
            Assert.Null(vm.Status);
            Assert.Null(vm.Date);
            Assert.Null(vm.StartDate);
            Assert.Null(vm.EndDate);
            Assert.Null(vm.Approver);
        }

        [Fact]
        public void Can_Construct_And_Assign_All_Fields()
        {
            var id = Guid.NewGuid();
            var start = new DateTime(2025, 6, 10);
            var end = new DateTime(2025, 6, 12);
            var date = new DateTime(2025, 6, 11);

            var vm = new AbsenceReportViewModel
            {
                Id = id,
                Noreg = "100200",
                Name = "Jane Doe",
                Reason = "Annual Leave",
                Status = "approved",
                Date = date,
                StartDate = start,
                EndDate = end,
                Approver = "Manager A"
            };

            Assert.Equal(id, vm.Id);
            Assert.Equal("100200", vm.Noreg);
            Assert.Equal("Jane Doe", vm.Name);
            Assert.Equal("Annual Leave", vm.Reason);
            Assert.Equal("approved", vm.Status);
            Assert.Equal(date, vm.Date);
            Assert.Equal(start, vm.StartDate);
            Assert.Equal(end, vm.EndDate);
            Assert.Equal("Manager A", vm.Approver);

            // sanity: jika Start & End terisi, End >= Start seharusnya true untuk data valid
            if (vm.StartDate.HasValue && vm.EndDate.HasValue)
                Assert.True(vm.EndDate.Value >= vm.StartDate.Value);

            // sanity: jika Date diisi, pastikan berada dalam rentang (contoh skenario data valid)
            if (vm.Date.HasValue && vm.StartDate.HasValue && vm.EndDate.HasValue)
                Assert.InRange(vm.Date.Value, vm.StartDate.Value, vm.EndDate.Value);
        }

        [Fact]
        public void Json_RoundTrip_Should_Preserve_Key_Fields()
        {
            var vm = new AbsenceReportViewModel
            {
                Id = Guid.NewGuid(),
                Noreg = "889900",
                Name = "John Smith",
                Reason = "Sick Leave",
                Status = "pending",
                Date = new DateTime(2025, 7, 2),
                StartDate = new DateTime(2025, 7, 1),
                EndDate = new DateTime(2025, 7, 3),
                Approver = "HR Admin"
            };

            var json = JsonSerializer.Serialize(vm);
            Assert.False(string.IsNullOrWhiteSpace(json));

            var back = JsonSerializer.Deserialize<AbsenceReportViewModel>(json);
            Assert.NotNull(back);

            Assert.Equal(vm.Id, back.Id);
            Assert.Equal(vm.Noreg, back.Noreg);
            Assert.Equal(vm.Name, back.Name);
            Assert.Equal(vm.Reason, back.Reason);
            Assert.Equal(vm.Status, back.Status);
            Assert.Equal(vm.Date, back.Date);
            Assert.Equal(vm.StartDate, back.StartDate);
            Assert.Equal(vm.EndDate, back.EndDate);
            Assert.Equal(vm.Approver, back.Approver);
        }
    }
}
