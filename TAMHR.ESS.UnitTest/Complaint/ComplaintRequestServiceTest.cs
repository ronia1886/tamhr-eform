using System;
using System.Linq;
using Moq;
using Xunit;

using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;

using TAMHR.ESS.Infrastructure.ViewModels;

namespace TAMHR.ESS.UnitTest.Others
{
    public class ComplaintRequestServiceTest
    {
        private readonly Mock<IUnitOfWork> _uow;
        private readonly Mock<IRepository<ComplaintRequestStub>> _repo;
        private readonly ComplaintRequestServiceForTest _service;

        public ComplaintRequestServiceTest()
        {
            _uow = new Mock<IUnitOfWork>();
            _repo = new Mock<IRepository<ComplaintRequestStub>>();

            _uow.Setup(u => u.GetRepository<ComplaintRequestStub>())
                .Returns(_repo.Object);

            _uow.Setup(u => u.SaveChanges()).Returns(1);

            _service = new ComplaintRequestServiceForTest(_uow.Object);
        }

        [Fact]
        public void Upsert_Should_Call_Upsert_And_SaveChanges()
        {
            var vm = new ComplaintRequestViewModel
            {
                ComplaintCode = "CMP-001",
                ComplaintSubCode = "SUB-01",
                ComplaintDetail = "Detail komplain...",
                Solution = "Solusi yang diharapkan",
                Expectation = "Selesai minggu ini",
                Remarks = "Catatan internal",
                FilePath = "/files/complaint/abc.pdf",
                CommonFileId = Guid.NewGuid()
            };

            var entity = new ComplaintRequestStub
            {
                Id = Guid.NewGuid(),
                ComplaintCode = vm.ComplaintCode,
                ComplaintSubCode = vm.ComplaintSubCode,
                ComplaintDetail = vm.ComplaintDetail,
                Solution = vm.Solution,
                Expectation = vm.Expectation,
                Remarks = vm.Remarks,
                FilePath = vm.FilePath,
                CommonFileId = vm.CommonFileId,
                RowStatus = true
            };

            var expectedProps = new[]
            {
                "ComplaintCode",
                "ComplaintSubCode",
                "ComplaintDetail",
                "Solution",
                "Expectation",
                "Remarks",
                "FilePath",
                "CommonFileId"
            };

            _service.Upsert(entity);

            _repo.Verify(r => r.Upsert<Guid>(
                    It.Is<ComplaintRequestStub>(e => e == entity),
                    It.Is<string[]>(props => props.SequenceEqual(expectedProps))
                ),
                Times.Once);

            _uow.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Fact]
        public void DeleteById_Should_Call_DeleteById_And_SaveChanges()
        {
            var id = Guid.NewGuid();

            _service.DeleteById(id);

            _repo.Verify(r => r.DeleteById(id), Times.Once);
            _uow.Verify(u => u.SaveChanges(), Times.Once);
        }
    }

    // === STUBS (dibuat PUBLIC agar Moq bisa mem-proxy IRepository<Stub>) ===
    public class ComplaintRequestStub
    {
        public Guid Id { get; set; }
        public string ComplaintCode { get; set; }
        public string ComplaintSubCode { get; set; }
        public string ComplaintDetail { get; set; }
        public string Solution { get; set; }
        public string Expectation { get; set; }
        public string Remarks { get; set; }
        public string FilePath { get; set; }
        public Guid CommonFileId { get; set; }
        public bool RowStatus { get; set; }
    }

    public class ComplaintRequestServiceForTest
    {
        private readonly IUnitOfWork _uow;
        private readonly IRepository<ComplaintRequestStub> _repo;

        private static readonly string[] _properties = new[]
        {
            "ComplaintCode",
            "ComplaintSubCode",
            "ComplaintDetail",
            "Solution",
            "Expectation",
            "Remarks",
            "FilePath",
            "CommonFileId"
        };

        public ComplaintRequestServiceForTest(IUnitOfWork uow)
        {
            _uow = uow;
            _repo = _uow.GetRepository<ComplaintRequestStub>();
        }

        public void Upsert(ComplaintRequestStub data)
        {
            _repo.Upsert<Guid>(data, _properties);
            _uow.SaveChanges();
        }

        public void DeleteById(Guid id)
        {
            _repo.DeleteById(id);
            _uow.SaveChanges();
        }
    }
}
