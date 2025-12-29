using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;
using Xunit;

namespace TAMHR.ESS.UnitTest.Administration
{
    public class SapCategoryMapServiceTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IRepository<SapGeneralCategoryMap>> _mockRepo;
        private readonly TestSapGeneralCategoryMapService _service;

        public SapCategoryMapServiceTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockRepo = new Mock<IRepository<SapGeneralCategoryMap>>();

            _mockUnitOfWork.Setup(u => u.GetRepository<SapGeneralCategoryMap>())
                           .Returns(_mockRepo.Object);

            _mockUnitOfWork.Setup(u => u.SaveChanges());

            _service = new TestSapGeneralCategoryMapService(_mockUnitOfWork.Object);
        }

        [Fact]
        public void Upsert_ShouldCallRepositoryUpsert_AndSaveChanges()
        {
            // Arrange
            var model = new SapGeneralCategoryMap { Id = Guid.NewGuid() };

            // Act
            _service.Upsert(model);

            // Assert
            _mockRepo.Verify(r => r.Upsert<Guid>(model, It.IsAny<string[]>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Fact]
        public void Delete_ShouldCallRepositoryUpsert_AndSaveChanges()
        {
            // Arrange
            var Id = Guid.NewGuid();

            // Act
            _service.DeleteById(Id);

            // Assert
            _mockRepo.Verify(r => r.DeleteById(Id), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }
    }

    public class TestSapGeneralCategoryMapService : SapGeneralCategoryMapService
    {
        public TestSapGeneralCategoryMapService(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        protected override string[] Properties => Array.Empty<string>();
    }
}
