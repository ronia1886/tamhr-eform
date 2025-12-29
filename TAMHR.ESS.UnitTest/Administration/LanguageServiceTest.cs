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
    public class LanguageServiceTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Mock<IRepository<Language>> _mockLanguage;
        private readonly CoreService _service;

        public LanguageServiceTest()
        {
            _unitOfWork = new Mock<IUnitOfWork>();
            _mockLanguage = new Mock<IRepository<Language>>();
            _service = new CoreService(_unitOfWork.Object);

            _unitOfWork.Setup(u => u.GetRepository<Language>())
                           .Returns(_mockLanguage.Object);

            _unitOfWork.Setup(u => u.Transact(It.IsAny<Action>(), It.IsAny<System.Data.IsolationLevel>()))
                        .Callback<Action, System.Data.IsolationLevel>((action, _) => action());

            _unitOfWork.Setup(u => u.SaveChanges());
        }

        [Fact]
        public void CreateLanguage()
        {
            // Arrange
            var model = new Language { Id = Guid.NewGuid() };

            // Act
            _service.UpsertLanguage(model);

            // Assert
            _mockLanguage.Verify(r => r.Upsert<Guid>(model, It.IsAny<string[]>()), Times.Once);
            _unitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Fact]
        public void UpdateLangueage()
        {
            // Arrange
            var model = new Language { Id = Guid.NewGuid() };

            // Act
            _service.UpsertLanguage(model);

            // Assert
            _mockLanguage.Verify(r => r.Upsert<Guid>(model, It.IsAny<string[]>()), Times.Once);
            _unitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Fact]
        public void DeleteById_Language()
        {
            // Arrange
            var id = Guid.NewGuid();
            var userImpersonation = new Language { Id = id };

            _mockLanguage.Setup(r => r.FindById(id)).Returns(userImpersonation);

            _unitOfWork.Setup(u => u.SaveChanges()).Returns(1);

            // Act
            _service.DeleteLanguage(id);

            // Assert
            _mockLanguage.Verify(r => r.DeleteById(id), Times.Once);
            _unitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }
    }
}
