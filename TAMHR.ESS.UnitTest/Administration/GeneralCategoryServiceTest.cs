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
using TAMHR.ESS.UnitTest.MasterData;
using Xunit;
using static TAMHR.ESS.WebUI.Areas.OHS.TanyaOhsHelper;

namespace TAMHR.ESS.UnitTest.Administration
{
    public class GeneralCategoryServiceTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Mock<IRepository<GeneralCategory>> _mockGeneralCategory;
        private readonly Mock<IRepository<GeneralCategoryMapping>> _mockGmMapping;
        private readonly ConfigService _service;

        public GeneralCategoryServiceTest()
        {
            _unitOfWork = new Mock<IUnitOfWork>();
            _mockGeneralCategory = new Mock<IRepository<GeneralCategory>>();
            _mockGmMapping = new Mock<IRepository<GeneralCategoryMapping>>();

            _unitOfWork.Setup(u => u.GetRepository<GeneralCategory>())
                           .Returns(_mockGeneralCategory.Object);
            _unitOfWork.Setup(u => u.GetRepository<GeneralCategoryMapping>())
                           .Returns(_mockGmMapping.Object);

            _unitOfWork.Setup(u => u.Transact(It.IsAny<Action>(), It.IsAny<System.Data.IsolationLevel>()))
    .Callback<Action, System.Data.IsolationLevel>((action, _) => action());

            _unitOfWork.Setup(u => u.SaveChanges());

            _service = new ConfigService(_unitOfWork.Object);
        }

        [Fact]
        public void Upsert_ShouldCallRepositoryUpsert_AndSaveChanges()
        {
            // Arrange
            var model = new GeneralCategory { Id = Guid.NewGuid() };

            // Act
            _service.UpsertGeneralCategory(model);

            // Assert
            _mockGeneralCategory.Verify(r => r.Upsert<Guid>(model, It.IsAny<string[]>()), Times.Once);
            _unitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Fact]
        public void Map_ShouldCallRepositoryUpsert_AndSaveChanges()
        {
            // Arrange
            var model = new GeneralCategoryMapping { Id = Guid.NewGuid() };

            // Act
            _service.UpsertGeneralCategoryMapping(model);

            // Assert
            _mockGmMapping.Verify(r => r.Upsert<Guid>(model, It.IsAny<string[]>()), Times.Once);
            _unitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Fact]
        public void DeleteById_ShouldCallRepositoryUpsert_AndSaveChanges()
        {
            // Arrange
            var id = Guid.NewGuid();
            var generalCategory = new GeneralCategory { Id = id, Readonly = false, Code = "GC001" };

            _mockGeneralCategory.Setup(r => r.FindById(id)).Returns(generalCategory);

            _unitOfWork.Setup(u => u.SaveChanges()).Returns(1);

            // Act
            var result = _service.DeleteGeneralCategory(id);

            // Assert
            _mockGeneralCategory.Verify(r => r.Delete(generalCategory), Times.Once);
            _unitOfWork.Verify(u => u.SaveChanges(), Times.Once);
            Assert.True(result);
        }
    }
}
