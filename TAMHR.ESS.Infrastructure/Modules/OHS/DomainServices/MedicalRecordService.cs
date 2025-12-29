using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TAMHR.ESS.Domain;
using Agit.Domain;
using Agit.Domain.Extensions;
using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;
using Agit.Common.Extensions;
using Dapper;
using Z.EntityFramework.Plus;
using TAMHR.ESS.Infrastructure.ViewModels;
using Agit.Common;
using OfficeOpenXml;
using System.Data.SqlClient;
using System.Data;
using System.Collections;
using System.Globalization;

namespace TAMHR.ESS.Infrastructure.DomainServices
{

    public class MedicalRecordService : DomainServiceBase
    {
        #region Repositories
        protected IRepository<ReqDownloadMedical> ReqDownloadMedical => UnitOfWork.GetRepository<ReqDownloadMedical>();
        protected IReadonlyRepository<KpGroupView> KpReadonlyRepository => UnitOfWork.GetRepository<KpGroupView>();
        protected IRepository<PersonalDataMedicalHistory> GetMcuRepo => UnitOfWork.GetRepository<PersonalDataMedicalHistory>();
        protected IRepository<EmployeeAbsent> GetSickRepo => UnitOfWork.GetRepository<EmployeeAbsent>();
        protected IRepository<UPKK> GetUpkkRepo => UnitOfWork.GetRepository<UPKK>();
        protected IRepository<User> GetUserRepo => UnitOfWork.GetRepository<User>();
        protected IRepository<EmployeProfileView> GetEsdRepo => UnitOfWork.GetRepository<EmployeProfileView>();
        protected IRepository<Patient> GetPatientRepo => UnitOfWork.GetRepository<Patient>();
        protected IRepository<TanyaOhs> GetTanyaRepo => UnitOfWork.GetRepository<TanyaOhs>();
        #endregion
        private readonly string[] PropertiesInsert = new[] {
            //"Id",
            "Approver",
            "Requestor",
            "StatusRequest",

        };
        private readonly string[] PropertiesPut = new[] {
           "Approver",
           "StatusRequest",

        };
        #region Constructor
        public MedicalRecordService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        #endregion

        #region Logic
        public IEnumerable<MedicalRecordSummaryStoredEntity> GetsummaryMR(DateTime? dateFrom, DateTime? dateTo, DateTime? lahirDateDT, string namaKaryawanVendor, string noRegCsv, string noNamaKaryawanTamCsv, string noDivCsv)
        {
            return UnitOfWork.UspQuery<MedicalRecordSummaryStoredEntity>(new { DateFrom = dateFrom, DateTo = dateTo, LahirDate = lahirDateDT, NamaKaryawanVendor = namaKaryawanVendor, NoRegList = noRegCsv, NoNamaKaryawanTamList = noNamaKaryawanTamCsv, NoDivList = noDivCsv });
        }
        public IEnumerable<MedicalRecordUpkkSummaryStoredEntity> GetsummaryUpkkMR(DateTime? dateFrom, DateTime? dateTo, DateTime? lahirDateDT, string namaKaryawanVendor, string noRegCsv, string noNamaKaryawanTamCsv, string noDivCsv)
        {
            return UnitOfWork.UspQuery<MedicalRecordUpkkSummaryStoredEntity>(new { DateFrom = dateFrom, DateTo = dateTo, LahirDate = lahirDateDT, NamaKaryawanVendor = namaKaryawanVendor, NoRegList = noRegCsv, NoNamaKaryawanTamList = noNamaKaryawanTamCsv, NoDivList = noDivCsv });
        }
        public IEnumerable<MedicalRecordSickLeaveSummaryStoredEntity> GetsummarySickLeaveMR(DateTime? dateFrom, DateTime? dateTo, DateTime? lahirDateDT, string namaKaryawanVendor, string noRegCsv, string noNamaKaryawanTamCsv, string noDivCsv)
        {
            return UnitOfWork.UspQuery<MedicalRecordSickLeaveSummaryStoredEntity>(new { DateFrom = dateFrom, DateTo = dateTo, LahirDate = lahirDateDT, NamaKaryawanVendor = namaKaryawanVendor, NoRegList = noRegCsv, NoNamaKaryawanTamList = noNamaKaryawanTamCsv, NoDivList = noDivCsv });
        }

        public IEnumerable<MedicalRecordSickNessRateSummaryStoredEntity> GetsummarySickNessRateMR(DateTime? dateFrom, DateTime? dateTo, DateTime? lahirDateDT, string namaKaryawanVendor, string noRegCsv, string noNamaKaryawanTamCsv, string noDivCsv)
        {
            return UnitOfWork.UspQuery<MedicalRecordSickNessRateSummaryStoredEntity>(new { DateFrom = dateFrom, DateTo = dateTo, LahirDate = lahirDateDT, NamaKaryawanVendor = namaKaryawanVendor, NoRegList = noRegCsv, NoNamaKaryawanTamList = noNamaKaryawanTamCsv, NoDivList = noDivCsv });
        }

        public IEnumerable<MedicalRecordTotalPatientSummaryStoredEntity> GetsummaryTotalPatientMR(DateTime? dateFrom, DateTime? dateTo, DateTime? lahirDateDT, string namaKaryawanVendor, string noRegCsv, string noNamaKaryawanTamCsv, string noDivCsv)
        {
            return UnitOfWork.UspQuery<MedicalRecordTotalPatientSummaryStoredEntity>(new { DateFrom = dateFrom, DateTo = dateTo, LahirDate = lahirDateDT, NamaKaryawanVendor = namaKaryawanVendor, NoRegList = noRegCsv, NoNamaKaryawanTamList = noNamaKaryawanTamCsv, NoDivList = noDivCsv });
        }
        public IEnumerable<MedicalRecordTotalSickLeaveSummaryStoredEntity> GetsummaryTotalSickLeaveMR(DateTime? dateFrom, DateTime? dateTo, DateTime? lahirDateDT, string namaKaryawanVendor, string noRegCsv, string noNamaKaryawanTamCsv, string noDivCsv)
        {
            return UnitOfWork.UspQuery<MedicalRecordTotalSickLeaveSummaryStoredEntity>(new { DateFrom = dateFrom, DateTo = dateTo, LahirDate = lahirDateDT, NamaKaryawanVendor = namaKaryawanVendor, NoRegList = noRegCsv, NoNamaKaryawanTamList = noNamaKaryawanTamCsv, NoDivList = noDivCsv });
        }
        public IEnumerable<MedicalRecordTotalSickNessRateSummaryStoredEntity> GetsummaryTotalSickNessRateMR(DateTime? dateFrom, DateTime? dateTo, DateTime? lahirDateDT, string namaKaryawanVendor, string noRegCsv, string noNamaKaryawanTamCsv, string noDivCsv)
        {
            return UnitOfWork.UspQuery<MedicalRecordTotalSickNessRateSummaryStoredEntity>(new { DateFrom = dateFrom, DateTo = dateTo, LahirDate = lahirDateDT, NamaKaryawanVendor = namaKaryawanVendor, NoRegList = noRegCsv, NoNamaKaryawanTamList = noNamaKaryawanTamCsv, NoDivList = noDivCsv });
        }
        public IEnumerable<MedicalRecordTotalUpkkSummaryStoredEntity> GetsummaryTotalUpkkMR(DateTime? dateFrom, DateTime? dateTo, DateTime? lahirDateDT, string namaKaryawanVendor, string noRegCsv, string noNamaKaryawanTamCsv, string noDivCsv)
        {
            return UnitOfWork.UspQuery<MedicalRecordTotalUpkkSummaryStoredEntity>(new { DateFrom = dateFrom, DateTo = dateTo, LahirDate = lahirDateDT, NamaKaryawanVendor = namaKaryawanVendor, NoRegList = noRegCsv, NoNamaKaryawanTamList = noNamaKaryawanTamCsv, NoDivList = noDivCsv });
        }

        public IEnumerable<MedicalRecordHeaderStoredEntity> GetMedicalHeader(DateTime? dateFrom, DateTime? dateTo, DateTime? lahirDateDT, string namaKaryawanVendor,string Role)
        {
            return UnitOfWork.UspQuery<MedicalRecordHeaderStoredEntity>(new { DateFrom = dateFrom, DateTo = dateTo, LahirDate = lahirDateDT, NamaKaryawanVendor = namaKaryawanVendor, Role = Role });
        }
        public IQueryable<EmployeProfileView> GetByNoreg(string noreg)
        {
            //var userRepo = UnitOfWork.GetRepository<User>();
            //var VW_EMPLOYEE_STATUS_repo = UnitOfWork.GetRepository<EmployeProfileView>();
            //var User = userRepo.Fetch().AsNoTracking();
            //var Vw_Name = VW_EMPLOYEE_STATUS_repo.Fetch().AsNoTracking();


            //var result = from a in User
            //             join d in Vw_Name on a.NoReg equals d.Noreg // Pastikan properti join sesuai dengan tabel Anda
            //             where d.Noreg == noreg
            //             select new
            //             {
            //                 Gender = a.Gender,     // Mengubah Id dari Guid ke string
            //                 Nama = d.Name,// Properti dari tabel Divisi
            //             };

            //return result.Distinct().OrderBy(x => x.Nama);

            var set = UnitOfWork.GetRepository<EmployeProfileView>();

            return set.Fetch()
                .Where(p => p.Noreg == noreg)
                .AsNoTracking();

        }

        public IQueryable<KlinikTB> GetUpkkDropDown()
        {
            // Ambil repository untuk entitas Area
            var repository = UnitOfWork.GetRepository<KlinikTB>();

            // Query untuk mendapatkan data area
            return repository.Fetch()
                .AsNoTracking()
                .Select(x => new KlinikTB
                {
                    Id = x.Id,
                    Klinik = x.Klinik
                })
                .Distinct()
                .OrderBy(x => x.Klinik);
        }
        //public IQueryable<Kategori_Penyakit> GetKpDropDown()
        //{
        //    // Ambil repository untuk entitas Area
        //    var repository = UnitOfWork.GetRepository<Kategori_Penyakit>();

        //    // Query untuk mendapatkan data area
        //    return repository.Fetch()
        //        .AsNoTracking()
        //        .Select(x => new Kategori_Penyakit
        //        {
        //            //Id = x.Id,
        //            KategoriPenyakit = x.KategoriPenyakit
        //        })
        //        .Distinct()
        //        .OrderBy(x => x.KategoriPenyakit);
        //}
        public IEnumerable<Kategori_Penyakit> GetKpDropDown()
        {
            var repository = UnitOfWork.GetRepository<Kategori_Penyakit>();

            return repository.Fetch()
                .AsNoTracking()
                .Select(x => new Kategori_Penyakit
                {
                    KategoriPenyakit = x.KategoriPenyakit
                })
                .Distinct()
                .ToList() // tarik dulu ke memory
                .OrderBy(x =>
                {
                    var text = x.KategoriPenyakit;
                    var dotIndex = text.IndexOf('.');
                    if (dotIndex > 0 && int.TryParse(text.Substring(0, dotIndex), out int number))
                        return number;
                    return int.MaxValue;
                });
        }

        public IQueryable<KpGroupView> GetMappingQuery(string code)
        {
            // Get list of access groups by access role code without object tracking order by name.
            return KpReadonlyRepository.Fetch()
                .AsNoTracking()
                .Where(x => string.IsNullOrEmpty(code) || x.Code == code)
                .OrderBy(x => x.Name);
        }
        public DateTime? Gettgllahir(string noreg)
        {
            var persondata = UnitOfWork.GetRepository<PersonalData>();
            var atributdata = UnitOfWork.GetRepository<PersonalDataCommonAttribute>();

            // Cari ID berdasarkan NoReg di tabel PersonalData
            var resultpd = persondata.Fetch().AsNoTracking()
                .Where(x => x.NoReg == noreg)
                .Select(x => x.CommonAttributeId); // Ambil ID saja

            // Cari tanggal lahir berdasarkan ID yang ditemukan di PersonalDataCommonAttribute
            var resultAttr = atributdata.Fetch().AsNoTracking()
                .Where(x => resultpd.Contains(x.Id)) // Cocokkan dengan ID
                .Select(x => x.BirthDate); // Ambil hanya kolom TanggalLahir

            return resultAttr.FirstOrDefault();
        }
        public IEnumerable<SickLeaveDetailStoredEntity> GetSickLeaveDetail(string noreg, DateTime? dateFrom, DateTime? dateTo)
        {
            return UnitOfWork.UspQuery<SickLeaveDetailStoredEntity>(new { Noreg = noreg, DateFrom = dateFrom, DateTo = dateTo });
        }

        #endregion
        #region REQ DOWNLOAD MEDICAL insert update dan Validasi
        public bool GetDataReq(string employeenoreg)
        {
            var set = UnitOfWork.GetRepository<ReqDownloadMedical>();

            // Filter data berdasarkan kondisi
            var exists = set.Fetch()
                            .AsNoTracking()
                            .Any(x => x.Requestor == employeenoreg && x.StatusRequest == "Waiting Approve");

            // Return hasil pengecekan
            return exists;
        }
        public bool GetDataApprove(string employeenoreg)
        {
            var set = UnitOfWork.GetRepository<ReqDownloadMedical>();
            DateTime today = DateTime.Today;
            DateTime lastWeek = today.AddDays(-7); // Ambil 7 hari ke belakang
            // Filter data berdasarkan kondisi dalam seminggu terakhir
            //var exists = set.Fetch()
            //                .AsNoTracking()
            //                .Any(x => x.Requestor == employeenoreg && x.StatusRequest == "Approved" && x.ModifiedOn.GetValueOrDefault().Date >= lastWeek);

            var exists = set.Fetch()
        .AsNoTracking()
        .Any(x => x.Requestor == employeenoreg
               && x.StatusRequest == "Approved"
               && x.ModifiedOn.HasValue
               && x.ModifiedOn.Value.Date >= lastWeek);

            // Return hasil pengecekan
            return exists;
        }

        public void UpsertPost(ReqDownloadMedical param)
        {
            ReqDownloadMedical.Upsert<Guid>(param, PropertiesInsert);
            UnitOfWork.SaveChanges();

            // CARI ROLE NYA
            var roleId = UnitOfWork.GetRepository<Role>()
                         .Fetch()
                         .AsNoTracking()
                         .Where(r => r.RoleKey == "OHS_admin_approver")
                         .Select(r => r.Id)
                         .FirstOrDefault();

            var userIds = UnitOfWork.GetRepository<UserRole>()
                          .Fetch()
                          .AsNoTracking()
                          .Where(ur => ur.RoleId == roleId)
                          .Select(ur => ur.UserId)
                          .ToList();

            //var noRegs = UnitOfWork.GetRepository<User>()
            //            .Fetch()
            //            .AsNoTracking()
            //            .Where(u => userIds.Contains(u.Id)) // Menggunakan Contains untuk pencarian dalam list
            //            .Select(u => u.NoReg)
            //            .ToList();
            var noRegs = (from user in UnitOfWork.GetRepository<User>().Fetch().AsNoTracking()
                          join userRole in UnitOfWork.GetRepository<UserRole>().Fetch().AsNoTracking()
                              on user.Id equals userRole.UserId
                          join role in UnitOfWork.GetRepository<Role>().Fetch().AsNoTracking()
                              on userRole.RoleId equals role.Id
                          where role.RoleKey == "OHS_admin_approver"
                          select user.NoReg).ToList();

            //lOOP DATA DARI NOREG ITU\

            foreach (var itemnoreg in noRegs)
            {
                SendEmailAsync(itemnoreg, param.Requestor, param.StatusRequest); // Panggil fungsi pengiriman email
            }

            

        }
        public void UpsertPut(ReqDownloadMedical param)
        {
           
            ReqDownloadMedical.Upsert<Guid>(param, PropertiesPut);

            UnitOfWork.SaveChanges();
        }

        public IEnumerable<ReqDownloadMedicalStoredEntity> GetReqDownloadMedical( string Requestor,string Role)
        {
            return UnitOfWork.UspQuery<ReqDownloadMedicalStoredEntity>(new { Requestor = Requestor , Role = Role });
        }
        #endregion

        #region fungsi download medical record
        public IEnumerable<MedicalRecordDownloadMCUStoredEntity> GetdataMcu(DateTime? dateFrom, DateTime? dateTo, DateTime? lahirDateDT, string namaKaryawanVendor, string noRegCsv, string noNamakaryawanTamCsv, string noDivCsv)
        {
            //if (Noreg == null)
            //{
            //    Noreg = string.Empty;
            //}
            //return GetMcuRepo.Fetch()
            //    .AsNoTracking()
            //    .Where(x =>
            //        (!dateFrom.HasValue || x.YearPeriod >= dateFrom.Value.Year) &&
            //        (!dateTo.HasValue || x.YearPeriod <= dateTo.Value.Year)
            //        &&
            //        (!Noreg.Any() || Noreg.Contains(x.NoReg))
            //    );

            return UnitOfWork.UspQuery<MedicalRecordDownloadMCUStoredEntity>(new { DateFrom = dateFrom, DateTo = dateTo, LahirDate = lahirDateDT, NamaKaryawanVendor = namaKaryawanVendor, NoRegList = noRegCsv, NoNamakaryawanTamList = noNamakaryawanTamCsv, NoDivList = noDivCsv });
        }
        public IEnumerable<MedicalRecordDownloadSickLeaveStoredEntity> GetdataSick(DateTime? dateFrom, DateTime? dateTo, DateTime? lahirDateDT, string namaKaryawanVendor, string noRegCsv,string noNamakaryawanTamCsv, string noDivCsv)
        {
            //if (Noreg == null)
            //{
            //    Noreg = string.Empty;
            //}
            //return GetSickRepo.Fetch()
            //    .AsNoTracking()
            //    .Where(x =>
            //        (!dateFrom.HasValue || x.CreatedOn.Date >= dateFrom.Value.Date) &&
            //        (!dateTo.HasValue || x.CreatedOn.Date <= dateTo.Value.Date) &&
            //        (!Noreg.Any() || Noreg.Contains(x.NoReg)) &&
            //        new[] { "up-SakitBiasa", "up-SakitKeras" }.Contains(x.ReasonCode)


            //    );
            return UnitOfWork.UspQuery<MedicalRecordDownloadSickLeaveStoredEntity>(new { DateFrom = dateFrom, DateTo = dateTo, LahirDate = lahirDateDT, NamaKaryawanVendor = namaKaryawanVendor, NoRegList = noRegCsv, NoNamakaryawanTamList = noNamakaryawanTamCsv, NoDivList = noDivCsv });
        }
        #region query download upkk TABEL ALL COLOM
        public IEnumerable<MedicalRecordDownloadUpkkAllStoredEntity> GetdataUpkkAll(DateTime? dateFrom, DateTime? dateTo, DateTime? lahirDateDT, string namaKaryawanVendor, string noRegCsv, string noNamakaryawanTamCsv, string noDivCsv)
        {
            //if (Noreg == null)
            //{
            //    Noreg = string.Empty;
            //}

            //var query = from mh in GetUpkkRepo.Fetch().AsNoTracking()
            //            join u in GetUserRepo.Fetch().AsNoTracking()
            //            on mh.Noreg equals u.NoReg into userGroup
            //            from u in userGroup.DefaultIfEmpty()

            //            join esd in GetEsdRepo.Fetch().AsNoTracking()
            //            on mh.Noreg equals esd.Noreg into esdGroup
            //            from esd in esdGroup.DefaultIfEmpty()

            //            where (esd == null || esd.Expr1 == "Active Employee")
            //                  && mh.RowStatus == true
            //                  && ((!dateFrom.HasValue && !dateTo.HasValue) || (mh.TanggalKunjungan >= dateFrom && mh.TanggalKunjungan <= dateTo))
            //                  && (string.IsNullOrEmpty(Noreg) || mh.Noreg == Noreg)

            //            select new UPKK
            //            {

            //                LokasiUPKK = mh.LokasiUPKK,
            //                KategoriKunjungan = mh.KategoriKunjungan,
            //                TanggalKunjungan = mh.TanggalKunjungan,
            //                Company = mh.Divisi == "LAINNYA" ? mh.Company : "TAM",
            //                Divisi = mh.Divisi == "LAINNYA" ? mh.Divisi : (esd != null ? esd.Divisi : null),
            //                Noreg = mh.Noreg,
            //                NamaEmployeeVendor = mh.Divisi == "LAINNYA" ? mh.NamaEmployeeVendor : (esd != null ? esd.Name : null),

            //                Usia = mh.Usia,
            //                JenisKelaminEmployeeVendor = mh.Divisi == "LAINNYA" ? mh.JenisKelaminEmployeeVendor : (u != null ? u.Gender : null),
            //                JenisPekerjaan = mh.JenisPekerjaan,
            //                AreaId = mh.AreaId,
            //                LokasiKerja = mh.LokasiKerja,
            //                Keluhan = mh.Keluhan,
            //                TDSistole = mh.TDSistole,
            //                TDDiastole = mh.TDDiastole,
            //                Nadi = mh.Nadi,
            //                Respirasi = mh.Respirasi,
            //                Suhu = mh.Suhu,
            //                Diagnosa = mh.Diagnosa,
            //                KategoriPenyakit = mh.KategoriPenyakit,
            //                SpesifikPenyakit = mh.SpesifikPenyakit,
            //                JenisKasus = mh.JenisKasus,
            //                Treatment = mh.Treatment,
            //                Pemeriksa = mh.Pemeriksa,
            //                NamaPemeriksa = mh.NamaPemeriksa,
            //                HasilAkhir = mh.HasilAkhir,

            //            };


            //return query.AsQueryable();
            return UnitOfWork.UspQuery<MedicalRecordDownloadUpkkAllStoredEntity>(new { DateFrom = dateFrom, DateTo = dateTo, LahirDate = lahirDateDT, NamaKaryawanVendor = namaKaryawanVendor, NoRegList = noRegCsv, NoNamakaryawanTamList = noNamakaryawanTamCsv, NoDivList = noDivCsv });
        }
        #endregion
        #region query download upkk TABEL XL 1
        public IEnumerable<MedicalRecordDownloadUpkkTab1StoredEntity> GetdataUpkk1(DateTime? dateFrom, DateTime? dateTo, DateTime? lahirDateDT, string namaKaryawanVendor, string noRegCsv, string noNamakaryawanTamCsv, string noDivCsv)
        {
            //if (Noreg == null)
            //{
            //    Noreg = string.Empty;
            //}

            //var query = GetUpkkRepo.Fetch()
            //    .AsNoTracking()
            //    .Where(x =>
            //        (!dateFrom.HasValue || x.TanggalKunjungan >= dateFrom.Value.Date) &&
            //        (!dateTo.HasValue || x.TanggalKunjungan <= dateTo.Value.Date) &&
            //        (string.IsNullOrEmpty(Noreg) || x.Noreg.Contains(Noreg)) &&
            //        x.LokasiUPKK == "UPKK Sunter 2 - TSD"
            //    );

            //var visitNumberQuery = query
            //    .Select(x => new UPKK
            //    {
            //        Kategori = "VisitNumber",
            //        LokasiUpkkGroup = x.LokasiUPKK,
            //        TAM = query.Count(y => y.Divisi != "LAINNYA"),
            //        Outsource = query.Count(y => y.Divisi == "LAINNYA")
            //    })
            //    .Distinct();

            //var totalVisitorQuery = query
            //    .Select(x => new UPKK
            //    {
            //        Kategori = "TotalVisitor",
            //        LokasiUpkkGroup = x.LokasiUPKK,
            //        TAM = query.Select(y => y.Noreg).Where(noreg => query.Any(y => y.Divisi != "LAINNYA" && y.Noreg == noreg)).Distinct().Count(),
            //        Outsource = query.Select(y => y.Noreg).Where(noreg => query.Any(y => y.Divisi == "LAINNYA" && y.Noreg == noreg)).Distinct().Count()
            //    })
            //    .Distinct();

            //return visitNumberQuery.Union(totalVisitorQuery).AsQueryable();
            return UnitOfWork.UspQuery<MedicalRecordDownloadUpkkTab1StoredEntity>(new { DateFrom = dateFrom, DateTo = dateTo, LahirDate = lahirDateDT, NamaKaryawanVendor = namaKaryawanVendor, NoRegList = noRegCsv, NoNamakaryawanTamList = noNamakaryawanTamCsv, NoDivList = noDivCsv });
        }
        public IQueryable<UPKK> GetdataUpkk2(DateTime? dateFrom, DateTime? dateTo, string Noreg)
        {
            if (Noreg == null)
            {
                Noreg = string.Empty;
            }

            var query = GetUpkkRepo.Fetch()
                .AsNoTracking()
                .Where(x =>
                     (!dateFrom.HasValue || x.TanggalKunjungan >= dateFrom.Value.Date) &&
                    (!dateTo.HasValue || x.TanggalKunjungan <= dateTo.Value.Date) &&
                    (string.IsNullOrEmpty(Noreg) || x.Noreg.Contains(Noreg)) &&
                    x.LokasiUPKK == "UPKK Sunter 2 - PPDD"
                );

            var visitNumberQuery = query
                .Select(x => new UPKK
                {
                    Kategori = "VisitNumber",
                    LokasiUpkkGroup = x.LokasiUPKK,
                    TAM = query.Count(y => y.Divisi != "LAINNYA"),
                    Outsource = query.Count(y => y.Divisi == "LAINNYA")
                })
                .Distinct();

            var totalVisitorQuery = query
                .Select(x => new UPKK
                {
                    Kategori = "TotalVisitor",
                    LokasiUpkkGroup = x.LokasiUPKK,
                    TAM = query.Select(y => y.Noreg).Where(noreg => query.Any(y => y.Divisi != "LAINNYA" && y.Noreg == noreg)).Distinct().Count(),
                    Outsource = query.Select(y => y.Noreg).Where(noreg => query.Any(y => y.Divisi == "LAINNYA" && y.Noreg == noreg)).Distinct().Count()
                })
                .Distinct();

            return visitNumberQuery.Union(totalVisitorQuery).AsQueryable();
        }
        public IQueryable<UPKK> GetdataUpkk3(DateTime? dateFrom, DateTime? dateTo, string Noreg)
        {
            if (Noreg == null)
            {
                Noreg = string.Empty;
            }

            var query = GetUpkkRepo.Fetch()
                .AsNoTracking()
                .Where(x =>
                     (!dateFrom.HasValue || x.TanggalKunjungan >= dateFrom.Value.Date) &&
                    (!dateTo.HasValue || x.TanggalKunjungan <= dateTo.Value.Date) &&
                    (string.IsNullOrEmpty(Noreg) || x.Noreg.Contains(Noreg)) &&
                    x.LokasiUPKK == "UPKK Sunter 2 - HRGA"
                );

            var visitNumberQuery = query
                .Select(x => new UPKK
                {
                    Kategori = "VisitNumber",
                    LokasiUpkkGroup = x.LokasiUPKK,
                    TAM = query.Count(y => y.Divisi != "LAINNYA"),
                    Outsource = query.Count(y => y.Divisi == "LAINNYA")
                })
                .Distinct();

            var totalVisitorQuery = query
                .Select(x => new UPKK
                {
                    Kategori = "TotalVisitor",
                    LokasiUpkkGroup = x.LokasiUPKK,
                    TAM = query.Select(y => y.Noreg).Where(noreg => query.Any(y => y.Divisi != "LAINNYA" && y.Noreg == noreg)).Distinct().Count(),
                    Outsource = query.Select(y => y.Noreg).Where(noreg => query.Any(y => y.Divisi == "LAINNYA" && y.Noreg == noreg)).Distinct().Count()
                })
                .Distinct();

            return visitNumberQuery.Union(totalVisitorQuery).AsQueryable();
        }
        public IQueryable<UPKK> GetdataUpkk4(DateTime? dateFrom, DateTime? dateTo, string Noreg)
        {
            if (Noreg == null)
            {
                Noreg = string.Empty;
            }

            var query = GetUpkkRepo.Fetch()
                .AsNoTracking()
                .Where(x =>
                     (!dateFrom.HasValue || x.TanggalKunjungan >= dateFrom.Value.Date) &&
                    (!dateTo.HasValue || x.TanggalKunjungan <= dateTo.Value.Date) &&
                    (string.IsNullOrEmpty(Noreg) || x.Noreg.Contains(Noreg)) &&
                    x.LokasiUPKK == "UPKK Lexus - LNTC"
                );

            var visitNumberQuery = query
                .Select(x => new UPKK
                {
                    Kategori = "VisitNumber",
                    LokasiUpkkGroup = x.LokasiUPKK,
                    TAM = query.Count(y => y.Divisi != "LAINNYA"),
                    Outsource = query.Count(y => y.Divisi == "LAINNYA")
                })
                .Distinct();

            var totalVisitorQuery = query
                .Select(x => new UPKK
                {
                    Kategori = "TotalVisitor",
                    LokasiUpkkGroup = x.LokasiUPKK,
                    TAM = query.Select(y => y.Noreg).Where(noreg => query.Any(y => y.Divisi != "LAINNYA" && y.Noreg == noreg)).Distinct().Count(),
                    Outsource = query.Select(y => y.Noreg).Where(noreg => query.Any(y => y.Divisi == "LAINNYA" && y.Noreg == noreg)).Distinct().Count()
                })
                .Distinct();

            return visitNumberQuery.Union(totalVisitorQuery).AsQueryable();
        }
        public IQueryable<UPKK> GetdataUpkk5(DateTime? dateFrom, DateTime? dateTo, string Noreg)
        {
            if (Noreg == null)
            {
                Noreg = string.Empty;
            }

            var query = GetUpkkRepo.Fetch()
                .AsNoTracking()
                .Where(x =>
                     (!dateFrom.HasValue || x.TanggalKunjungan >= dateFrom.Value.Date) &&
                    (!dateTo.HasValue || x.TanggalKunjungan <= dateTo.Value.Date) &&
                    (string.IsNullOrEmpty(Noreg) || x.Noreg.Contains(Noreg)) &&
                    x.LokasiUPKK == "UPKK Lexus - Menteng"
                );

            var visitNumberQuery = query
                .Select(x => new UPKK
                {
                    Kategori = "VisitNumber",
                    LokasiUpkkGroup = x.LokasiUPKK,
                    TAM = query.Count(y => y.Divisi != "LAINNYA"),
                    Outsource = query.Count(y => y.Divisi == "LAINNYA")
                })
                .Distinct();

            var totalVisitorQuery = query
                .Select(x => new UPKK
                {
                    Kategori = "TotalVisitor",
                    LokasiUpkkGroup = x.LokasiUPKK,
                    TAM = query.Select(y => y.Noreg).Where(noreg => query.Any(y => y.Divisi != "LAINNYA" && y.Noreg == noreg)).Distinct().Count(),
                    Outsource = query.Select(y => y.Noreg).Where(noreg => query.Any(y => y.Divisi == "LAINNYA" && y.Noreg == noreg)).Distinct().Count()
                })
                .Distinct();

            return visitNumberQuery.Union(totalVisitorQuery).AsQueryable();
        }
        public IQueryable<UPKK> GetdataUpkk6(DateTime? dateFrom, DateTime? dateTo, string Noreg)
        {
            if (Noreg == null)
            {
                Noreg = string.Empty;
            }

            var query = GetUpkkRepo.Fetch()
                .AsNoTracking()
                .Where(x =>
                     (!dateFrom.HasValue || x.TanggalKunjungan >= dateFrom.Value.Date) &&
                    (!dateTo.HasValue || x.TanggalKunjungan <= dateTo.Value.Date) &&
                    (string.IsNullOrEmpty(Noreg) || x.Noreg.Contains(Noreg)) &&
                    x.LokasiUPKK == "UPKK SPLD"
                );

            var visitNumberQuery = query
                .Select(x => new UPKK
                {
                    Kategori = "VisitNumber",
                    LokasiUpkkGroup = x.LokasiUPKK,
                    TAM = query.Count(y => y.Divisi != "LAINNYA"),
                    Outsource = query.Count(y => y.Divisi == "LAINNYA")
                })
                .Distinct();

            var totalVisitorQuery = query
                .Select(x => new UPKK
                {
                    Kategori = "TotalVisitor",
                    LokasiUpkkGroup = x.LokasiUPKK,
                    TAM = query.Select(y => y.Noreg).Where(noreg => query.Any(y => y.Divisi != "LAINNYA" && y.Noreg == noreg)).Distinct().Count(),
                    Outsource = query.Select(y => y.Noreg).Where(noreg => query.Any(y => y.Divisi == "LAINNYA" && y.Noreg == noreg)).Distinct().Count()
                })
                .Distinct();

            return visitNumberQuery.Union(totalVisitorQuery).AsQueryable();
        }
        public IQueryable<UPKK> GetdataUpkk7(DateTime? dateFrom, DateTime? dateTo, string Noreg)
        {
            if (Noreg == null)
            {
                Noreg = string.Empty;
            }

            var query = GetUpkkRepo.Fetch()
                .AsNoTracking()
                .Where(x =>
                     (!dateFrom.HasValue || x.TanggalKunjungan >= dateFrom.Value.Date) &&
                    (!dateTo.HasValue || x.TanggalKunjungan <= dateTo.Value.Date) &&
                    (string.IsNullOrEmpty(Noreg) || x.Noreg.Contains(Noreg)) &&
                    x.LokasiUPKK == "UPKK TTC"
                );

            var visitNumberQuery = query
                .Select(x => new UPKK
                {
                    Kategori = "VisitNumber",
                    LokasiUpkkGroup = x.LokasiUPKK,
                    TAM = query.Count(y => y.Divisi != "LAINNYA"),
                    Outsource = query.Count(y => y.Divisi == "LAINNYA")
                })
                .Distinct();

            var totalVisitorQuery = query
                .Select(x => new UPKK
                {
                    Kategori = "TotalVisitor",
                    LokasiUpkkGroup = x.LokasiUPKK,
                    TAM = query.Select(y => y.Noreg).Where(noreg => query.Any(y => y.Divisi != "LAINNYA" && y.Noreg == noreg)).Distinct().Count(),
                    Outsource = query.Select(y => y.Noreg).Where(noreg => query.Any(y => y.Divisi == "LAINNYA" && y.Noreg == noreg)).Distinct().Count()
                })
                .Distinct();

            return visitNumberQuery.Union(totalVisitorQuery).AsQueryable();
        }
        public IQueryable<UPKK> GetdataUpkk8(DateTime? dateFrom, DateTime? dateTo, string Noreg)
        {
            if (Noreg == null)
            {
                Noreg = string.Empty;
            }

            var query = GetUpkkRepo.Fetch()
                .AsNoTracking()
                .Where(x =>
                     (!dateFrom.HasValue || x.TanggalKunjungan >= dateFrom.Value.Date) &&
                    (!dateTo.HasValue || x.TanggalKunjungan <= dateTo.Value.Date) &&
                    (string.IsNullOrEmpty(Noreg) || x.Noreg.Contains(Noreg)) &&
                    x.LokasiUPKK == "UPKK Sunter 3 - VLD"
                );

            var visitNumberQuery = query
                .Select(x => new UPKK
                {
                    Kategori = "VisitNumber",
                    LokasiUpkkGroup = x.LokasiUPKK,
                    TAM = query.Count(y => y.Divisi != "LAINNYA"),
                    Outsource = query.Count(y => y.Divisi == "LAINNYA")
                })
                .Distinct();

            var totalVisitorQuery = query
                .Select(x => new UPKK
                {
                    Kategori = "TotalVisitor",
                    LokasiUpkkGroup = x.LokasiUPKK,
                    TAM = query.Select(y => y.Noreg).Where(noreg => query.Any(y => y.Divisi != "LAINNYA" && y.Noreg == noreg)).Distinct().Count(),
                    Outsource = query.Select(y => y.Noreg).Where(noreg => query.Any(y => y.Divisi == "LAINNYA" && y.Noreg == noreg)).Distinct().Count()
                })
                .Distinct();

            return visitNumberQuery.Union(totalVisitorQuery).AsQueryable();
        }
        public IQueryable<UPKK> GetdataUpkk9(DateTime? dateFrom, DateTime? dateTo, string Noreg)
        {
            if (Noreg == null)
            {
                Noreg = string.Empty;
            }

            var query = GetUpkkRepo.Fetch()
                .AsNoTracking()
                .Where(x =>
                     (!dateFrom.HasValue || x.TanggalKunjungan >= dateFrom.Value.Date) &&
                    (!dateTo.HasValue || x.TanggalKunjungan <= dateTo.Value.Date) &&
                    (string.IsNullOrEmpty(Noreg) || x.Noreg.Contains(Noreg)) &&
                    x.LokasiUPKK == "UPKK CCY - VLD"
                );

            var visitNumberQuery = query
                .Select(x => new UPKK
                {
                    Kategori = "VisitNumber",
                    LokasiUpkkGroup = x.LokasiUPKK,
                    TAM = query.Count(y => y.Divisi != "LAINNYA"),
                    Outsource = query.Count(y => y.Divisi == "LAINNYA")
                })
                .Distinct();

            var totalVisitorQuery = query
                .Select(x => new UPKK
                {
                    Kategori = "TotalVisitor",
                    LokasiUpkkGroup = x.LokasiUPKK,
                    TAM = query.Select(y => y.Noreg).Where(noreg => query.Any(y => y.Divisi != "LAINNYA" && y.Noreg == noreg)).Distinct().Count(),
                    Outsource = query.Select(y => y.Noreg).Where(noreg => query.Any(y => y.Divisi == "LAINNYA" && y.Noreg == noreg)).Distinct().Count()
                })
                .Distinct();

            return visitNumberQuery.Union(totalVisitorQuery).AsQueryable();
        }
        public IQueryable<UPKK> GetdataUpkk10(DateTime? dateFrom, DateTime? dateTo, string Noreg)
        {
            if (Noreg == null)
            {
                Noreg = string.Empty;
            }

            var query = GetUpkkRepo.Fetch()
                .AsNoTracking()
                .Where(x =>
                     (!dateFrom.HasValue || x.TanggalKunjungan >= dateFrom.Value.Date) &&
                    (!dateTo.HasValue || x.TanggalKunjungan <= dateTo.Value.Date) &&
                    (string.IsNullOrEmpty(Noreg) || x.Noreg.Contains(Noreg)) &&
                    x.LokasiUPKK == "UPKK Karawang - VLD"
                );

            var visitNumberQuery = query
                .Select(x => new UPKK
                {
                    Kategori = "VisitNumber",
                    LokasiUpkkGroup = x.LokasiUPKK,
                    TAM = query.Count(y => y.Divisi != "LAINNYA"),
                    Outsource = query.Count(y => y.Divisi == "LAINNYA")
                })
                .Distinct();

            var totalVisitorQuery = query
                .Select(x => new UPKK
                {
                    Kategori = "TotalVisitor",
                    LokasiUpkkGroup = x.LokasiUPKK,
                    TAM = query.Select(y => y.Noreg).Where(noreg => query.Any(y => y.Divisi != "LAINNYA" && y.Noreg == noreg)).Distinct().Count(),
                    Outsource = query.Select(y => y.Noreg).Where(noreg => query.Any(y => y.Divisi == "LAINNYA" && y.Noreg == noreg)).Distinct().Count()
                })
                .Distinct();

            return visitNumberQuery.Union(totalVisitorQuery).AsQueryable();
        }
        public IQueryable<UPKK> GetdataUpkk11(DateTime? dateFrom, DateTime? dateTo, string Noreg)
        {
            if (Noreg == null)
            {
                Noreg = string.Empty;
            }

            var query = GetUpkkRepo.Fetch()
                .AsNoTracking()
                .Where(x =>
                    (!dateFrom.HasValue || x.TanggalKunjungan >= dateFrom.Value.Date) &&
                    (!dateTo.HasValue || x.TanggalKunjungan <= dateTo.Value.Date) &&
                    (string.IsNullOrEmpty(Noreg) || x.Noreg.Contains(Noreg)) &&
                    x.LokasiUPKK == "UPKK Ngoro - VLD"
                );

            var visitNumberQuery = query
                .Select(x => new UPKK
                {
                    Kategori = "VisitNumber",
                    LokasiUpkkGroup = x.LokasiUPKK,
                    TAM = query.Count(y => y.Divisi != "LAINNYA"),
                    Outsource = query.Count(y => y.Divisi == "LAINNYA")
                })
                .Distinct();

            var totalVisitorQuery = query
                .Select(x => new UPKK
                {
                    Kategori = "TotalVisitor",
                    LokasiUpkkGroup = x.LokasiUPKK,
                    TAM = query.Select(y => y.Noreg).Where(noreg => query.Any(y => y.Divisi != "LAINNYA" && y.Noreg == noreg)).Distinct().Count(),
                    Outsource = query.Select(y => y.Noreg).Where(noreg => query.Any(y => y.Divisi == "LAINNYA" && y.Noreg == noreg)).Distinct().Count()
                })
                .Distinct();

            return visitNumberQuery.Union(totalVisitorQuery).AsQueryable();
        }
        #endregion

        #region download upkk tabel xl 2
        public IEnumerable<MedicalRecordDownloadUpkkTab2StoredEntity> GetdataUpkkTab2(DateTime? dateFrom, DateTime? dateTo, DateTime? lahirDateDT, string namaKaryawanVendor, string noRegCsv, string noNamakaryawanTamCsv, string noDivCsv)
        {
            //if (string.IsNullOrEmpty(Noreg))
            //{
            //    Noreg = string.Empty;
            //}

            //var query = GetUpkkRepo.Fetch()
            //    .AsNoTracking()
            //     //.Where(x => x.LokasiUPKK == "UPKK Lexus - LNTC");
            //     //.Where(x => x.KategoriPenyakit != "");
            //     .Where(x =>
            //     (!dateFrom.HasValue || x.TanggalKunjungan >= dateFrom.Value.Date) &&
            //        (!dateTo.HasValue || x.TanggalKunjungan <= dateTo.Value.Date) &&
            //        (string.IsNullOrEmpty(Noreg) || x.Noreg.Contains(Noreg)) &&
            //        (!string.IsNullOrWhiteSpace(x.KategoriPenyakit))
            //    );

            //var result = query.GroupBy(x => x.KategoriPenyakit)
            //    .Select(g => new UPKK
            //    {
            //        KategoriPenyakit = g.Key,
            //        //TAM = query.Select(y => y.Noreg).Where(noreg => query.Any(y => y.Divisi != "LAINNYA" && y.Noreg == noreg)).Distinct().Count(),
            //        //Outsource = query.Select(y => y.Noreg).Where(noreg => query.Any(y => y.Divisi == "LAINNYA" && y.Noreg == noreg)).Distinct().Count(),
            //        TAM = g.Where(x => x.Divisi != "LAINNYA").Select(x => x.Noreg).Distinct().Count(),
            //        Outsource = g.Where(x => x.Divisi == "LAINNYA").Select(x => x.Noreg).Distinct().Count(),
            //        Total = g.Select(x => x.Noreg).Distinct().Count()
            //    });

            ////var result = from u in GetUpkkRepo.Fetch().AsNoTracking()
            ////             where !string.IsNullOrWhiteSpace(u.KategoriPenyakit)
            ////             group u by u.KategoriPenyakit into g
            ////             select new UPKK
            ////             {
            ////                 KategoriPenyakit = g.Key,
            ////                 TAM = g.Where(x => x.Divisi != "LAINNYA").Select(x => x.Noreg).Distinct().Count(),
            ////                 Outsource = g.Where(x => x.Divisi == "LAINNYA").Select(x => x.Noreg).Distinct().Count(),
            ////                 Total = g.Select(x => x.Noreg).Distinct().Count()
            ////                // AreaId = g.FirstOrDefault().AreaId.ToString() // Konversi ke string
            ////             };

            //return result.ToList().AsQueryable();
            return UnitOfWork.UspQuery<MedicalRecordDownloadUpkkTab2StoredEntity>(new { DateFrom = dateFrom, DateTo = dateTo, LahirDate = lahirDateDT, NamaKaryawanVendor = namaKaryawanVendor, NoRegList = noRegCsv, NoNamakaryawanTamList = noNamakaryawanTamCsv, NoDivList = noDivCsv });
        }
        #endregion

        #region download upkk tabel xl 3
        public IEnumerable<MedicalRecordDownloadUpkkTab3StoredEntity> GetdataUpkkTab3(DateTime? dateFrom, DateTime? dateTo, DateTime? lahirDateDT, string namaKaryawanVendor, string noRegCsv, string noNamakaryawanTamCsv, string noDivCsv)
        {
            //String Noreg = "";
            //if (string.IsNullOrEmpty(Noreg))
            //{
            //    Noreg = string.Empty;
            //}

            //var query = GetUpkkRepo.Fetch()
            //    .AsNoTracking()
            //    //.Where(x => x.LokasiUPKK == "UPKK Lexus - LNTC");
            //    //.Where(x => x.KategoriPenyakit != "");
            //    .Where(x =>
            //        (!dateFrom.HasValue || x.TanggalKunjungan >= dateFrom.Value.Date) &&
            //        (!dateTo.HasValue || x.TanggalKunjungan <= dateTo.Value.Date) &&
            //        (string.IsNullOrEmpty(Noreg) || x.Noreg.Contains(Noreg)) &&
            //        (!string.IsNullOrEmpty(x.KategoriPenyakit)) &&
            //        (!string.IsNullOrEmpty(x.LokasiUPKK))
            //     );

            //var result = query.GroupBy(x => x.KategoriPenyakit)
            //    .Select(g => new UPKK
            //    {
            //        KategoriPenyakit = g.Key,
            //        TAM_TSD = g.Select(x => x.Noreg).Distinct().Count(x => g.Any(y => y.LokasiUPKK == "UPKK Sunter 2 - TSD" && y.Divisi != "LAINNYA" && y.Noreg == x)),
            //        Outsource_TSD = g.Select(x => x.Noreg).Distinct().Count(x => g.Any(y => y.LokasiUPKK == "UPKK Sunter 2 - TSD" && y.Divisi == "LAINNYA" && y.Noreg == x)),
            //        TAM_PPDD = g.Select(x => x.Noreg).Distinct().Count(x => g.Any(y => y.LokasiUPKK == "UPKK Sunter 2 - PPDD" && y.Divisi != "LAINNYA" && y.Noreg == x)),
            //        Outsource_PPDD = g.Select(x => x.Noreg).Distinct().Count(x => g.Any(y => y.LokasiUPKK == "UPKK Sunter 2 - PPDD" && y.Divisi == "LAINNYA" && y.Noreg == x)),
            //        TAM_HRGA = g.Select(x => x.Noreg).Distinct().Count(x => g.Any(y => y.LokasiUPKK == "UPKK Sunter 2 - HRGA" && y.Divisi != "LAINNYA" && y.Noreg == x)),
            //        Outsource_HRGA = g.Select(x => x.Noreg).Distinct().Count(x => g.Any(y => y.LokasiUPKK == "UPKK Sunter 2 - HRGA" && y.Divisi == "LAINNYA" && y.Noreg == x)),
            //        TAM_LNTC = g.Select(x => x.Noreg).Distinct().Count(x => g.Any(y => y.LokasiUPKK == "UPKK Lexus - LNTC" && y.Divisi != "LAINNYA" && y.Noreg == x)),
            //        Outsource_LNTC = g.Select(x => x.Noreg).Distinct().Count(x => g.Any(y => y.LokasiUPKK == "UPKK Lexus - LNTC" && y.Divisi == "LAINNYA" && y.Noreg == x)),
            //        TAM_MENTENG = g.Select(x => x.Noreg).Distinct().Count(x => g.Any(y => y.LokasiUPKK == "UPKK Lexus - Menteng" && y.Divisi != "LAINNYA" && y.Noreg == x)),
            //        Outsource_MENTENG = g.Select(x => x.Noreg).Distinct().Count(x => g.Any(y => y.LokasiUPKK == "UPKK Lexus - Menteng" && y.Divisi == "LAINNYA" && y.Noreg == x)),
            //        TAM_SPLD = g.Select(x => x.Noreg).Distinct().Count(x => g.Any(y => y.LokasiUPKK == "UPKK SPLD" && y.Divisi != "LAINNYA" && y.Noreg == x)),
            //        Outsource_SPLD = g.Select(x => x.Noreg).Distinct().Count(x => g.Any(y => y.LokasiUPKK == "UPKK SPLD" && y.Divisi == "LAINNYA" && y.Noreg == x)),
            //        TAM_TTC = g.Select(x => x.Noreg).Distinct().Count(x => g.Any(y => y.LokasiUPKK == "UPKK TTC" && y.Divisi != "LAINNYA" && y.Noreg == x)),
            //        Outsource_TTC = g.Select(x => x.Noreg).Distinct().Count(x => g.Any(y => y.LokasiUPKK == "UPKK TTC" && y.Divisi == "LAINNYA" && y.Noreg == x)),
            //        TAM_SUNTER3 = g.Select(x => x.Noreg).Distinct().Count(x => g.Any(y => y.LokasiUPKK == "UPKK Sunter 3 - VLD" && y.Divisi != "LAINNYA" && y.Noreg == x)),
            //        Outsource_SUNTER3 = g.Select(x => x.Noreg).Distinct().Count(x => g.Any(y => y.LokasiUPKK == "UPKK Sunter 3 - VLD" && y.Divisi == "LAINNYA" && y.Noreg == x)),
            //        TAM_CCY = g.Select(x => x.Noreg).Distinct().Count(x => g.Any(y => y.LokasiUPKK == "UPKK CCY - VLD" && y.Divisi != "LAINNYA" && y.Noreg == x)),
            //        Outsource_CCY = g.Select(x => x.Noreg).Distinct().Count(x => g.Any(y => y.LokasiUPKK == "UPKK CCY - VLD" && y.Divisi == "LAINNYA" && y.Noreg == x))
            //    });



            //return result.ToList().AsQueryable();
            return UnitOfWork.UspQuery<MedicalRecordDownloadUpkkTab3StoredEntity>(new { DateFrom = dateFrom, DateTo = dateTo, LahirDate = lahirDateDT, NamaKaryawanVendor = namaKaryawanVendor, NoRegList = noRegCsv, NoNamakaryawanTamList = noNamakaryawanTamCsv, NoDivList = noDivCsv });
        }
        #endregion

        public IEnumerable<MedicalRecordDownloadInOutPatientStoredEntity> GetdataPatient(DateTime? dateFrom, DateTime? dateTo, DateTime? lahirDateDT, string namaKaryawanVendor, string noRegCsv, string noNamakaryawanTamCsv, string noDivCsv)
        {
            //if (Noreg == null)
            //{
            //    Noreg = string.Empty;
            //}
            //return GetPatientRepo.Fetch()
            //    .AsNoTracking()
            //    .Where(x =>
            //        (!dateFrom.HasValue || x.AdmissionDate >= dateFrom.Value.Date) &&
            //        (!dateTo.HasValue || x.AdmissionDate <= dateTo.Value.Date) &&
            //        (!Noreg.Any() || Noreg.Contains(x.Noreg))
            //    );
            return UnitOfWork.UspQuery<MedicalRecordDownloadInOutPatientStoredEntity>(new { DateFrom = dateFrom, DateTo = dateTo, LahirDate = lahirDateDT, NamaKaryawanVendor = namaKaryawanVendor, NoRegList = noRegCsv, NoNamakaryawanTamList = noNamakaryawanTamCsv, NoDivList = noDivCsv });


        }
        public IEnumerable<MedicalRecordDownloadTanyaOhsStoredEntity> GetdataTanyaOhs(DateTime? dateFrom, DateTime? dateTo, DateTime? lahirDateDT, string namaKaryawanVendor, string noRegCsv, string noNamakaryawanTamCsv, string noDivCsv)
        {
            //if (Noreg == null)
            //{
            //    Noreg = string.Empty;
            //}
            //return GetTanyaRepo.Fetch()
            //    .AsNoTracking()
            //    .Where(x =>
            //        (!dateFrom.HasValue || x.CreatedOn.Date >= dateFrom.Value.Date) &&
            //        (!dateTo.HasValue || x.CreatedOn.Date <= dateTo.Value.Date) &&
            //        (!Noreg.Any() || Noreg.Contains(x.Noreg))
            //    );

            return UnitOfWork.UspQuery<MedicalRecordDownloadTanyaOhsStoredEntity>(new { DateFrom = dateFrom, DateTo = dateTo, LahirDate = lahirDateDT, NamaKaryawanVendor = namaKaryawanVendor, NoRegList = noRegCsv, NoNamakaryawanTamList = noNamakaryawanTamCsv, NoDivList = noDivCsv });
        }

        #endregion

        public void SendEmailAsync(string noregKey, string RequestFrom, string Status)
        {

            var users = new UserService(this.UnitOfWork).GetByNoReg(noregKey);
            var emailService = new EmailService(UnitOfWork);
            var coreService = new CoreService(UnitOfWork);

            var emailTemplate = coreService.GetEmailTemplate("ohs-reminder-approver-email");
            var mailSubject = emailTemplate.Subject;
            var mailFrom = emailTemplate.MailFrom;
            var template = Scriban.Template.Parse(emailTemplate.MailContent);

            var mailManager = emailService.CreateEmailManager();
            var mailContent = template.Render(new
            {

                requestor = RequestFrom,
                status = Status,
                year = DateTime.Now.Year
            });

            mailManager.SendAsync(mailFrom, mailSubject, mailContent, string.Join(",", users.Email));
        }

    }
}
