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
    
    public class EquipmentService : DomainServiceBase
    {
        #region Domain Repository
        protected IRepository<EquipmentView> EquipmentViewRepository => UnitOfWork.GetRepository<EquipmentView>();
        protected IReadonlyRepository<EquipmentView> EquipmentViewReadonlyRepository => UnitOfWork.GetRepository<EquipmentView>();
        protected IRepository<Equipment> EquipmentRepository => UnitOfWork.GetRepository<Equipment>();
        protected IReadonlyRepository<Equipment> EquipmentReadonlyRepository => UnitOfWork.GetRepository<Equipment>();
        
        #endregion
        #region Variables & Properties
        /// <summary>
        /// Field that hold properties that can be updated for bank entity.
        /// </summary>
        private readonly string[] Properties = new[] {
            //"Id",
            "DivisiCode",
            "AreaId",
            "EquipmentName",
            "Quantity"

        };
        #endregion

        #region Constructor
        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="unitOfWork">This <see cref="IUnitOfWork"/> concrete object.</param>
        public EquipmentService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        #endregion
        public IQueryable<EquipmentView> GetQuery()
        {
            return EquipmentViewReadonlyRepository.Fetch()
                .AsNoTracking();
        }

        public IEnumerable<EquipmentView> Gets()
        {
            return GetQuery().ToList();
        }
        public EquipmentView Get(Guid id)
        {
            return EquipmentViewReadonlyRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.Id == id)
                .FirstOrDefaultIfEmpty();
        }

        public IEnumerable<EquipmentView> getkategoriPenyakit()
        {
            return EquipmentViewRepository.Fetch()
                .AsNoTracking().ToList();
        }
        public IQueryable<EquipmentView> GetPopUpEquipment(string divisionCode = null, string areaId = null)
        {
            var query = EquipmentViewReadonlyRepository.Fetch()
                .AsNoTracking();

            // Jika parameter tidak null atau tidak kosong, lakukan filter
            if (!string.IsNullOrEmpty(divisionCode))
            {
                Console.WriteLine($"Division: {divisionCode}");
                query = query.Where(e => e.DivisiCode == divisionCode);
            }

            if (!string.IsNullOrEmpty(areaId))
            {
                Console.WriteLine($"areaId: {areaId}");
                query = query.Where(e => e.AreaId == areaId);
            }

            // Filter untuk mengecualikan APAR dan Hydrant
            query = query.Where(e => e.EquipmentName != "APAR" && e.EquipmentName != "Hydrant");

            return query;
        }


        public EquipmentView GetPopUpEquipment(Guid id)
        {
            return GetPopUpEquipment().Where(x => x.Id == id)
                .FirstOrDefaultIfEmpty();
        }

        public IQueryable<ActualEntityStructure> GetDivisions()
        {
            var set = UnitOfWork.GetRepository<ActualEntityStructure>();
            var div = set.Fetch()
                .AsNoTracking()
                .Where(x => x.ObjectDescription == "Division");
            return div.Select(x => new ActualEntityStructure { ObjectCode = x.ObjectCode, ObjectText = x.ObjectText }).Distinct().OrderBy(x => x.ObjectText);
        }

        public void Upsert(Equipment param)
        {
            param.AreaId = param.AreaIdEquip;
            param.DivisiCode = param.DivisiCodeEquip;
            EquipmentRepository.Upsert<Guid>(param, Properties);

            UnitOfWork.SaveChanges();
        }

        public void Delete(Guid id)
        {
            EquipmentRepository.DeleteById(id);

            UnitOfWork.SaveChanges();
        }
    }
}
