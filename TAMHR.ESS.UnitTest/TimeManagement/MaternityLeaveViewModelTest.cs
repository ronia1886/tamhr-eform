using System;
using System.Text.Json;
using Xunit;
using TAMHR.ESS.Infrastructure.ViewModels;

namespace TAMHR.ESS.UnitTest.TimeManagementTest
{
    public class MaternityLeaveViewModelTest
    {
        [Fact]
        public void Can_Construct_And_Assign_All_Fields()
        {
            // Arrange
            var vm = new MaternityLeaveViewModel
            {
                GestationalAge = "32 weeks",
                Date = new DateTime(2025, 2, 1),
                EstimatedDayOfBirth = new DateTime(2025, 3, 10),
                DayOfBirth = new DateTime(2025, 3, 8),
                StartHandoverOfWork = new DateTime(2025, 2, 20),
                StartMaternityLeave = new DateTime(2025, 2, 25),
                BackToWork = new DateTime(2025, 4, 22),
                MedicalMertificatePath = "/files/medical_cert.pdf",
                BirthCertificatePath = "/files/birth_cert.pdf",
                Remarks = "All good"
            };

            // Assert
            Assert.Equal("32 weeks", vm.GestationalAge);
            Assert.Equal(new DateTime(2025, 2, 1), vm.Date);
            Assert.Equal(new DateTime(2025, 3, 10), vm.EstimatedDayOfBirth);
            Assert.Equal(new DateTime(2025, 3, 8), vm.DayOfBirth);
            Assert.Equal(new DateTime(2025, 2, 20), vm.StartHandoverOfWork);
            Assert.Equal(new DateTime(2025, 2, 25), vm.StartMaternityLeave);
            Assert.Equal(new DateTime(2025, 4, 22), vm.BackToWork);
            Assert.Equal("/files/medical_cert.pdf", vm.MedicalMertificatePath);
            Assert.Equal("/files/birth_cert.pdf", vm.BirthCertificatePath);
            Assert.Equal("All good", vm.Remarks);
        }

        [Fact]
        public void Json_RoundTrip_Should_Preserve_Important_Fields()
        {
            var vm = new MaternityLeaveViewModel
            {
                GestationalAge = "30 weeks",
                EstimatedDayOfBirth = new DateTime(2025, 3, 1),
                Remarks = "serialize test",
                MedicalMertificatePath = "/docs/mc.pdf"
            };

            var json = JsonSerializer.Serialize(vm);
            Assert.False(string.IsNullOrWhiteSpace(json));

            var back = JsonSerializer.Deserialize<MaternityLeaveViewModel>(json);
            Assert.NotNull(back);
            Assert.Equal("30 weeks", back.GestationalAge);
            Assert.Equal(new DateTime(2025, 3, 1), back.EstimatedDayOfBirth);
            Assert.Equal("serialize test", back.Remarks);
            Assert.Equal("/docs/mc.pdf", back.MedicalMertificatePath);
        }
    }

    public class MaternityLeaveDobViewModelTest
    {
        [Fact]
        public void BackToWork_Should_Be_DayOfBirth_Plus_45_Days_When_DayOfBirth_Set()
        {
            // Arrange
            var dob = new DateTime(2025, 5, 10);
            var vm = new MaternityLeaveDobViewModel
            {
                Id = Guid.NewGuid(),
                DocumentApprovalId = Guid.NewGuid(),
                DayOfBirth = dob,
                BirthCertificatePath = "/files/birth_cert.pdf",
                Attachments = null // boleh null; tidak perlu bikin instance DocumentApprovalFile
            };

            // Act
            var expected = dob.AddDays(45);

            // Assert
            Assert.Equal(expected, vm.BackToWork);
        }

        [Fact]
        public void BackToWork_When_DayOfBirth_Null_Should_Be_Approx_Now_Plus_45_Days()
        {
            // Arrange
            var before = DateTime.Now.AddDays(45).AddDays(-1); // toleransi -1 hari
            var after = DateTime.Now.AddDays(45).AddDays(1);  // toleransi +1 hari
            var vm = new MaternityLeaveDobViewModel
            {
                DayOfBirth = null
            };

            // Act
            var btw = vm.BackToWork;

            // Assert (pakai rentang agar stabil di CI)
            Assert.True(btw >= before && btw <= after,
                $"BackToWork {btw:yyyy-MM-dd HH:mm:ss} not within expected range [{before:yyyy-MM-dd HH:mm:ss} .. {after:yyyy-MM-dd HH:mm:ss}]");
        }

        [Fact]
        public void Can_Construct_And_Assign_Id_And_Paths()
        {
            var id = Guid.NewGuid();
            var docId = Guid.NewGuid();

            var vm = new MaternityLeaveDobViewModel
            {
                Id = id,
                DocumentApprovalId = docId,
                DayOfBirth = new DateTime(2025, 6, 1),
                BirthCertificatePath = "/proofs/birth.pdf"
            };

            Assert.Equal(id, vm.Id);
            Assert.Equal(docId, vm.DocumentApprovalId);
            Assert.Equal("/proofs/birth.pdf", vm.BirthCertificatePath);
            Assert.Equal(new DateTime(2025, 6, 1).AddDays(45), vm.BackToWork);
        }
    }
}
