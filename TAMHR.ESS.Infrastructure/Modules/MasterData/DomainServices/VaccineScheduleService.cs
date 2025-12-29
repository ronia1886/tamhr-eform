using TAMHR.ESS.Domain;
using Agit.Domain.UnitOfWork;
using System.Linq;
using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Dapper;
using System.Data;
using Agit.Domain.Extensions;
using Agit.Domain.Repository;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    /// <summary>
    /// Service class that handle vaccine schedule master data.
    /// </summary>
    public class VaccineScheduleService : GenericDomainServiceBase<VaccineSchedule>
    {
        #region Repository
        protected IRepository<VaccineScheduleLimit> vaccineScheduleLimitRepository => UnitOfWork.GetRepository<VaccineScheduleLimit>();
        #endregion
        #region Variables & Properties
        /// <summary>
        /// Field that hold properties that can be updated for vaccine schedule entity.
        /// </summary>
        protected override string[] Properties => new[] {
            "StartDateTime",
            "EndDateTime",
            "VaccineNumber",
        };

        protected string[] DetailProperties => new[] {
            "VaccineScheduleId",
            "VaccineHospitalId",
            "VaccineDate",
            "Qty",
        };
        #endregion

        #region Constructor
        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="unitOfWork">This <see cref="IUnitOfWork"/> concrete object.</param>
        public VaccineScheduleService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        #endregion

        public IQueryable<VaccineSchedule> GetVaccineSchedule(Guid Id)
        {
            var set = UnitOfWork.GetRepository<VaccineSchedule>();

            return set.Fetch()
                 .AsNoTracking()
                 .Where(x => x.Id == Id);
        }

        public IQueryable<VaccineHospital> GetVaccineHospitals()
        {
            var set = UnitOfWork.GetRepository<VaccineHospital>();

            return set.Fetch()
                 .AsNoTracking()
                 .Where(x => x.RowStatus == true);
        }

        public IEnumerable<VaccineScheduleLimit> GetVaccineScheduleLimits()
        {
            var set = UnitOfWork.GetRepository<VaccineScheduleLimit>().Fetch().Where(x => x.VaccineDate >= DateTime.Now.AddDays(-1)).AsNoTracking().ToList();
            var qty = GetScheduleLimit();
            //var query = from ls in set
            //            join qt in qty
            //            on new { ls.VaccineHospitalId, ls.VaccineDate } equals
            //               new { qt.VaccineHospitalId, qt.VaccineDate } into j1
            //            from j2 in j1
            //            select new VaccineScheduleLimit() {VaccineDate =ls.VaccineDate,Qty = j2.Qty};

            var qs = from t1 in qty
                     from t2 in set.Where(x => t1.VaccineHospitalId == x.VaccineHospitalId && t1.VaccineDate == x.VaccineDate)
                            .DefaultIfEmpty()
                     select new VaccineScheduleLimit() { VaccineDate = t1.VaccineDate,Qty = t1.Qty, VaccineHospitalId = t1.VaccineHospitalId };
            var ak = qs.ToList();
            return set;
        }

        public IEnumerable<VaccineScheduleLimit> GetVaccineScheduleLimitBackdates()
        {
            var set = UnitOfWork.GetRepository<VaccineScheduleLimit>().Fetch().AsNoTracking().ToList();
            var qty = GetScheduleLimit();
            var qs = from t1 in qty
                     from t2 in set.Where(x => t1.VaccineHospitalId == x.VaccineHospitalId && t1.VaccineDate == x.VaccineDate)
                            .DefaultIfEmpty()
                     select new VaccineScheduleLimit() { VaccineDate = t1.VaccineDate, Qty = t1.Qty, VaccineHospitalId = t1.VaccineHospitalId };
            var ak = qs.ToList();
            return set;
        }

        public List<VaccineScheduleLimit> GetScheduleLimit()
        {
            List<VaccineScheduleLimit> data = UnitOfWork.GetConnection().Query<VaccineScheduleLimit>(@"
            SELECT 
	        vsl.VaccineScheduleId
	        ,vsl.VaccineHospitalId
	        ,vsl.VaccineDate
	        ,vsl.Qty-ISNULL(SUM(t1.total),0) as Qty
	        FROM TB_M_VACCINE_SCHEDULE_LIMIT vsl
	        LEFT JOIN TB_M_VACCINE_HOSPITAL vh ON vsl.VaccineHospitalId=vh.Id
	        LEFT JOIN 
	        (SELECT COUNT(*) total,VaccineDate1,VaccineHospital1 FROM TB_R_VACCINE WHERE VaccineDate1 IS NOT NULL GROUP BY VaccineDate1,VaccineHospital1
	         UNION ALL 
	         SELECT COUNT(*) total,VaccineDate2,VaccineHospital2 FROM TB_R_VACCINE WHERE VaccineDate2 IS NOT NULL GROUP BY VaccineDate2,VaccineHospital2
	         ) t1 ON vsl.VaccineDate=t1.VaccineDate1 AND vh.Name=t1.VaccineHospital1
            WHERE vsl.VaccineDate >= DATEADD(day, -1, GETDATE())
	         GROUP BY 
	         vsl.VaccineScheduleId
	        ,vsl.VaccineHospitalId
	        ,vsl.VaccineDate
	        ,vsl.Qty
            ").ToList();
            return data;
        }

        public IQueryable<VaccineScheduleLimitStoredEntity> GetVaccineScheduleLimit(Guid vaccineScheduleId)
        {
            var set = UnitOfWork.GetRepository<VaccineScheduleLimitStoredEntity>();

           return set.Fetch()
                .AsNoTracking()
                .Where(x => x.VaccineScheduleId == vaccineScheduleId);
        }

        public VaccineScheduleLimit GetVaccineScheduleLimitById(Guid id)
        {
            var set = UnitOfWork.GetRepository<VaccineScheduleLimit>();
            return set.Fetch()
                .AsNoTracking()
                .Where(x => x.Id == id && x.RowStatus).FirstOrDefault();
        }

        public bool UpsertDetail(VaccineScheduleLimit data)
        {
            // Push pending changes into database and return the boolean result.
            var set = UnitOfWork.GetRepository<VaccineScheduleLimit>();
            set.Upsert<Guid>(data, DetailProperties);

            return UnitOfWork.SaveChanges() > 0;
        }

        public bool DeleteDetailById(Guid Id)
        {
            // Push pending changes into database and return the boolean result.
            var set = UnitOfWork.GetRepository<VaccineScheduleLimit>();
            set.DeleteById(Id);

            return UnitOfWork.SaveChanges() > 0;
        }

        public VaccineScheduleLimit GetScheduleLimit(DateTime VaccineDate, Guid VaccineHospitalId)
        {
            VaccineScheduleLimit data = UnitOfWork.GetConnection().Query<VaccineScheduleLimit>(@"
            SELECT 
	        vsl.VaccineScheduleId
	        ,vsl.VaccineHospitalId
	        ,vsl.VaccineDate
	        ,vsl.Qty-ISNULL(SUM(t1.total),0) as Qty
	        FROM TB_M_VACCINE_SCHEDULE_LIMIT vsl
	        LEFT JOIN TB_M_VACCINE_HOSPITAL vh ON vsl.VaccineHospitalId=vh.Id
	        LEFT JOIN 
	        (SELECT COUNT(*) total,VaccineDate1,VaccineHospital1 FROM TB_R_VACCINE WHERE VaccineDate1 IS NOT NULL GROUP BY VaccineDate1,VaccineHospital1
	         UNION ALL 
	         SELECT COUNT(*) total,VaccineDate2,VaccineHospital2 FROM TB_R_VACCINE WHERE VaccineDate2 IS NOT NULL GROUP BY VaccineDate2,VaccineHospital2
	         ) t1 ON vsl.VaccineDate=t1.VaccineDate1 AND vh.Name=t1.VaccineHospital1
            WHERE vsl.VaccineDate = @VaccineDate AND vsl.VaccineHospitalId = @VaccineHospitalId
	         GROUP BY 
	         vsl.VaccineScheduleId
	        ,vsl.VaccineHospitalId
	        ,vsl.VaccineDate
	        ,vsl.Qty
            ", new { VaccineDate, VaccineHospitalId }).FirstOrDefault();
            return data;
        }

        public List<VaccineHospital> GetAvailableVaccine(DateTime VaccineDate)
        {
            string strVaccineDate = VaccineDate.ToString("yyyy-MM-dd");
            var data = UnitOfWork.GetConnection().Query<VaccineHospital>(@"
            SELECT Id,Name FROM (
            SELECT 
	            vh.Id
                ,vh.Name
	            ,vsl.Qty-ISNULL(SUM(t1.total),0) as Qty
	            FROM TB_M_VACCINE_SCHEDULE_LIMIT vsl
	            INNER JOIN TB_M_VACCINE_HOSPITAL vh ON vsl.VaccineHospitalId=vh.Id
	            LEFT JOIN 
	            (SELECT COUNT(*) total,VaccineDate1,VaccineHospital1 FROM TB_R_VACCINE WHERE VaccineDate1 IS NOT NULL GROUP BY VaccineDate1,VaccineHospital1
	                UNION ALL 
	                SELECT COUNT(*) total,VaccineDate2,VaccineHospital2 FROM TB_R_VACCINE WHERE VaccineDate2 IS NOT NULL GROUP BY VaccineDate2,VaccineHospital2
	                ) t1 ON vsl.VaccineDate=t1.VaccineDate1 AND vh.Name=t1.VaccineHospital1
                WHERE 1=1 AND vsl.VaccineDate = @vaccineDate
	                GROUP BY 
	            vh.Id
                ,vh.Name
	            ,vsl.Qty
            ) t1
            GROUP BY t1.Id,t1.Name
            ", new { vaccineDate = strVaccineDate }).ToList();
            return data;
        }

        public int GetLastBatch()
        {
            return UnitOfWork.GetConnection().Query<int>("SELECT ISNULL(MAX(VaccineNumber),0) as lastBatch FROM TB_M_VACCINE_SCHEDULE").FirstOrDefault();
        }

        public DateTime GetLastEndDate(int? VaccineNumber)
        {
            return UnitOfWork.GetConnection().Query<DateTime>("SELECT TOP 1 ISNULL(EndDateTime,GETDATE()) as EndDateTime FROM TB_M_VACCINE_SCHEDULE WHERE VaccineNumber<@vaccineNumber " +
                " ORDER BY VaccineNumber DESC",new { vaccineNumber=VaccineNumber}).FirstOrDefault();
        }

        public List<VaccineScheduleLimit> GetVaccineScheduleLimitByDateHospital(DateTime VaccineDate, Guid VaccineHospitalId)
        {
            var set = UnitOfWork.GetRepository<VaccineScheduleLimit>();

            return set.Fetch()
                 .AsNoTracking()
                 .Where(x => x.VaccineDate == VaccineDate && x.VaccineHospitalId == VaccineHospitalId).ToList();
        }

        public void UploadVaccineSchedule(string actor, string postCode, DataTable dt)
        {
            UnitOfWork.Transact(trans =>
            {
                UnitOfWork.UspQuery("SP_UPLOAD_VACCINE_SCHEDULE", new { actor, postCode, data = dt.AsTableValuedParameter("TVP_VACCINE_SCHEDULE") }, trans);

                UnitOfWork.SaveChanges();
            });
        }

        public List<VaccineScheduleDownloadView> DownloadDataVaccineSchedule()
        {
            var set = UnitOfWork.GetRepository<VaccineScheduleDownloadView>();
            return set.Fetch().AsNoTracking()
                .OrderBy(ob => ob.VaccineNumber).ThenBy(ob => ob.StartDateTime).ThenBy(ob => ob.EndDateTime).ThenBy(ob => ob.VaccineDate).ThenBy(ob => ob.Hospital)
                .ToList();
        }

        public bool CheckBatchAndDate(int batch,DateTime startDate, DateTime endDate)
        {
            bool result = true;
            var data = UnitOfWork.GetConnection().Query<VaccineSchedule>("SELECT * FROM TB_M_VACCINE_SCHEDULE "+
                "WHERE VaccineNumber=@batch AND StartDateTime=@startDate AND EndDateTime=@endDate", new { batch, startDate, endDate }).FirstOrDefault();
            if (data != null)
            {
                result = false;
            }

            return result;
        }

        public VaccineScheduleLimit GetVaccineScheduleByDateHospitalName(DateTime? VaccineDate, string VaccineHospitalName)
        {
            try
            {
                var vaccineHospitalId = UnitOfWork.GetRepository<VaccineHospital>().Fetch().Where(wh => wh.Name == VaccineHospitalName).FirstOrDefault().Id;
                var set = UnitOfWork.GetRepository<VaccineScheduleLimit>();

                return set.Fetch()
                     .AsNoTracking()
                     .Where(x => x.VaccineDate == VaccineDate && x.VaccineHospitalId == vaccineHospitalId).FirstOrDefault();
            }
            catch
            {
                return null;
            }
            
        }

        public List<VaccineHospital> GetVaccineScheduleByDate(DateTime? VaccineDate)
        {
            try
            {
                var set = UnitOfWork.GetRepository<VaccineHospital>();
                var set2 = vaccineScheduleLimitRepository.Fetch().Where(x => x.VaccineDate == VaccineDate).ToList();
                return set.Fetch()
                     .Join(vaccineScheduleLimitRepository.Fetch().Where(x => x.VaccineDate==VaccineDate), vh => vh.Id, vhl => vhl.VaccineHospitalId, (vh, vhl) => vh)
                     .ToList();
            }
            catch
            {
                return null;
            }

        }
    }
}
