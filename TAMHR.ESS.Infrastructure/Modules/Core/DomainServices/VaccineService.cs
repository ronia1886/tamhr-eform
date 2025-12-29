using TAMHR.ESS.Domain;
using Agit.Domain;
using Agit.Domain.UnitOfWork;
using Agit.Domain.Repository;
using System.Linq;
using System;
using System.Text;
using Agit.Domain.Extensions;
using Dapper;
using TAMHR.ESS.Infrastructure.ViewModels;
using Z.EntityFramework.Plus;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using Microsoft.EntityFrameworkCore;
using Agit.Common;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    /// <summary>
    /// Service class that handle vaccine master data.
    /// </summary>
    //public class VaccineService : GenericDomainServiceBase<Vaccine>
    public class VaccineService : DomainServiceBase
    {
        protected IRepository<Vaccine> VaccineRepository => UnitOfWork.GetRepository<Vaccine>();
        protected IRepository<VaccineQuestion> VaccineQuestionRepository => UnitOfWork.GetRepository<VaccineQuestion>();
        protected IRepository<VaccineQuestionDetail> VaccineQuestionDetailRepository => UnitOfWork.GetRepository<VaccineQuestionDetail>();
        protected IRepository<VaccineSummaryStoredEntity> VaccineSummaryStoredEntityRepository => UnitOfWork.GetRepository<VaccineSummaryStoredEntity>();
        protected IRepository<Config> ConfigRepository => UnitOfWork.GetRepository<Config>();
        protected IRepository<PersonalDataAttributeStroredEntity> PersonalDataAttributeStroredEntityRepository => UnitOfWork.GetRepository<PersonalDataAttributeStroredEntity>();  
        protected IReadonlyRepository<VaccinReportEmployeeView> ReportEmployeeRepository => UnitOfWork.GetRepository<VaccinReportEmployeeView>();

        #region Private Properties
        /// <summary>
        /// Field that hold properties that can be updated for news entity
        /// </summary>
        private readonly string[] _vaccineProperties = new[] {"NoReg", "SubmissionDate", "FamilyStatus", "Name", "BirthDate", 
            "PhoneNumber", "Domicile","Address","IdentityId","IdentityImage",
            "Allergies","LastNegativeSwabDate","IsPregnant","OtherQuestion","OtherVaccine",
            "VaccineAgreement", "VaccineDate1","VaccineHospital1","VaccineCard1","VaccineType1", 
            "VaccineDate2","VaccineHospital2","VaccineCard2","VaccineType2","SHAStatus",
            "IsAllergies","IsPositive","Status","IsSideEffects1","SideEffects1","IsSideEffects2","SideEffects2"};

        private readonly string[] _vaccineQuestionProperties = new[] {"VaccineId", "FormQuestionId", "Answer" };
        private readonly string[] _vaccineQuestionDetailProperties = new[] { "VaccineQuestionId", "Answer" };

        #endregion

        #region Constructor
        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="unitOfWork">This <see cref="IUnitOfWork"/> concrete object.</param>
        public VaccineService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        #endregion

        public List<Vaccine> GetVaccineQuery(string NoReg, string Name) {
            return UnitOfWork.GetConnection().Query<Vaccine>(@"
            SELECT vac.Id,vw.NoReg,Nama_Pegawai as Name,OrderRank,'Employee' as FamilyStatus,
            vac.SubmissionDate,Tanggal_Lahir as BirthDate,ISNULL(vac.PhoneNumber,vw.PhoneNumber) as PhoneNumber,Isnull(vac.IdentityId,No_KTP) IdentityId,vac.Domicile, ISNULL(vac.Address,Alamat) as Address,
            vac.IdentityImage,vac.Allergies,vac.LastNegativeSwabDate,vac.IsPregnant,
            vac.OtherQuestion, vac.OtherVaccine, vac.VaccineAgreement,vac.VaccineDate1,vac.VaccineHospital1,
            vac.VaccineCard1,vac.VaccineDate2,vac.VaccineHospital2,vac.VaccineCard2,Jenis_Kelamin as Gender,
            vw.Kota as City, vw.Kecamatan as District, vw.Kelurahan as SubDistrict, vac.IsAllergies, vac.IsPositive,vac.IsOtherVaccine
            ,vac.HaveVaccine,vac.SHAStatus,gc.Name as Status,vac.IsSideEffects1,vac.SideEffects1,vac.IsSideEffects2,vac.SideEffects2,vw.Email
            ,vac.TAMVaccineAgreement
	            FROM VW_PERSONAL_DATA_INFORMATION vw
	            LEFT JOIN TB_R_VACCINE vac ON vac.NoReg = vw.NoReg AND vac.Name=vw.Nama_Pegawai
                LEFT JOIN TB_M_GENERAL_CATEGORY gc ON (CASE WHEN vac.SHAStatus=0 THEN '9' ELSE ISNULL(vac.Status,'1') END)=gc.Code
	            WHERE vw.noreg=@noreg AND Nama_Pegawai like ISNULL(@name,'%%')
	            UNION ALL
            SELECT vac.Id,vw.NoReg,vw.Name,ROW_NUMBER() OVER ( Order By vw.BirthDate)+1 as OrderRank,
	        (CASE WHEN FamilyType='Anak Kandung' THEN CONCAT('Anak ',ROW_NUMBER() OVER ( Order By vw.BirthDate)-1) ELSE FamilyType END) as FamilyStatus,
	        vac.SubmissionDate, vw.BirthDate,vac.PhoneNumber,ISNULL(vac.IdentityId,Nik) as IdentityId, vac.Domicile, vac.Address,
	        vac.IdentityImage,vac.Allergies,vac.LastNegativeSwabDate,vac.IsPregnant,
            vac.OtherQuestion, vac.OtherVaccine, vac.VaccineAgreement,vac.VaccineDate1,vac.VaccineHospital1,
            vac.VaccineCard1,vac.VaccineDate2,vac.VaccineHospital2,vac.VaccineCard2,vw.Gender,
            vac.City as City, vac.District, vac.SubDistrict, vac.IsAllergies, vac.IsPositive,vac.IsOtherVaccine
            ,vac.HaveVaccine,vac.SHAStatus,gc.Name as Status,vac.IsSideEffects1,vac.SideEffects1,vac.IsSideEffects2,vac.SideEffects2,null as Email
            ,vac.TAMVaccineAgreement
                FROM vw_personal_data_family_member_complete vw
	            LEFT JOIN TB_R_VACCINE vac ON vac.NoReg = vw.NoReg AND vac.Name=vw.Name
                LEFT JOIN TB_M_GENERAL_CATEGORY gc ON (CASE WHEN vac.SHAStatus=0 THEN '9' ELSE ISNULL(vac.Status,'1') END)=gc.Code
	            WHERE vw.FamilyType IN ('Pasangan','Anak Kandung','Anak Angkat') AND vw.noreg=@noreg AND vw.Name like ISNULL(@name,'%%')
            ", new { noreg = NoReg, name = Name }).ToList();
        }
        public Vaccine GetVaccine(string NoReg, string Name)
        {
                Vaccine newData = new Vaccine();
            Vaccine data = GetVaccineQuery(NoReg, Name).FirstOrDefault();
            if (data != null)
            {
                Guid vaccineId = data.Id;
                //List<VaccineQuestion> listVQData = VaccineQuestionRepository.Fetch()
                //     .Where(x => x.VaccineId == vaccineId)
                //     .ToList();
                var listVQData = GetVaccineQuestions(vaccineId);
                data.VaccineQuestionList = listVQData;
                if (listVQData.Count > 0)
                {
                    for (int i = 0; i < listVQData.Count; i++)
                    {
                        List<VaccineQuestionDetail> listVQDataDetail = VaccineQuestionDetailRepository.Fetch()
                         .Where(x => x.VaccineQuestionId == listVQData[i].Id)
                         .ToList();
                        listVQData[i].VaccineQuestionDetailList = listVQDataDetail;

                    }
                }
            }
            
            return data;
        }
        public Vaccine GetVaccineById(Guid Id)
        {
            Vaccine data = UnitOfWork.GetConnection().Query<Vaccine>(@"
            SELECT * FROM TB_R_VACCINE WHERE Id=@Id", new { Id }).FirstOrDefault();
            if (data != null)
            {
                Guid vaccineId = data.Id;
                var listVQData = GetVaccineQuestions(vaccineId);
                data.VaccineQuestionList = listVQData;
                if (listVQData.Count > 0)
                {
                    for (int i = 0; i < listVQData.Count; i++)
                    {
                        List<VaccineQuestionDetail> listVQDataDetail = VaccineQuestionDetailRepository.Fetch()
                         .Where(x => x.VaccineQuestionId == listVQData[i].Id)
                         .ToList();
                        listVQData[i].VaccineQuestionDetailList = listVQDataDetail;

                    }
                }
            }

            return data;
        }
        public List<VaccineQuestion> GetVaccineQuestions(Guid vaccineId)
        {
            return VaccineQuestionRepository.Fetch()
                     .Where(x => x.VaccineId == vaccineId)
                     .ToList();
        }

        public IQueryable<VaccineQuestion> GetVaccineQuestionsAll()
        {
            return VaccineQuestionRepository.Fetch().AsNoTracking();
        }

        public List<VaccineAnswerView> GetVaccineAnswer(Guid vaccineId,Guid questionId)
        {
            return  UnitOfWork.GetRepository<VaccineAnswerView>().Fetch()
                     .Where(x => x.VaccineId == vaccineId && x.Id == questionId ).AsNoTracking()
                     .ToList();
        }

        public IQueryable<VaccineAnswerView> GetVaccineAnswerAll()
        {
            return UnitOfWork.GetRepository<VaccineAnswerView>().Fetch().AsNoTracking();
        }



        public List<VaccineQuestionDetail> GetVaccineQuestionDetails(Guid vaccineQuestionId)
        {
            return VaccineQuestionDetailRepository.Fetch()
                     .Where(x => x.VaccineQuestionId == vaccineQuestionId)
                     .ToList();
        }

        private static int CalculateAge(DateTime dateOfBirth)
        {
            int age = 0;
            age = DateTime.Now.Year - dateOfBirth.Year;
            if (DateTime.Now.DayOfYear < dateOfBirth.DayOfYear)
                age = age - 1;

            return age;
        }

        public bool UpsertVaccine(Vaccine vaccine)
        {
            vaccine.CreatedOn = DateTime.Now;
            vaccine.ModifiedOn = DateTime.Now;
            vaccine.RowStatus = true;

            if (vaccine.SHAStatus)
            {
                int currentAge = vaccine.BirthDate.Month > DateTime.Now.Month ? vaccine.BirthDate.Year + 1 : vaccine.BirthDate.Year;
                int Age = DateTime.Now.Year - currentAge;
                int newAge = CalculateAge(vaccine.BirthDate);
                

                var dataConfig = ConfigRepository.Find(x => x.ConfigKey == "Vaccine.MinAge").FirstOrDefault();
                int minAge = 0;
                if (dataConfig != null)
                {
                    minAge = Convert.ToInt32(dataConfig.ConfigValue);
                }
                if (newAge >= minAge)
                {
                    vaccine.SHAStatus = true;
                }
                else
                {
                    vaccine.SHAStatus = false;
                }
            }
            

            if (vaccine.HaveVaccine == true)
            {
                vaccine.VaccineType1 = vaccine.VaccineType2 = "external";
            }
            else
            {
                vaccine.VaccineType1 = vaccine.VaccineType2 = "internal";
            }

            UnitOfWork.Transact((transaction) =>
            {
                VaccineUpsertStoredEntity data = (VaccineUpsertStoredEntity)UnitOfWork.UspQuery<VaccineUpsertStoredEntity>(new
                {
                    vaccine.Id,
                    vaccine.NoReg,
                    vaccine.SubmissionDate,
                    vaccine.FamilyStatus,
                    vaccine.Name,
                    vaccine.BirthDate,
                    vaccine.PhoneNumber,
                    vaccine.Domicile,
                    vaccine.Address,
                    vaccine.IdentityId,
                    vaccine.IdentityImage,
                    vaccine.CreatedBy,
                    vaccine.CreatedOn,
                    vaccine.ModifiedBy,
                    vaccine.ModifiedOn,
                    vaccine.RowStatus,
                    vaccine.Allergies,
                    vaccine.LastNegativeSwabDate,
                    vaccine.IsPregnant,
                    vaccine.OtherQuestion,
                    vaccine.OtherVaccine,
                    vaccine.VaccineAgreement,
                    vaccine.HaveVaccine,
                    vaccine.VaccineDate1,
                    vaccine.VaccineHospital1,
                    vaccine.VaccineCard1,
                    vaccine.VaccineType1,
                    vaccine.IsSideEffects1,
                    vaccine.SideEffects1,
                    vaccine.VaccineDate2,
                    vaccine.VaccineHospital2,
                    vaccine.VaccineCard2,
                    vaccine.VaccineType2,
                    vaccine.IsSideEffects2,
                    vaccine.SideEffects2,
                    vaccine.Gender,
                    vaccine.City,
                    vaccine.District,
                    vaccine.SubDistrict,
                    vaccine.SHAStatus,
                    vaccine.IsAllergies,
                    vaccine.IsPositive,
                    vaccine.Status,
                    vaccine.TAMVaccineAgreement
                }, transaction).FirstOrDefault();

                if (data != null)
                {
                    
                    foreach (VaccineQuestion dataQuestion in vaccine.VaccineQuestionList)
                    {
                        VaccineQuestionUpsertStoredEntity question = (VaccineQuestionUpsertStoredEntity)UnitOfWork.UspQuery<VaccineQuestionUpsertStoredEntity>(new
                        {
                            dataQuestion.Id,
                            VaccineId = data.Id,
                            dataQuestion.FormQuestionId,
                            dataQuestion.Answer,
                            vaccine.CreatedBy,
                            vaccine.CreatedOn,
                            vaccine.ModifiedBy,
                            vaccine.ModifiedOn
                        },transaction).FirstOrDefault();
                        foreach(var dataQuestionDetail in dataQuestion.VaccineQuestionDetailList)
                        {
                            UnitOfWork.UspQuery<VaccineQuestionDetailUpsertStoredEntity>(new
                            {
                                dataQuestionDetail.Id,
                                VaccineQuestionId = question.Id,
                                FormQuestionDetailId = dataQuestionDetail.FormQuestionDetailId,
                                dataQuestionDetail.Answer,
                                vaccine.CreatedBy,
                                vaccine.CreatedOn,
                                vaccine.ModifiedBy,
                                vaccine.ModifiedOn
                            }, transaction);
                        }
                    }
                    
                }

            });
            UnitOfWork.SaveChanges();
            //BEGIN update Status in SP
            UnitOfWork.UspQuery("dbo.SP_VACCINE_UPDATE_STATUS", new { NoReg = vaccine.NoReg, Name = vaccine.Name });
            //END update Status in SP
            UnitOfWork.SaveChanges();

            string name = null;
            List<Vaccine> employeeData = GetVaccineQuery(vaccine.NoReg, name);

            if (employeeData.Count > 0)
            {
                var emailService = new EmailService(UnitOfWork);
                var coreService = new CoreService(UnitOfWork);

                var emailTemplate = coreService.GetEmailTemplate("vaksin-notifikasi-status");
                var instanceKey = $"app-notice";
                var mailSubject = emailTemplate.Subject;
                var mailFrom = emailTemplate.MailFrom;
                var displayName = emailTemplate.DisplayName;
                var template = Scriban.Template.Parse(emailTemplate.MailContent);

                var mailManager = emailService.CreateEmailManager();
                string tableData = @"<table style='width:100%;border-collapse:collapse;' border='1'>
                                        <tr style='text-align:center'>
                                            <td><b>Nama</b></td>
                                            <td><b>Hubungan</b></td>
                                            <td><b>Status</b></td>
                                        </tr>";
                foreach (var data in employeeData)
                {
                    tableData = tableData + "<tr style='text-align:left'>" +
                                        "<td>" + data.Name + "</td>" +
                                        "<td>" + data.FamilyStatus + "</td>" +
                                        "<td>" + data.Status + "</td>" +
                                    "</tr>";
                }

                tableData += "</table>";
                var mailContent = template.Render(new
                {
                    employee_names = employeeData[0].Name,
                    nama = employeeData[0].Name,
                    hubungan = vaccine.FamilyStatus,
                    status = vaccine.Status,
                    year = DateTime.Now.Year,
                    tabledetail = tableData,
                });

                ///mailManager.SendAsync(mailFrom, mailSubject, mailContent, employeeData[0].Email, null, null, displayName);
            }

            return UnitOfWork.SaveChanges()>0;
        }

        public bool UpsertVaccineScheduleBackdate(VaccineScheduleBackdateStoredEntity vaccine)
        {
            UnitOfWork.UspQuery("SP_VACCINE_UPDATE_SCHEDULE_BACKDATE", new
            {
                vaccine.Id,
                vaccine.VaccineDate1,
                vaccine.VaccineDate2,
                vaccine.VaccineHospital1,
                vaccine.VaccineHospital2,
                vaccine.VaccineCard1,
                vaccine.VaccineCard2,
                vaccine.ModifiedBy,
                vaccine.ModifiedOn
            });

            //var vaccineScheduleSet = UnitOfWork.GetRepository<VaccineScheduleBackdateStoredEntity>();

            //vaccineScheduleSet.Fetch()
            //        .Where(x => x.Id == vaccine.Id)
            //        .Update(x => new VaccineScheduleBackdateStoredEntity { 
            //            VaccineDate1 = vaccine.VaccineDate1, 
            //            VaccineHospital1 = vaccine.VaccineHospital1, 
            //            VaccineDate2 = vaccine.VaccineDate2,
            //            VaccineHospital2 = vaccine.VaccineHospital2,
            //            ModifiedBy = vaccine.ModifiedBy,
            //            ModifiedOn = DateTime.Now,
            //            CreatedBy = vaccine.CreatedBy,
            //            CreatedOn = DateTime.Now});
            UnitOfWork.SaveChanges();
            //BEGIN update Status in SP
            UnitOfWork.UspQuery("dbo.SP_VACCINE_UPDATE_STATUS", new { NoReg = vaccine.NoReg, Name = vaccine.Name });
            //END update Status in SP
            return UnitOfWork.SaveChanges()>0;
        }

        public Vaccine GetById(Guid Id)
        {
            return VaccineRepository.Fetch()
            .Where(x => x.Id == Id)
            .FirstOrDefault();
        }

        public DateTime ActiveDate()
        {
            ConfigService configService = new ConfigService(UnitOfWork);
            return configService.GetConfigValue<DateTime>("Vaccine.ActiveDate");
        }

        public IEnumerable<VaccineSummaryStoredEntity> GetVaccineSummaries(DateTime keyDate)
        {
            // Get list of health declaration summaries from given submission date without object tracking.
            return UnitOfWork.UdfQuery<VaccineSummaryStoredEntity>(new { keyDate })
                .OrderBy(x => x.Division);
        }

        public IQueryable<VaccinReportEmployeeView> GetReportEmployee(DateTime? vaccineDate1, DateTime? vaccineDate2)
        {
            var set = UnitOfWork.GetRepository<VaccinReportEmployeeView>()
        .Fetch()
        .OrderBy(x => x.NoReg)
        .ThenBy(x => x.OrderRank)
        .AsNoTracking();

            // Jika hanya filter VaccineDate1
            if (vaccineDate1.HasValue && !vaccineDate2.HasValue)
            {
                set = set.Where(x => x.VaccineDate1 != null && x.VaccineDate1 >= vaccineDate1.Value);
            }

            // Jika hanya filter VaccineDate2
            else if (!vaccineDate1.HasValue && vaccineDate2.HasValue)
            {
                set = set.Where(x => x.VaccineDate2 != null && x.VaccineDate2 >= vaccineDate2.Value);
            }

            // Jika keduanya difilter
            else if (vaccineDate1.HasValue && vaccineDate2.HasValue)
            {
                set = set.Where(x =>
                    (x.VaccineDate1 != null && x.VaccineDate1 >= vaccineDate1.Value) &&
                    (x.VaccineDate2 != null && x.VaccineDate2 >= vaccineDate2.Value)
                );
            }

            return set;
        }

        public IQueryable<VaccinReportEmployeeView> GetReportEmployeeDetail(string noReg, string name)
        {
            var set = UnitOfWork.GetRepository<VaccinReportEmployeeView>();
            return set.Fetch().AsNoTracking().Where(x => x.NoReg == noReg && x.Name == name);
        }

        public IQueryable<VaccineQuestionView> GeVaccineQuestion()
        {
            var set = UnitOfWork.GetRepository<VaccineQuestionView>();
            return set.Fetch().AsNoTracking();
        }

        public bool Upsertentity(string actor, VaccineMonitoringLog entity)
        {
            var monitoringLogSet = UnitOfWork.GetRepository<VaccineMonitoringLog>();

            var hasOpenLog = monitoringLogSet.Fetch()
                .Any(x => x.NoReg == entity.NoReg && x.VaccineId == entity.VaccineId && x.CreatedBy == actor && !x.Closed);

            Assert.ThrowIf(hasOpenLog && entity.Id == default, "Cannot insert new log when previous log was not closed");

            monitoringLogSet.Upsert<Guid>(entity, new[] { "Notes", "Closed" });

            if (entity.Closed)
            {
                var now = DateTime.Now;
                var currentDate = now.Date;
                var healthDeclarationSet = UnitOfWork.GetRepository<HealthDeclaration>();

                healthDeclarationSet.Fetch()
                    .Where(x => x.NoReg == entity.NoReg && x.SubmissionDate >= x.CreatedOn.Date && x.SubmissionDate <= currentDate && x.RowStatus && !x.Marked)
                    .Update(x => new HealthDeclaration { Marked = true, ModifiedBy = actor, ModifiedOn = now });
            }

            return UnitOfWork.SaveChanges() > 0;
        }
        public IQueryable<VaccineMonitoringLog> GetMonitoringLogs()
        {
            var set = UnitOfWork.GetRepository<VaccineMonitoringLog>();

            return set.Fetch()
                .AsNoTracking()
                .Where(x => x.RowStatus);
        }

        public IQueryable<ActualEntityStructure> GetDivisions()
        {
            var set = UnitOfWork.GetRepository<ActualEntityStructure>();
            var div = set.Fetch()
                .AsNoTracking()
                .Where(x => x.ObjectDescription == "Division");
            return div.Select(x => new ActualEntityStructure { ObjectText = x.ObjectText }).Distinct().OrderBy(x => x.ObjectText);
        }

        public List<Vaccine> GetClass()
        {
            return UnitOfWork.GetConnection().Query<Vaccine>(@"
            select DISTINCT NK_SubKelas as Class from vw_personal_data_information Order By NK_SubKelas
            ").ToList();
        }

        public List<Vaccine> GetPosition()
        {
            return UnitOfWork.GetConnection().Query<Vaccine>(@"
            select DISTINCT Kode_Job as JobCode, Nama_Job as JobName from vw_personal_data_information Order By Kode_Job
            ").ToList();
        }

        public void UploadVaccineAutofill(string actor, DataTable dt)
        {
            UnitOfWork.Transact(trans =>
            {
                UnitOfWork.UspQuery("SP_UPLOAD_VACCINE_AUTOFILL", new { actor, data = dt.AsTableValuedParameter("TVP_VACCINE_AUTOFILL") }, trans);

                UnitOfWork.SaveChanges();
            });
        }

        public void UploadVaccineScheduleBackdate(string actor, DataTable dt)
        {
            UnitOfWork.Transact(trans =>
            {
                UnitOfWork.UspQuery("SP_UPLOAD_VACCINE_SCHEDULE_BACKDATE", new { actor, data = dt.AsTableValuedParameter("TVP_VACCINE_SCHEDULE_BACKDATE") }, trans);

                UnitOfWork.SaveChanges();
            });
        }
    }
}
