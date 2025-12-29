using System;
using System.Linq;
using Moq;
using Xunit;

using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;

namespace TAMHR.ESS.UnitTest.MasterData
{
    public class BdjkDetailServiceTest
    {
        private readonly Mock<IUnitOfWork> _uow = new Mock<IUnitOfWork>();
        private readonly Mock<IRepository<BdjkDetail>> _repo = new Mock<IRepository<BdjkDetail>>();
        private readonly BdjkDetailService _service;

        public BdjkDetailServiceTest()
        {
            _uow.Setup(u => u.GetRepository<BdjkDetail>()).Returns(_repo.Object);
            _uow.Setup(u => u.SaveChanges()).Returns(1);

            _service = new BdjkDetailService(_uow.Object);
        }

        [Fact]
        public void Upsert_Should_Call_Repo_Upsert_Then_Save()
        {
            var model = new BdjkDetail
            {
                Id = Guid.NewGuid(),
                BdjkCode = "BD01",
                Description = "BDJK Desc",

                // === pilih salah satu sesuai tipe di entity ===
                // BdjkValue = 1.5m,     // jika tipe decimal
                BdjkValue = "1.5",       // jika tipe string

                ClassFrom = "A",
                ClassTo = "B",
                FlagHoliday = true,       // bool? -> pakai true/false (BUKAN 1/0)
                FlagDuration = 0,     // bool? -> pakai true/false
                RowStatus = true
            };

            var expectedProps = new[]
            {
                "BdjkCode",
                "Description",
                "BdjkValue",
                "ClassFrom",
                "ClassTo",
                "FlagHoliday",
                "FlagDuration"
            };

            _service.Upsert(model);

            _repo.Verify(r => r.Upsert<Guid>(
                    It.Is<BdjkDetail>(d => d == model),
                    It.Is<string[]>(p => p.SequenceEqual(expectedProps))
                ),
                Times.Once);

            _uow.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Fact]
        public void DeleteById_Should_Call_Repo_DeleteById_Then_Save()
        {
            var id = Guid.NewGuid();

            _service.DeleteById(id);

            _repo.Verify(r => r.DeleteById(id), Times.Once);
            _uow.Verify(u => u.SaveChanges(), Times.Once);
        }
    }
}
