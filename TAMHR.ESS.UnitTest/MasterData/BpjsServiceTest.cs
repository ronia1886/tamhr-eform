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
    public class BpjsServiceTest
    {
        private readonly Mock<IUnitOfWork> _uow;
        private readonly Mock<IRepository<PersonalDataBpjs>> _entityRepo;

        // Satu mock untuk view repo yang punya 2 “wajah”: readonly & repo biasa
        private readonly Mock<IReadonlyRepository<PersonalDataBpjsView>> _viewRepoRo;
        private readonly Mock<IRepository<PersonalDataBpjsView>> _viewRepoRw;

        private readonly BpjsService _service;

        public BpjsServiceTest()
        {
            _uow = new Mock<IUnitOfWork>();

            // repo entity dasar (dibutuhkan GenericDomainServiceBase<T>)
            _entityRepo = new Mock<IRepository<PersonalDataBpjs>>();
            _uow.Setup(u => u.GetRepository<PersonalDataBpjs>())
                .Returns(_entityRepo.Object);

            // repo view: implement keduanya agar tidak bentrok signature
            _viewRepoRo = new Mock<IReadonlyRepository<PersonalDataBpjsView>>();
            _viewRepoRw = _viewRepoRo.As<IRepository<PersonalDataBpjsView>>();

            // kembalikan “repo biasa” karena signature GetRepository<T> mengembalikan IRepository<T>
            _uow.Setup(u => u.GetRepository<PersonalDataBpjsView>())
                .Returns(_viewRepoRw.Object);

            _service = new BpjsService(_uow.Object);
        }

        [Fact]
        public void GetBpjs_Should_Return_Only_Items_With_BpjsNumber()
        {
            // Arrange
            var data = new List<PersonalDataBpjsView>
            {
                new PersonalDataBpjsView { BpjsNumber = "BPJS-001" },
                new PersonalDataBpjsView { BpjsNumber = "" },
                new PersonalDataBpjsView { BpjsNumber = null }
            }.AsQueryable();

            // Pastikan kedua interface mengembalikan sumber data yang sama
            _viewRepoRo.Setup(r => r.Fetch()).Returns(data);
            _viewRepoRw.Setup(r => r.Fetch()).Returns(data);

            // Act
            var result = _service.GetBpjs().ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal("BPJS-001", result[0].BpjsNumber);
        }

        [Fact]
        public void GetBpjs_When_Empty_Returns_Empty()
        {
            // Arrange
            var empty = new List<PersonalDataBpjsView>().AsQueryable();
            _viewRepoRo.Setup(r => r.Fetch()).Returns(empty);
            _viewRepoRw.Setup(r => r.Fetch()).Returns(empty);

            // Act
            var result = _service.GetBpjs().ToList();

            // Assert
            Assert.Empty(result);
        }
    }
}
