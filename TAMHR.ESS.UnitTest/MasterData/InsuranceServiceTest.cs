using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Xunit;

using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;

using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;

namespace TAMHR.ESS.UnitTest.MasterData
{
    public class InsuranceServiceTest
    {
        private readonly Mock<IUnitOfWork> _uow;
        private readonly Mock<IRepository<PersonalDataInsurance>> _entityRepo;
        private readonly Mock<IReadonlyRepository<PersonalDataInsuranceView>> _viewRepoRo;   // readonly face
        private readonly Mock<IRepository<PersonalDataInsuranceView>> _viewRepoRw;           // write face (via .As<>)
        private readonly InsuranceService _service;

        public InsuranceServiceTest()
        {
            _uow = new Mock<IUnitOfWork>();

            // Base entity repo (dibutuhkan oleh GenericDomainServiceBase<T>)
            _entityRepo = new Mock<IRepository<PersonalDataInsurance>>();
            _uow.Setup(u => u.GetRepository<PersonalDataInsurance>())
                .Returns(_entityRepo.Object);

            // View repo: buat mock yang implement *dua* interface sekaligus
            _viewRepoRo = new Mock<IReadonlyRepository<PersonalDataInsuranceView>>();
            _viewRepoRw = _viewRepoRo.As<IRepository<PersonalDataInsuranceView>>();

            // Kembalikan *write face* karena signature GetRepository<T>() di proyek test kamu mengembalikan IRepository<T>
            _uow.Setup(u => u.GetRepository<PersonalDataInsuranceView>())
                .Returns(_viewRepoRw.Object);

            _service = new InsuranceService(_uow.Object);
        }

        [Fact]
        public void GetInsruances_Should_Return_Only_Items_With_MemberNumber()
        {
            // Arrange
            var data = new List<PersonalDataInsuranceView>
            {
                new PersonalDataInsuranceView { MemberNumber = "MN-001" },
                new PersonalDataInsuranceView { MemberNumber = "" },
                new PersonalDataInsuranceView { MemberNumber = null }
            }.AsQueryable();

            // Pastikan kedua interface (RO & RW) mengembalikan sumber data yang sama
            _viewRepoRo.Setup(r => r.Fetch()).Returns(data);
            _viewRepoRw.Setup(r => r.Fetch()).Returns(data);

            // Act
            var result = _service.GetInsruances().ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal("MN-001", result[0].MemberNumber);
        }

        [Fact]
        public void GetInsruances_When_Empty_Returns_Empty()
        {
            // Arrange
            var empty = new List<PersonalDataInsuranceView>().AsQueryable();
            _viewRepoRo.Setup(r => r.Fetch()).Returns(empty);
            _viewRepoRw.Setup(r => r.Fetch()).Returns(empty);

            // Act
            var result = _service.GetInsruances().ToList();

            // Assert
            Assert.Empty(result);
        }
    }
}
