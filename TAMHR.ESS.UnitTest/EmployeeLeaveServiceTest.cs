using Agit.Domain.Extensions;
using Agit.Domain.UnitOfWork;
using Moq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.ViewModels;
using Xunit;

namespace TAMHR.ESS.UnitTest
{
    public class EmployeeLeaveServiceTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly EmployeeLeaveService _employeeLeaveService;

        public EmployeeLeaveServiceTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _employeeLeaveService = new EmployeeLeaveService(_mockUnitOfWork.Object);
        }

        [Fact]
        public void AddLeaveEmployee()
        {
            // Arrange
            var model = new AddEmployeeLeaveViewModel
            {
                noreg = "EMP001",
                TotalLeave = 12,
                UsedLeave = 0,
                RemainingLeave = 12,
                Period = "2023",
                ModifiedBy = "admin",
                TotalLongLeave = 30,
                UsedLongLeave = 0,
                RemainingLongLeave = 30,
                PeriodLongLeave = "2023"
            };

            
            var result = _employeeLeaveService.AddLeaveEmployee(model);

            // Assert
            Assert.True(result);
            
            
        }

        [Fact]
        public void UpdateLeave()
        {
            // Arrange
            var model = new EmployeeLeaveViewModel
            {
                noreg = "EMP001",
                TotalLeave = 12,
                Period = "2023",
                ModifiedBy = "admin",
                UsedLeave = 5,
                RemainingLeave = 7
            };

          

            // Act
            var result = _employeeLeaveService.UpdateLeave(model);

            // Assert
            Assert.True(result);
           
            
        }

        [Fact]
        public void UpdateLongLeave()
        {
            // Arrange
            var model = new EmployeeLeaveViewModel
            {
                noreg = "EMP001",
                TotalLeave = 30,
                Period = "2023",
                ModifiedBy = "admin",
                UsedLeave = 10,
                RemainingLeave = 20
            };

            // Act
            var result = _employeeLeaveService.UpdateLongLeave(model);

            // Assert
            Assert.True(result);
            
            
        }

       

        
    }
}

