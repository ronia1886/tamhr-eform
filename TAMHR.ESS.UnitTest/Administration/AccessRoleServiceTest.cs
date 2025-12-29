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
using static TAMHR.ESS.WebUI.Areas.OHS.TanyaOhsHelper;

namespace TAMHR.ESS.UnitTest.Administration
{
    public class AccessRoleServiceTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWork;

        public AccessRoleServiceTest()
        {
            _unitOfWork = new Mock<IUnitOfWork>();
        }

        [Fact]
        public void UpsertAccessRole_Should_Call_Upsert_And_SaveChanges()
        {
            // Arrange
            var model = new AccessRole { Id = Guid.NewGuid(), RowStatus = true };
            var mockRepo = new Mock<IRepository<AccessRole>>();
            var mockUnitOfWork = new Mock<IUnitOfWork>();

            // Mock repository method
            mockUnitOfWork.Setup(u => u.GetRepository<AccessRole>()).Returns(mockRepo.Object);

            var service = new AccessRoleService(mockUnitOfWork.Object);

            // Act
            service.Upsert(model);

            // Assert
            mockRepo.Verify(r => r.Upsert<Guid>(It.IsAny<AccessRole>(), It.IsAny<string[]>()), Times.Once);
            mockUnitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Fact]
        public void DeleteAccessRole()
        {
            // Arrange
            var id = Guid.NewGuid();
            var mockRepo = new Mock<IRepository<AccessRole>>();
            var mockUnitOfWork = new Mock<IUnitOfWork>();

            mockUnitOfWork.Setup(u => u.GetRepository<AccessRole>()).Returns(mockRepo.Object);

            var service = new AccessRoleService(mockUnitOfWork.Object);

            // Act
            service.Delete(id);

            // Assert
            mockRepo.Verify(r => r.DeleteById(id), Times.Once);
            mockUnitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }
    }
}
