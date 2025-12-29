using TAMHR.ESS.Domain;
using Agit.Domain.UnitOfWork;
using Agit.Domain;
using Agit.Domain.Repository;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System;
using Dapper;
using System.Collections.Generic;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    /// <summary>
    /// Service class that handle VaccineHospital master data.
    /// </summary>
    public class VaccineHospitalService : GenericDomainServiceBase<VaccineHospital>
    {
        #region Variables & Properties
        /// <summary>
        /// Field that hold properties that can be updated for VaccineHospital entity.
        /// </summary>
        protected override string[] Properties => new[] {
            "Name",
            "Province",
            "City",
            "Address"
        };
        #endregion

        #region Constructor
        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="unitOfWork">This <see cref="IUnitOfWork"/> concrete object.</param>
        public VaccineHospitalService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public VaccineHospital GetHospitalByName(string hospitalName)
        {
            var set = UnitOfWork.GetConnection().Query<VaccineHospital>("SELECT vhos.* FROM TB_M_VACCINE_HOSPITAL vhos " +
                " WHERE vhos.name=@hospitalName", new { hospitalName }).FirstOrDefault();
            return set;
        }

        public VaccineHospital GetScheduleByHospitalDate(string hospitalName,DateTime hospitalDate)
        {
            var set = UnitOfWork.GetConnection().Query<VaccineHospital>("SELECT vhos.* FROM TB_M_VACCINE_HOSPITAL vhos " +
                " INNER JOIN TB_M_VACCINE_SCHEDULE_LIMIT vsl ON vsl.VaccineHospitalId=vhos.Id WHERE vhos.name=@hospitalName AND vsl.VaccineDate=@hospitalDate", new { hospitalName,hospitalDate }).FirstOrDefault();

            return set;
        }
        #endregion
    }

    public class VaccineHospitalRepoService : DomainServiceBase
    {
       
        #region Constructor
        /// <summary>
        /// Constructore
        /// </summary>
        /// <param name="unitOfWork">Concrete UnitOfWork</param>
        public VaccineHospitalRepoService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        #endregion

        #region Public Methods

        public IQueryable<Hospital> GetHospital()
        {
            var set = UnitOfWork.GetRepository<Hospital>();

            return set.Fetch()
                .AsNoTracking();
        }

        
        #endregion
    }
}
