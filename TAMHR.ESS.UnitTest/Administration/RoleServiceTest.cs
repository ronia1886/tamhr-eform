using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;
using Moq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.ViewModels;
using Xunit;
using Z.EntityFramework.Plus;

namespace TAMHR.ESS.UnitTest.Administration
{
    public class RoleServiceTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Mock<IRepository<Role>> _mockRole;
        private readonly Mock<IRepository<RoleViewModel>> _mockRoleViewModel;
        private readonly Mock<IRepository<RolePermission>> _mockRolePermission;
        private readonly Mock<IRepository<AccessRole>> _mockAccessRole;
        private readonly Mock<IRepository<UserRole>> _mockUserRole;
        private readonly CoreService _service;

        public RoleServiceTest()
        {
            _unitOfWork = new Mock<IUnitOfWork>();
            _mockRoleViewModel = new Mock<IRepository<RoleViewModel>>();
            _mockRole = new Mock<IRepository<Role>>();
            _mockRolePermission = new Mock<IRepository<RolePermission>>();
            _mockAccessRole = new Mock<IRepository<AccessRole>>();
            _mockUserRole = new Mock<IRepository<UserRole>>();
            _service = new CoreService(_unitOfWork.Object);

            // Setup repository returns
            _unitOfWork.Setup(u => u.GetRepository<RoleViewModel>())
                           .Returns(_mockRoleViewModel.Object);
            _unitOfWork.Setup(u => u.GetRepository<Role>())
                           .Returns(_mockRole.Object);
            _unitOfWork.Setup(u => u.GetRepository<RolePermission>())
                           .Returns(_mockRolePermission.Object);
            _unitOfWork.Setup(u => u.GetRepository<AccessRole>())
                           .Returns(_mockAccessRole.Object);
            _unitOfWork.Setup(u => u.GetRepository<UserRole>())
                           .Returns(_mockUserRole.Object);

            // Simulate transaction behavior
            _unitOfWork.Setup(u => u.Transact(It.IsAny<Action>(), It.IsAny<System.Data.IsolationLevel>()))
               .Callback<Action, System.Data.IsolationLevel>((action, _) => action());

            // SaveChanges dummy
            _unitOfWork.Setup(u => u.SaveChanges()).Returns(1);
        }

        [Fact]
        public void UpsertRole_Should_AddNewRole_And_SaveChanges()
        {
            // Arrange
            var mockRoleRepo = new Mock<IRepository<Role>>();
            var mockRolePermissionRepo = new Mock<IRepository<RolePermission>>();
            var mockUow = new Mock<IUnitOfWork>();

            var viewModel = new RoleViewModel
            {
                Id = Guid.Empty, // role baru
                Title = "Admin",
                Description = "Admin Role",
                RoleTypeCode = "ADM",
                RoleKey = "ADMIN"
            };

            // Setup UoW
            mockUow.Setup(u => u.GetRepository<Role>()).Returns(mockRoleRepo.Object);
            mockUow.Setup(u => u.GetRepository<RolePermission>()).Returns(mockRolePermissionRepo.Object);
            mockUow.Setup(u => u.Transact(It.IsAny<Action>(), It.IsAny<System.Data.IsolationLevel>()))
               .Callback<Action, System.Data.IsolationLevel>((action, _) => action());

            // Simulasi role tidak ketemu
            mockRoleRepo.Setup(r => r.FindById(viewModel.Id)).Returns((Role)null);

            var service = new CoreService(mockUow.Object);

            // Act
            service.UpsertRole(viewModel);

            mockUow.Verify(u => u.SaveChanges(), Times.AtLeastOnce);
        }

        [Fact]
        public void DeleteRole_ShouldCallRepositoryDeleteById_AndSaveChanges()
        {
            // Arrange
            var id = Guid.NewGuid();


            // UnitOfWork Transact
            _unitOfWork.Setup(u => u.Transact(It.IsAny<Action>(), It.IsAny<System.Data.IsolationLevel>()))
                       .Callback<Action, System.Data.IsolationLevel>((action, _) => action());

            _unitOfWork.Setup(u => u.SaveChanges()).Returns(1);

            // Act
            _service.DeleteRole(id);

            // Assert
            _mockRole.Verify(r => r.DeleteById(id), Times.Once);
            _unitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }
    }
}
