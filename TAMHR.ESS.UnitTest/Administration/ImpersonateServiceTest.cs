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
    public class ImpersonateServiceTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Mock<IRepository<UserImpersonation>> _mockImpersonate;
        private readonly CoreService _service;

        public ImpersonateServiceTest()
        {
            _unitOfWork = new Mock<IUnitOfWork>();
            _mockImpersonate = new Mock<IRepository<UserImpersonation>>();
            _service = new CoreService(_unitOfWork.Object);

            _unitOfWork.Setup(u => u.GetRepository<UserImpersonation>())
                           .Returns(_mockImpersonate.Object);

            _unitOfWork.Setup(u => u.Transact(It.IsAny<Action>(), It.IsAny<System.Data.IsolationLevel>()))
                        .Callback<Action, System.Data.IsolationLevel>((action, _) => action());

            _unitOfWork.Setup(u => u.SaveChanges());
        }

        [Fact]
        public void Upsert_ShouldCallRepositoryUpsert_AndSaveChanges()
        {
            // Arrange
            var model = new UserImpersonation { Id = Guid.NewGuid() };

            // Act
            _service.UpsertUserImpersonation(model);

            // Assert
            _mockImpersonate.Verify(r => r.Upsert<Guid>(model, It.IsAny<string[]>()), Times.Once);
            _unitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Fact]
        public void DeleteById_ShouldCallRepositoryUpsert_AndSaveChanges()
        {
            // Arrange
            var id = Guid.NewGuid();
            var userImpersonation = new UserImpersonation { Id = id};

            _mockImpersonate.Setup(r => r.FindById(id)).Returns(userImpersonation);

            _unitOfWork.Setup(u => u.SaveChanges()).Returns(1);

            // Act
            var result = _service.DeleteUserImpersonation(id);

            // Assert
            _mockImpersonate.Verify(r => r.DeleteById(id), Times.Once);
            _unitOfWork.Verify(u => u.SaveChanges(), Times.Once);
            Assert.True(result);
        }
    }
}
