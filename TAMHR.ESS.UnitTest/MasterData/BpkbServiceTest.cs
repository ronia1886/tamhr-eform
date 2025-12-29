using System;
using System.Linq;
using System.Collections.Generic;
using Moq;
using Xunit;

using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;

using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;

namespace TAMHR.ESS.UnitTest.MasterData
{
    public class BpkbServiceTest
    {
        private readonly Mock<IUnitOfWork> _uow;
        private readonly Mock<IRepository<Bpkb>> _bpkbRepo;
        private readonly Mock<IRepository<BpkbView>> _bpkbViewRepo; // <- pakai IRepository, bukan IReadonlyRepository
        private readonly Mock<IRepository<User>> _userRepo;
        private readonly Mock<IRepository<BpkbRequest>> _bpkbRequestRepo;

        private readonly BpkbService _service;

        public BpkbServiceTest()
        {
            _uow = new Mock<IUnitOfWork>();
            _bpkbRepo = new Mock<IRepository<Bpkb>>();
            _bpkbViewRepo = new Mock<IRepository<BpkbView>>(); // <- disini juga
            _userRepo = new Mock<IRepository<User>>();
            _bpkbRequestRepo = new Mock<IRepository<BpkbRequest>>();

            // Map semua GetRepository<> yang dipakai service
            _uow.Setup(u => u.GetRepository<Bpkb>()).Returns(_bpkbRepo.Object);
            _uow.Setup(u => u.GetRepository<BpkbView>()).Returns(_bpkbViewRepo.Object); // <- kembalikan IRepository<BpkbView>
            _uow.Setup(u => u.GetRepository<User>()).Returns(_userRepo.Object);
            _uow.Setup(u => u.GetRepository<BpkbRequest>()).Returns(_bpkbRequestRepo.Object);

            _uow.Setup(u => u.SaveChanges());

            _service = new BpkbService(_uow.Object);
        }

        [Fact]
        public void Upsert_ShouldCallRepositoryUpsert_AndSaveChanges()
        {
            var model = new Bpkb { Id = Guid.NewGuid(), NoReg = "EMP001", NoBPKB = "BPKB-001" };

            _service.Upsert(model);

            _bpkbRepo.Verify(r => r.Upsert<Guid>(model, It.IsAny<string[]>()), Times.Once);
            _uow.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Fact]
        public void DeleteById_ShouldCallRepositoryDeleteById_AndSaveChanges()
        {
            var id = Guid.NewGuid();

            _service.DeleteById(id);

            _bpkbRepo.Verify(r => r.DeleteById(id), Times.Once);
            _uow.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Fact]
        public void GetViews_ShouldReturnData_FromRepo()
        {
            var list = new List<BpkbView>
            {
                new BpkbView { Id = Guid.NewGuid(), NoBPKB = "A-001" },
                new BpkbView { Id = Guid.NewGuid(), NoBPKB = "A-002" },
            }.AsQueryable();

            _bpkbViewRepo.Setup(r => r.Fetch()).Returns(list);

            var result = _service.GetViews().ToList();

            Assert.Equal(2, result.Count);
            Assert.Contains(result, x => x.NoBPKB == "A-001");
            Assert.Contains(result, x => x.NoBPKB == "A-002");
        }

        [Fact]
        public void GetView_ById_ShouldFilterCorrectly()
        {
            var id = Guid.NewGuid();
            var list = new List<BpkbView>
            {
                new BpkbView { Id = id, NoBPKB = "B-999" },
                new BpkbView { Id = Guid.NewGuid(), NoBPKB = "B-111" },
            }.AsQueryable();

            _bpkbViewRepo.Setup(r => r.Fetch()).Returns(list);

            var item = _service.GetView(id);

            Assert.NotNull(item);
            Assert.Equal("B-999", item.NoBPKB);
        }

        [Fact]
        public void GetByNoreg_ShouldReturnOnlyMatchingNoReg()
        {
            var data = new List<Bpkb>
            {
                new Bpkb { Id = Guid.NewGuid(), NoReg = "EMP001", NoBPKB = "X-1" },
                new Bpkb { Id = Guid.NewGuid(), NoReg = "EMP002", NoBPKB = "X-2" },
                new Bpkb { Id = Guid.NewGuid(), NoReg = "EMP001", NoBPKB = "X-3" },
            }.AsQueryable();

            _bpkbRepo.Setup(r => r.Fetch()).Returns(data);

            var result = _service.GetByNoreg("EMP001").ToList();

            Assert.Equal(2, result.Count);
            Assert.All(result, x => Assert.Equal("EMP001", x.NoReg));
        }

        [Fact]
        public void GetDetails_ShouldJoinAndFilter()
        {
            var parentId = Guid.NewGuid();
            var noreg = "EMP007";

            _bpkbRepo.Setup(r => r.Fetch())
                .Returns(new List<Bpkb> { new Bpkb { Id = parentId, NoReg = noreg } }.AsQueryable());

            var reqOk = new BpkbRequest
            {
                Id = Guid.NewGuid(),
                BPKBId = parentId,
                NoReg = noreg,
                RowStatus = true,
                RequestType = "borrow",
                BorrowDate = new DateTime(2024, 1, 2),
                ReturnDate = new DateTime(2024, 1, 10),
                BorrowPurpose = "Pinjam untuk proses"
            };
            var reqOther = new BpkbRequest
            {
                Id = Guid.NewGuid(),
                BPKBId = Guid.NewGuid(),
                NoReg = "EMP999",
                RowStatus = true,
                RequestType = "borrow",
                BorrowDate = new DateTime(2024, 1, 5),
                ReturnDate = new DateTime(2024, 1, 6),
                BorrowPurpose = "Tidak relevan"
            };
            _bpkbRequestRepo.Setup(r => r.Fetch())
                .Returns(new List<BpkbRequest> { reqOk, reqOther }.AsQueryable());

            _userRepo.Setup(r => r.Fetch())
                .Returns(new List<User> { new User { NoReg = noreg, Name = "Alice" } }.AsQueryable());

            var rows = _service.GetDetails(parentId).ToList();

            Assert.Single(rows);

            var obj = rows.Single();
            var t = obj.GetType();

            Assert.Equal(reqOk.Id, (Guid)t.GetProperty("Id")!.GetValue(obj));
            Assert.Equal(noreg, (string)t.GetProperty("NoReg")!.GetValue(obj));
            Assert.Equal("Alice", (string)t.GetProperty("Name")!.GetValue(obj));
            Assert.Equal("borrow", (string)t.GetProperty("RequestType")!.GetValue(obj));
            Assert.Equal(reqOk.BorrowDate, (DateTime?)t.GetProperty("BorrowDate")!.GetValue(obj));
            Assert.Equal(reqOk.ReturnDate, (DateTime?)t.GetProperty("ReturnDate")!.GetValue(obj));
            Assert.Equal("Pinjam untuk proses", (string)t.GetProperty("BorrowPurpose")!.GetValue(obj));
        }
    }
}
