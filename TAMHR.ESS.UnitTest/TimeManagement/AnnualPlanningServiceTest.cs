using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Xunit;

using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;

using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;

namespace TAMHR.ESS.UnitTest.TimeManagementTest
{
    public class AnnualPlanningServiceTest
    {
        private readonly Mock<IUnitOfWork> _uow = new Mock<IUnitOfWork>();

        private readonly Mock<IRepository<PersonalAnnualPlanningDashboardView>> _personalRepo
            = new Mock<IRepository<PersonalAnnualPlanningDashboardView>>();

        private readonly Mock<IRepository<SubordinateAnnualPlanningDashboardView>> _subRepo
            = new Mock<IRepository<SubordinateAnnualPlanningDashboardView>>();

        private readonly Mock<IRepository<AnnualLeavePlanning>> _alpRepo
            = new Mock<IRepository<AnnualLeavePlanning>>();

        private readonly Mock<IRepository<AnnualLeavePlanningDetail>> _alpDetailRepo
            = new Mock<IRepository<AnnualLeavePlanningDetail>>();

        private readonly AnnualPlanningService _service;

        public AnnualPlanningServiceTest()
        {
            _uow.Setup(u => u.GetRepository<PersonalAnnualPlanningDashboardView>())
                .Returns(_personalRepo.Object);

            _uow.Setup(u => u.GetRepository<SubordinateAnnualPlanningDashboardView>())
                .Returns(_subRepo.Object);

            _uow.Setup(u => u.GetRepository<AnnualLeavePlanning>())
                .Returns(_alpRepo.Object);

            _uow.Setup(u => u.GetRepository<AnnualLeavePlanningDetail>())
                .Returns(_alpDetailRepo.Object);

            _service = new AnnualPlanningService(_uow.Object);
        }

        [Fact]
        public void GetPersonalAnnualPlanningDashboard_Filters_By_NoReg_Year_And_FormKey()
        {
            var data = new List<PersonalAnnualPlanningDashboardView>
            {
                new PersonalAnnualPlanningDashboardView { NoReg = "E001", YearPeriod = 2025, FormKey = "annual-ot-planning" },
                new PersonalAnnualPlanningDashboardView { NoReg = "E001", YearPeriod = 2025, FormKey = "annual-leave-planning" },
                new PersonalAnnualPlanningDashboardView { NoReg = "E001", YearPeriod = 2024, FormKey = "annual-ot-planning" },
                new PersonalAnnualPlanningDashboardView { NoReg = "E002", YearPeriod = 2025, FormKey = "annual-ot-planning" },
            }.AsQueryable();

            _personalRepo.Setup(r => r.Fetch()).Returns(data);

            var result = _service.GetPersonalAnnualPlanningDashboard("E001", 2025, "annual-ot-planning").ToList();

            Assert.Single(result);
            Assert.Equal("E001", result[0].NoReg);
            Assert.Equal(2025, result[0].YearPeriod);
            Assert.Equal("annual-ot-planning", result[0].FormKey);
        }

        [Fact]
        public void GetSubordinateAnnualPlanningDashboard_Applies_Inclusion_Logic_And_Filters()
        {
            var data = new List<SubordinateAnnualPlanningDashboardView>
            {
                // masuk: superior match, status != draft
                new SubordinateAnnualPlanningDashboardView { SuperiorNoReg = "SUP1", NoReg = "E010", DocumentStatusCode = "completed", FormKey = "annual-ot-planning", YearPeriod = 2025 },

                // tidak: status draft
                new SubordinateAnnualPlanningDashboardView { SuperiorNoReg = "SUP1", NoReg = "E011", DocumentStatusCode = "draft", FormKey = "annual-ot-planning", YearPeriod = 2025 },

                // masuk: dirinya sendiri (NoReg==SUP1) & formKey != annual-leave-planning
                new SubordinateAnnualPlanningDashboardView { SuperiorNoReg = "SUPX", NoReg = "SUP1", DocumentStatusCode = "completed", FormKey = "annual-ot-planning", YearPeriod = 2025 },

                // tidak: dirinya sendiri tapi formKey annual-leave-planning
                new SubordinateAnnualPlanningDashboardView { SuperiorNoReg = "SUPX", NoReg = "SUP1", DocumentStatusCode = "completed", FormKey = "annual-leave-planning", YearPeriod = 2025 },

                // tahun lain
                new SubordinateAnnualPlanningDashboardView { SuperiorNoReg = "SUP1", NoReg = "E012", DocumentStatusCode = "completed", FormKey = "annual-ot-planning", YearPeriod = 2024 },
            }.AsQueryable();

            _subRepo.Setup(r => r.Fetch()).Returns(data);

            var result = _service.GetSubordinateAnnualPlanningDashboard("SUP1", 2025, "annual-ot-planning").ToList();

            Assert.Equal(2, result.Count);
            Assert.All(result, x =>
            {
                Assert.Equal(2025, x.YearPeriod);
                Assert.Equal("annual-ot-planning", x.FormKey);
            });
        }

        [Fact]
        public void GetYearPeriod_Returns_Distinct_Union_From_Personal_And_Subordinate()
        {
            // gunakan noReg yang sama untuk personal & subordinate: "SUP1"
            var personal = new List<PersonalAnnualPlanningDashboardView>
            {
                new PersonalAnnualPlanningDashboardView { NoReg = "SUP1", YearPeriod = 2024, FormKey = "annual-ot-planning" },
                new PersonalAnnualPlanningDashboardView { NoReg = "SUP1", YearPeriod = 2025, FormKey = "annual-leave-planning" },
                // baris lain NoReg berbeda tidak akan ikut
                new PersonalAnnualPlanningDashboardView { NoReg = "E002", YearPeriod = 2026, FormKey = "annual-ot-planning" },
            }.AsQueryable();

            var sub = new List<SubordinateAnnualPlanningDashboardView>
            {
                // diakui karena SuperiorNoReg == "SUP1"
                new SubordinateAnnualPlanningDashboardView { SuperiorNoReg = "SUP1", NoReg = "E010", DocumentStatusCode = "completed", FormKey = "annual-ot-planning", YearPeriod = 2023 },

                // diakui karena SuperiorNoReg == "SUP1"
                new SubordinateAnnualPlanningDashboardView { SuperiorNoReg = "SUP1", NoReg = "E011", DocumentStatusCode = "completed", FormKey = "annual-ot-planning", YearPeriod = 2022 },

                // ini tidak relevan (NoReg/SuperiorNoReg lain)
                new SubordinateAnnualPlanningDashboardView { SuperiorNoReg = "SUPX", NoReg = "SUPX", DocumentStatusCode = "completed", FormKey = "annual-ot-planning", YearPeriod = 2027 },
            }.AsQueryable();

            _personalRepo.Setup(r => r.Fetch()).Returns(personal);
            _subRepo.Setup(r => r.Fetch()).Returns(sub);

            var years = _service.GetYearPeriod("SUP1", "POSTX").OrderBy(x => x).ToList();

            Assert.Equal(new[] { 2022, 2023, 2024, 2025 }, years);
        }

        [Fact]
        public void GetAnnualLeavePlanningDetail_Returns_Details_For_Found_Plan()
        {
            var planId = Guid.NewGuid();

            var plans = new List<AnnualLeavePlanning>
            {
                new AnnualLeavePlanning { Id = planId, NoReg = "E001", YearPeriod = 2025 },
                new AnnualLeavePlanning { Id = Guid.NewGuid(), NoReg = "E002", YearPeriod = 2025 }
            }.AsQueryable();

            var details = new List<AnnualLeavePlanningDetail>
            {
                new AnnualLeavePlanningDetail { Id = Guid.NewGuid(), AnnualLeavePlanningId = planId },
                new AnnualLeavePlanningDetail { Id = Guid.NewGuid(), AnnualLeavePlanningId = planId },
                // milik plan lain
                new AnnualLeavePlanningDetail { Id = Guid.NewGuid(), AnnualLeavePlanningId = Guid.NewGuid() },
            }.AsQueryable();

            _alpRepo.Setup(r => r.Fetch()).Returns(plans);
            _alpDetailRepo.Setup(r => r.Fetch()).Returns(details);

            var result = _service.GetAnnualLeavePlanningDetail(2025, "E001").ToList();

            Assert.Equal(2, result.Count);
            Assert.All(result, x => Assert.Equal(planId, x.AnnualLeavePlanningId));
        }
    }
}
