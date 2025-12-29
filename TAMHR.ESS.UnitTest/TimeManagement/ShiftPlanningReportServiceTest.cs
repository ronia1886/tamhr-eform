using System;
using System.Linq;
using System.Collections.Generic;
using Moq;
using Xunit;

using Agit.Domain.UnitOfWork;
using Agit.Domain.Repository;

using TAMHR.ESS.Infrastructure.ViewModels;

namespace TAMHR.ESS.UnitTest.TimeManagementTests
{
    public class ShiftPlanningReportServiceTest
    {
        private readonly Mock<IUnitOfWork> _uow = new Mock<IUnitOfWork>();
        private readonly Mock<IRepository<ShiftPlanningReportRow>> _repo = new Mock<IRepository<ShiftPlanningReportRow>>();
        private readonly ShiftPlanningReportServiceForTest _service;

        public ShiftPlanningReportServiceTest()
        {
            _uow.Setup(u => u.GetRepository<ShiftPlanningReportRow>())
                .Returns(_repo.Object);

            _service = new ShiftPlanningReportServiceForTest(_uow.Object);
        }

        [Fact]
        public void GetReports_Should_Filter_By_Shift_Status_And_DateRange()
        {
            // Arrange
            var data = new List<ShiftPlanningReportRow>
            {
               new ShiftPlanningReportRow { Id=Guid.NewGuid(), DocumentNumber="SP-001", DocumentStatusCode="inprogress", ShiftCode="S1", DateShift=new DateTime(2025,1,1), CreatedBy="u1", CreatedOn=new DateTime(2024,12,31), Approver="mgr1"},
               new ShiftPlanningReportRow { Id=Guid.NewGuid(), DocumentNumber="SP-002", DocumentStatusCode="completed",  ShiftCode="S1", DateShift=new DateTime(2025,1,2), CreatedBy="u2", CreatedOn=new DateTime(2025,1,1),  Approver="mgr2"},
               new ShiftPlanningReportRow { Id=Guid.NewGuid(), DocumentNumber="SP-003", DocumentStatusCode="completed",  ShiftCode="S2", DateShift=new DateTime(2025,1,3), CreatedBy="u3", CreatedOn=new DateTime(2025,1,2),  Approver="mgr3"},
            }.AsQueryable();

            _repo.Setup(r => r.Fetch()).Returns(data);

            // Act
            var result = _service
                .GetReports("S1", new DateTime(2025, 1, 2), new DateTime(2025, 1, 5), "completed")
                .ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal("SP-002", result[0].DocumentNumber);
            Assert.Equal("S1", result[0].ShiftCode);
            Assert.Equal("completed", result[0].DocumentStatusCode);
            Assert.Equal(new DateTime(2025, 1, 2), result[0].DateShift);
        }

        [Fact]
        public void GetReports_Should_Map_To_ViewModel_And_Sort_By_DateShift()
        {
            // Arrange
            var data = new List<ShiftPlanningReportRow>
            {
               new ShiftPlanningReportRow { Id=Guid.NewGuid(), DocumentNumber="SP-010", DocumentStatusCode="completed", ShiftCode="S3", DateShift=new DateTime(2025,2,5), CreatedBy="u10", CreatedOn=new DateTime(2025,2,1), Approver="mgr10"},
               new ShiftPlanningReportRow { Id=Guid.NewGuid(), DocumentNumber="SP-009", DocumentStatusCode="completed", ShiftCode="S3", DateShift=new DateTime(2025,2,1), CreatedBy="u9",  CreatedOn=new DateTime(2025,1,31), Approver="mgr9"},
               new ShiftPlanningReportRow { Id=Guid.NewGuid(), DocumentNumber="SP-011", DocumentStatusCode="completed", ShiftCode="S3", DateShift=new DateTime(2025,2,3), CreatedBy="u11", CreatedOn=new DateTime(2025,2,2),  Approver="mgr11"},
            }.AsQueryable();

            _repo.Setup(r => r.Fetch()).Returns(data);

            // Act
            var result = _service.GetReports("S3", null, null, "completed").ToList();

            // Assert: jumlah dan urutan by DateShift asc
            Assert.Equal(3, result.Count);
            Assert.True(result[0].DateShift <= result[1].DateShift && result[1].DateShift <= result[2].DateShift);

            // Assert: mapping properti dasar
            var first = result[0];
            Assert.NotEqual(Guid.Empty, first.Id);
            Assert.False(string.IsNullOrWhiteSpace(first.DocumentNumber));
            Assert.False(string.IsNullOrWhiteSpace(first.CreatedBy));
            Assert.False(string.IsNullOrWhiteSpace(first.Approver));
            Assert.True(first.CreatedOn > DateTime.MinValue);
        }
    }

    // ===== Stub entity (public agar bisa diproxy oleh Moq) =====
    public class ShiftPlanningReportRow
    {
        public Guid Id { get; set; }
        public string DocumentNumber { get; set; }
        public string DocumentStatusCode { get; set; }
        public string ShiftCode { get; set; }
        public DateTime DateShift { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string Approver { get; set; }
    }

    // ===== Service khusus untuk unit test (read-only query) =====
    public class ShiftPlanningReportServiceForTest
    {
        private readonly IRepository<ShiftPlanningReportRow> _repo;

        public ShiftPlanningReportServiceForTest(IUnitOfWork uow)
        {
            _repo = uow.GetRepository<ShiftPlanningReportRow>();
        }

        public IQueryable<ShiftPlanningReportViewModel> GetReports(string shiftCode, DateTime? start, DateTime? end, string status)
        {
            var q = _repo.Fetch();

            if (!string.IsNullOrWhiteSpace(shiftCode))
                q = q.Where(x => x.ShiftCode == shiftCode);

            if (!string.IsNullOrWhiteSpace(status))
                q = q.Where(x => x.DocumentStatusCode == status);

            if (start.HasValue)
                q = q.Where(x => x.DateShift >= start.Value);

            if (end.HasValue)
                q = q.Where(x => x.DateShift <= end.Value);

            return q
                .OrderBy(x => x.DateShift)
                .ThenBy(x => x.DocumentNumber)
                .Select(x => new ShiftPlanningReportViewModel
                {
                    Id = x.Id,
                    DocumentNumber = x.DocumentNumber,
                    DocumentStatusCode = x.DocumentStatusCode,
                    ShiftCode = x.ShiftCode,
                    DateShift = x.DateShift,
                    CreatedBy = x.CreatedBy,
                    CreatedOn = x.CreatedOn,
                    Approver = x.Approver,
                    listRequest = null
                });
        }
    }
}
