using Agit.Common.Extensions;
using Agit.Domain;
using Agit.Domain.Extensions;
using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.ViewModels;
using Z.EntityFramework.Plus;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    public class EmployeeWorkPlanService : DomainServiceBase
    {
        #region Domain Repositories
        /// <summary>
        /// Employee Work Plan repository object.
        /// </summary>
        protected IReadonlyRepository<GeneralCategory> GeneralCategoryReadOnlyRepository => UnitOfWork.GetRepository<GeneralCategory>();
        protected IRepository<WorkDivisionPlan> WorkDivisionPlanRepository => UnitOfWork.GetRepository<WorkDivisionPlan>();
        #endregion


        #region Constructor
        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="unitOfWork">This <see cref="IUnitOfWork"/> concrete object.</param>
        public EmployeeWorkPlanService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        #endregion

        #region Public Methods
        public IEnumerable<GeneralCategory> GetMasterPlan()
        {
            var result =  GeneralCategoryReadOnlyRepository.Fetch()
                .Where(x=>x.Category=="WFO-Plan")
                .AsNoTracking().ToList();
            return result;
        }

        /// <summary>
        /// Get list of permissions by role id
        /// </summary>
        /// <param name="id">Role Id</param>
        /// <returns>List of User Role Object</returns>
        public IEnumerable<WorkPlanStoredEntity> GetPlans(string Id)
        {
            return UnitOfWork.UdfQuery<WorkPlanStoredEntity>(new { objectId = Id });
        }

        /// <summary>
        /// Get list of permissions by role id
        /// </summary>
        /// <param name="id">Role Id</param>
        /// <returns>List of User Role Object</returns>
        public IEnumerable<WorkDivisionPlanStoredEntity> GetDivisions(Guid id)
        {
            var result = UnitOfWork.UdfQuery<WorkDivisionPlanStoredEntity>(new { planId = id }).OrderBy(a => a.ObjectText);
            return result;
        }

        /// <summary>
        /// Get role by id
        /// </summary>
        /// <param name="id">Role Id</param>
        /// <returns>Role Object</returns>
        public GeneralCategory GetMasterPlanById(Guid id)
        {
            return GeneralCategoryReadOnlyRepository.Fetch()
               .AsNoTracking()
               .Where(x => x.Id == id)
               .FirstOrDefaultIfEmpty();
        }

        /// <summary>
        /// Update or insert role
        /// </summary>
        /// <param name="viewModel">Role View Model</param>
        public void UpsertPlan(WorkDivisionPlanViewModel viewModel)
        {
            var plan = GeneralCategoryReadOnlyRepository.FindById(viewModel.Id);

            //UnitOfWork.Transact(() =>
            //{
                if (plan != null)
                {

                    WorkDivisionPlanRepository.Fetch().Where(x => x.PlanId == plan.Id).Delete();
                }

                if (viewModel.Division != null)
                {
                    foreach (var division in viewModel.Division)
                    {
                        WorkDivisionPlanRepository.Add(new WorkDivisionPlan { PlanId = plan.Id, OrgCode = division });
                    }
                }

                UnitOfWork.SaveChanges();

            //}, System.Data.IsolationLevel.ReadUncommitted);
        }

        #endregion

    }
}
