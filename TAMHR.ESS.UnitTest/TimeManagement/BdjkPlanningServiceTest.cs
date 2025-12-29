using Agit.Domain.Extensions;
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
using Xunit;
using static TAMHR.ESS.WebUI.Areas.OHS.TanyaOhsHelper;

namespace TAMHR.ESS.UnitTest.TimeManagementTest
{
    public class BdjkPlanningServiceTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Mock<IRepository<BdjkRequestStoredEntity>> _mockBdjkRequest;
        private readonly BdjkService _bdjkService;

        public BdjkPlanningServiceTest()
        {
            _unitOfWork = new Mock<IUnitOfWork>();
            _mockBdjkRequest = new Mock<IRepository<BdjkRequestStoredEntity>>();

            _unitOfWork.Setup(u => u.GetRepository<BdjkRequestStoredEntity>()).Returns(_mockBdjkRequest.Object);

            _bdjkService = new BdjkService(_unitOfWork.Object);
        }

        
    }
}
