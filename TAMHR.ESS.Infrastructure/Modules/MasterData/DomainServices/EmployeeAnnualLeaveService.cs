using System.Linq;
using Microsoft.EntityFrameworkCore;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Domain.Models.TimeManagement;          // ← penting: DTO
using Agit.Common.Extensions;
using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;
using System;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    /// <summary>
    /// Service class that handle employee leave master data.
    /// </summary>
    public class EmployeeAnnualLeaveService : GenericDomainServiceBase<EmployeeAnnualLeave>
    {
        #region Domain Repositories
        protected IRepository<User> UserRepository => UnitOfWork.GetRepository<User>();
        #endregion

        #region Variables & Properties
        protected override string[] Properties => new[] { "Period", "AnnualLeave" };
        #endregion

        public EmployeeAnnualLeaveService(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        #region Public Methods

        /// <summary>
        /// Query untuk list/grid: sepenuhnya bisa ditranslate EF (tanpa method custom).
        /// </summary>
        public override IQueryable<EmployeeAnnualLeave> GetQuery()
        {
            var baseQ =
                from e in CommonRepository.Fetch().AsNoTracking()
                join u in UserRepository.Fetch().AsNoTracking()
                    on e.NoReg equals u.NoReg
                select new EmployeeAnnualLeave
                {
                    Id = e.Id,
                    NoReg = e.NoReg,
                    Period = e.Period,
                    AnnualLeave = e.AnnualLeave,
                    Name = u.Name      // pastikan properti Name ada (boleh [NotMapped])
                };

            // >>> pipeline setelah ini (GroupBy/First) akan berjalan di memory
            return baseQ.AsEnumerable().AsQueryable();
        }




        public EmployeeAnnualLeave GetByNoreg(string noreg, int period)
        {
            return CommonRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.NoReg == noreg && x.Period == period)
                .FirstOrDefaultIfEmpty();
        }

        public IQueryable<EmployeeAnnualLeave> GetByNoreg(string noreg)
        {
            return CommonRepository.Fetch()
               .AsNoTracking()
               .Where(x => x.NoReg == noreg);
        }

        public void Delete(Guid id)
        {
            CommonRepository.DeleteById(id);
            UnitOfWork.SaveChanges();
        }
        #endregion
    }
}
