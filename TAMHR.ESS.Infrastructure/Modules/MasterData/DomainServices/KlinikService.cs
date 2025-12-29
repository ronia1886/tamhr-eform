using TAMHR.ESS.Domain;
using Agit.Domain.UnitOfWork;
using Agit.Domain;
using Agit.Domain.Repository;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;
using Agit.Common.Extensions;

namespace TAMHR.ESS.Infrastructure.DomainServices
{

    public class KlinikService : DomainServiceBase
    {
        #region Domain Repository
        protected IRepository<KlinikView> KlinikViewRepository => UnitOfWork.GetRepository<KlinikView>();
        protected IReadonlyRepository<KlinikView> KlinikViewReadonlyRepository => UnitOfWork.GetRepository<KlinikView>();
        protected IRepository<KlinikTB> KlinikTBRepository => UnitOfWork.GetRepository<KlinikTB>();
        protected IReadonlyRepository<KlinikTB> KlinikReadonlyRepository => UnitOfWork.GetRepository<KlinikTB>();

        #endregion
        #region Variables & Properties
        /// <summary>
        /// Field that hold properties that can be updated for bank entity.
        /// </summary>
        private readonly string[] Properties = new[] {
            //"Id",
            "AreaId",
            "Klinik",
            "CategoryCode",
            "FromHours",
            "ToHours",
            "PIC"

        };
        #endregion

        #region Constructor
        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="unitOfWork">This <see cref="IUnitOfWork"/> concrete object.</param>
        public KlinikService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        #endregion
        public IQueryable<KlinikView> GetQuery()
        {
            return KlinikViewReadonlyRepository.Fetch()
                .AsNoTracking();
        }

        public IEnumerable<KlinikView> Gets()
        {
            return GetQuery().ToList();
        }
        public KlinikView Get(Guid id)
        {
            return KlinikViewReadonlyRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.Id == id)
                .FirstOrDefaultIfEmpty();
        }

        public IEnumerable<KlinikView> getkategoriPenyakit()
        {
            return KlinikViewRepository.Fetch()
                .AsNoTracking().ToList();
        }
        public IQueryable<KlinikView> GetPopUpKlinik()
        {
            return KlinikViewReadonlyRepository.Fetch()
                .AsNoTracking();
        }
        public KlinikView GetPopUpKlinik(Guid id)
        {
            return GetPopUpKlinik().Where(x => x.Id == id)
                .FirstOrDefaultIfEmpty();
        }
        public IQueryable<GeneralCategory> GetcategoryName()
        {
            var set = UnitOfWork.GetRepository<GeneralCategory>();
            var div = set.Fetch()
                .AsNoTracking()
                .Where(x => x.Category == "TanyaOHS");
            return div.Select(x => new GeneralCategory { Name = x.Name, Code = x.Code }).Distinct().OrderBy(x => x.Name);
        }

        public IQueryable<EmployeProfileView> GetUserNameKilink()
        {
            var set = UnitOfWork.GetRepository<EmployeProfileView>();
            var div = set.Fetch()
                .AsNoTracking()
                .Where(x => x.Name != null);
            return div.Select(x => new EmployeProfileView { Name = x.Name, Noreg = x.Noreg }).Distinct().OrderBy(x => x.Name);
        }

        public void Upsert(KlinikTB param)
        {
            param.AreaId = param.AreaIdKlinik;

            KlinikTBRepository.Upsert<Guid>(param, Properties);

            UnitOfWork.SaveChanges();
        }

        public void Delete(Guid id)
        {
            KlinikTBRepository.DeleteById(id);

            UnitOfWork.SaveChanges();
        }
    }
}
