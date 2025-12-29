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
    public class PermissionServiceTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Mock<IRepository<Permission>> _mockPermission;
        private readonly Mock<IRepository<RolePermission>> _mockRolePermission;
        private readonly Mock<IRepository<Menu>> _mockMenu;
        private readonly Mock<IRepository<FavouriteMenu>> _mockFavMenu;
        private readonly CoreService _service;

        public PermissionServiceTest()
        {
            _unitOfWork = new Mock<IUnitOfWork>();
            _mockPermission = new Mock<IRepository<Permission>>();
            _mockRolePermission = new Mock<IRepository<RolePermission>>();
            _mockMenu = new Mock<IRepository<Menu>>();
            _mockFavMenu = new Mock<IRepository<FavouriteMenu>>();
            _service = new CoreService(_unitOfWork.Object);

            _unitOfWork.Setup(u => u.GetRepository<Permission>())
                           .Returns(_mockPermission.Object);
            _unitOfWork.Setup(u => u.GetRepository<RolePermission>())
                           .Returns(_mockRolePermission.Object);
            _unitOfWork.Setup(u => u.GetRepository<Menu>())
                           .Returns(_mockMenu.Object);
            _unitOfWork.Setup(u => u.GetRepository<FavouriteMenu>())
                           .Returns(_mockFavMenu.Object);

            _unitOfWork.Setup(u => u.Transact(It.IsAny<Action>(), It.IsAny<System.Data.IsolationLevel>()))
                        .Callback<Action, System.Data.IsolationLevel>((action, _) => action());

            _unitOfWork.Setup(u => u.SaveChanges());
        }

        [Fact]
        public void CreatePermission()
        {
            // Arrange
            var model = new Permission { Id = Guid.NewGuid() };

            // Act
            _service.UpsertPermission(model);

            // Assert
            _mockPermission.Verify(r => r.Upsert<Guid>(model, It.IsAny<string[]>()), Times.Once);
            _unitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Fact]
        public void UpdatePermission()
        {
            // Arrange
            var model = new Permission { Id = Guid.NewGuid() };

            // Act
            _service.UpsertPermission(model);

            // Assert
            _mockPermission.Verify(r => r.Upsert<Guid>(model, It.IsAny<string[]>()), Times.Once);
            _unitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Fact]
        public void DeleteById_Permission()
        {
            // Arrange
            var id = Guid.NewGuid();


            // UnitOfWork Transact
            _unitOfWork.Setup(u => u.Transact(It.IsAny<Action>(), It.IsAny<System.Data.IsolationLevel>()))
                       .Callback<Action, System.Data.IsolationLevel>((action, _) => action());

            _unitOfWork.Setup(u => u.SaveChanges()).Returns(1);

            // Act
            var result = _service.DeletePermission(id);

            // Assert
            Assert.True(result);
            _mockPermission.Verify(r => r.DeleteById(id), Times.Once);
            _unitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }
    }
}
