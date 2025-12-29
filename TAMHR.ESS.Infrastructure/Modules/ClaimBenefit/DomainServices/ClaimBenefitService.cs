using System;
using System.Linq;
using TAMHR.ESS.Domain;
using Agit.Domain;
using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;
using Z.EntityFramework.Plus;
using System.Collections.Generic;
using TAMHR.ESS.Infrastructure.ViewModels;
using Newtonsoft.Json;
using Microsoft.Extensions.Localization;
using Microsoft.EntityFrameworkCore;
using Agit.Common.Utility;
using Agit.Common;
using Agit.Domain.Extensions;
using Microsoft.SqlServer.Server;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    public class ClaimBenefitService : DomainServiceBase
    {
        #region Repositories
        /// <summary>
        /// Document approval repository
        /// </summary>
        protected IRepository<DocumentApproval> DocumentApprovalRepository => UnitOfWork.GetRepository<DocumentApproval>();

        /// <summary>
        /// User repository
        /// </summary>
        protected IRepository<User> UserRepository => UnitOfWork.GetRepository<User>();
        protected IRepository<UserPositionView> UserPosition => UnitOfWork.GetRepository<UserPositionView>();

        /// <summary>
        /// Document approval file repository
        /// </summary>
        protected IRepository<DocumentApprovalFile> DocumentApprovalFileRepository => UnitOfWork.GetRepository<DocumentApprovalFile>();

        /// <summary>
        /// Document request detail repository
        /// </summary>
        protected IRepository<DocumentRequestDetail> DocumentRequestDetailRepository => UnitOfWork.GetRepository<DocumentRequestDetail>();

        /// <summary>
        /// Tracking Approval repository
        /// </summary>
        protected IRepository<TrackingApproval> TrackingApprovalRepository => UnitOfWork.GetRepository<TrackingApproval>();

        /// <summary>
        /// Approval matrix repository
        /// </summary>
        protected IRepository<ApprovalMatrix> ApprovalMatrixRepository => UnitOfWork.GetRepository<ApprovalMatrix>();

        /// <summary>
        /// Claim benefit general repository
        /// </summary>
        protected IRepository<General> ClaimGeneralRepository => UnitOfWork.GetRepository<General>();

        /// <summary>
        /// Recreation reward repository
        /// </summary>
        protected IRepository<RecreationReward> RecreationRewardRepository => UnitOfWork.GetRepository<RecreationReward>();

        /// <summary>
        /// Recreation reward member repository
        /// </summary>
        protected IRepository<RecreationRewardMember> RecreationRewardMemberRepository => UnitOfWork.GetRepository<RecreationRewardMember>();

        /// <summary>
        /// Distressed repository
        /// </summary>
        protected IRepository<Distressed> DistressedRepository => UnitOfWork.GetRepository<Distressed>();
        
        /// <summary>
        /// Concept idea repository
        /// </summary>
        protected IRepository<ConceptIdea> ConceptIdeaRepository => UnitOfWork.GetRepository<ConceptIdea>();

        /// <summary>
        /// Hospital claim repository
        /// </summary>
        protected IRepository<ClaimHospital> ClaimHospitalRepository => UnitOfWork.GetRepository<ClaimHospital>();

        /// <summary>
        /// COP fuel repository
        /// </summary>
        protected IRepository<CopFuel> CopFuelRepository => UnitOfWork.GetRepository<CopFuel>();

        /// <summary>
        /// DPA repository
        /// </summary>
        protected IRepository<Dpa> DpaRepository => UnitOfWork.GetRepository<Dpa>();

        /// <summary>
        /// DPA member repository
        /// </summary>
        protected IRepository<DpaMember> DpaMemberRepository => UnitOfWork.GetRepository<DpaMember>();

        /// <summary>
        /// Meal allowance repository
        /// </summary>
        protected IRepository<MealAllowance> MealAllowanceRepository => UnitOfWork.GetRepository<MealAllowance>();

        /// <summary>
        /// Ayo sekolah repository
        /// </summary>
        protected IRepository<AyoSekolah> AyoSekolahRepository => UnitOfWork.GetRepository<AyoSekolah>();

        /// <summary>
        /// BPKB request repository
        /// </summary>
        protected IRepository<BpkbRequest> BpkbRequestRepository => UnitOfWork.GetRepository<BpkbRequest>();

        /// <summary>
        /// Car purchase repository
        /// </summary>
        protected IRepository<CarPurchase> CarPurchaseRepository => UnitOfWork.GetRepository<CarPurchase>();

        /// <summary>
        /// Company loan repository
        /// </summary>
        protected IRepository<CompanyLoan> CompanyLoanRepository => UnitOfWork.GetRepository<CompanyLoan>();

        /// <summary>
        /// Company loan detail repository
        /// </summary>
        protected IRepository<CompanyLoanDetail> CompanyLoanDetailRepository => UnitOfWork.GetRepository<CompanyLoanDetail>();

        /// <summary>
        /// Company loan sequence repository
        /// </summary>
        protected IRepository<CompanyLoanSeq> CompanyLoanSeqRepository => UnitOfWork.GetRepository<CompanyLoanSeq>();

        /// <summary>
        /// Meal allowance shift repository
        /// </summary>
        protected IRepository<MealAllowanceShitf> MealAllowanceShitfRepository => UnitOfWork.GetRepository<MealAllowanceShitf>();

        /// <summary>
        /// Letter of guarantee repository
        /// </summary>
        protected IRepository<LetterGuarantee> LetterGuaranteeRepository => UnitOfWork.GetRepository<LetterGuarantee>();

        /// <summary>
        /// KB allowance repository
        /// </summary>
        protected IRepository<KBAllowance> KBAllowanceRepository => UnitOfWork.GetRepository<KBAllowance>();

        /// <summary>
        /// Personal data tax repository
        /// </summary>
        protected IRepository<PersonalDataTaxStatus> PersonalDataTaxStatusRepository => UnitOfWork.GetRepository<PersonalDataTaxStatus>();

        /// <summary>
        /// Actual organization structure repository
        /// </summary>
        protected IRepository<ActualOrganizationStructure> ActualOrganizationStructureRepository => UnitOfWork.GetRepository<ActualOrganizationStructure>();

        /// <summary>
        /// Personal data common attribute repository
        /// </summary>
        protected IRepository<PersonalDataCommonAttribute> PersonalDataCommonAttributeRepository => UnitOfWork.GetRepository<PersonalDataCommonAttribute>();

        /// <summary>
        /// Allowance detail repository
        /// </summary>
        protected IRepository<AllowanceDetail> AllowanceDetailRepository => UnitOfWork.GetRepository<AllowanceDetail>();

        /// <summary>
        /// Personal data repository
        /// </summary>
        protected IRepository<PersonalData> PersonalDataRepository => UnitOfWork.GetRepository<PersonalData>();

        /// <summary>
        /// Form repository
        /// </summary>
        protected IRepository<Form> FormRepository => UnitOfWork.GetRepository<Form>();

        /// <summary>
        /// Employee subgroup NP repository
        /// </summary>
        protected IRepository<EmployeeSubgroupNP> EmployeeSubgroupNPRepository => UnitOfWork.GetRepository<EmployeeSubgroupNP>();

        /// <summary>
        /// Vehicle repository
        /// </summary>
        protected IRepository<Vehicle> VehicleRepository => UnitOfWork.GetRepository<Vehicle>();

        /// <summary>
        /// Vehicle matrix repository
        /// </summary>
        protected IRepository<VehicleMatrix> VehicleMatrixRepository => UnitOfWork.GetRepository<VehicleMatrix>();

        /// <summary>
        /// Personal data bank account repository
        /// </summary>
        protected IRepository<PersonalDataBankAccount> PersonalDataBankAccountRepository => UnitOfWork.GetRepository<PersonalDataBankAccount>();

        /// <summary>
        /// Bank repository
        /// </summary>
        protected IRepository<Bank> BankRepository => UnitOfWork.GetRepository<Bank>();

        /// <summary>
        /// Form validation matrix repository
        /// </summary>
        protected IRepository<FormValidationMatrix> FormValidationMatrixRepository => UnitOfWork.GetRepository<FormValidationMatrix>();

        /// <summary>
        /// General category repository
        /// </summary>
        protected IRepository<GeneralCategory> GeneralCategoryRepository => UnitOfWork.GetRepository<GeneralCategory>();

        /// <summary>
        /// Config repository
        /// </summary>
        protected IRepository<Config> ConfigRepository => UnitOfWork.GetRepository<Config>();

        /// <summary>
        /// BPKB repository
        /// </summary>
        protected IRepository<Bpkb> BpkbRepository => UnitOfWork.GetRepository<Bpkb>();
        #endregion
        
        #region Private Properties
        /// <summary>
        /// Field that hold tag labels for caching
        /// </summary>
        private readonly string[] _tags = new[] { "menus", "roles", "permissions" };

        /// <summary>
        /// Field that hold localizer object
        /// </summary>
        private IStringLocalizer<IUnitOfWork> _localizer;
        #endregion

        #region Constructor
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="unitOfWork">Unit of Work Object</param>
        public ClaimBenefitService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        /// <summary>
        /// Default constructor with localizer object
        /// </summary>
        /// <param name="unitOfWork">Unit of Work Object</param>
        /// <param name="localizer">Localizer Object</param>
        public ClaimBenefitService(IUnitOfWork unitOfWork, IStringLocalizer<IUnitOfWork> localizer)
            : base(unitOfWork)
        {
            _localizer = localizer;
        }
        #endregion

        #region PTAAlloance Area
        /// <summary>
        /// Get list of Bank from cache async
        /// </summary>
        /// <param name="cache">Cache flag (if true get from cache).</param>
        /// <returns>List of Bank</returns>
        public IEnumerable<AllowanceDetail> GetAmmountPta(string type, bool cache = false)
        {
            var dbitems = AllowanceDetailRepository.Fetch().Where(x => x.Type == type && x.RowStatus);

            return dbitems;
        }

        public IQueryable<object> GetLoan(string type, string subtype,int np)
        {
            //var dbitems = AllowanceDetailRepository.Fetch().Where(x => x.Type == type && x.RowStatus && (kelas >= x.ClassFrom && kelas <= x.ClassTo));
            var data = from ad in AllowanceDetailRepository.Fetch()
                       where (np >= ad.ClassFrom && np <= ad.ClassTo) && ad.Type == type && ad.RowStatus && ad.SubType == subtype
                       select new
                       {
                           Ammount = ad.Ammount
                       };

            return data;
        }

        public IEnumerable<AllowanceDetail> GetTypePta(bool cache = false)
        {
            var myInClause = new string[] { "pta", "fta", "hrp" };
            var dbitems = AllowanceDetailRepository.Fetch().GroupBy(x => x.Type).Select(y => y.First()).Where(x => x.RowStatus).Where(s => myInClause.Contains(s.Type));

            return dbitems;
        }
        #endregion

        public IQueryable<object> GetInfo(string Noreg)
        { 
            var data = from aoe in ActualOrganizationStructureRepository.Fetch()
                       join np in EmployeeSubgroupNPRepository.Fetch()
                       on aoe.EmployeeSubgroup equals np.EmployeeSubgroup
                       where aoe.NoReg == Noreg && np.RowStatus
                       select new
                       {
                           Name = aoe.Name,
                           Kelas = aoe.EmployeeSubgroupText,
                           NP = np.NP,
                       };

            return data;
        }

        public IEnumerable<object> GetPdfInfoPersonal(string Noreg)
        {
            var data = from PD in PersonalDataRepository.Fetch()
                       join CA in PersonalDataCommonAttributeRepository.Fetch() on PD.CommonAttributeId equals CA.Id
                       join GC in GeneralCategoryRepository.Fetch() on CA.CityCode equals GC.Code into psgroup
                       from GC in psgroup.DefaultIfEmpty()
                       where PD.NoReg == Noreg
                       select new
                       {
                           CA.BirthDate,
                           KTP = CA.Nik ?? string.Empty,
                           Address = CA.Address ?? string.Empty,
                           RT = CA.Rt ?? string.Empty,
                           RW = CA.Rw ?? string.Empty,
                           City = GC != null ? GC.Name : string.Empty,
                           PostalCode = CA.PostalCode ?? string.Empty,
                           CA.StartDate,
                           Gender = CA.GenderCode ?? string.Empty
                       };

            return data;
        }

        public IEnumerable<object> GetOthers(string ConfigKey)
        {
            var data = from config in ConfigRepository.Fetch()
                       where config.ModuleCode == "others" && config.ConfigKey == ConfigKey
                       select new
                       {
                           ConfigValue = config.ConfigValue
                       };
            return data;
        }

        public List<PersonalDataBankViewModel> GetDataBank(string noreg, DateTime keyDate)
        {
            var aa = (from DB in PersonalDataBankAccountRepository.Fetch().AsNoTracking()
                      join B in BankRepository.Fetch().AsNoTracking() on DB.BankCode equals B.BankKey
                      join u in UserRepository.Fetch().AsNoTracking() on DB.NoReg equals u.NoReg
                      where (DB.NoReg == noreg) && keyDate >= DB.StartDate && keyDate <= DB.EndDate
                      select PersonalDataBankViewModel.CreateFrom(u, DB, B));

            return aa.ToList();

        }

        public IQueryable<object> GetMaxLens(int np)
        {
            var data = from ad in AllowanceDetailRepository.Fetch()
                       where (np >= ad.ClassFrom && np <= ad.ClassTo) && ad.Type == "EyewearAllowanceType" && ad.SubType == "lensa"
                       select new
                       {
                           AmmountTunjangan = ad.Ammount
                       };

            return data;
        }
        public IQueryable<object> GetMaxFrame(int np)
        {
            var data = from ad in AllowanceDetailRepository.Fetch()
                       where (np >= ad.ClassFrom && np <= ad.ClassTo) && ad.Type == "EyewearAllowanceType" && ad.SubType == "frame"
                       select new
                       {
                           AmmountTunjangan = ad.Ammount
                       };

            return data;
        }
        public List<General> GetLastEyeGlassesClaim(string noreg)
        {
            return ClaimGeneralRepository.Fetch().Where(x => x.NoReg == noreg && (x.AllowanceType == "frame" || x.AllowanceType == "lensa")).ToList();
        }

        public General GetLatestClaim(string noreg, string allowanceType)
        {
            return ClaimGeneralRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.NoReg == noreg && x.AllowanceType == allowanceType && x.RowStatus)
                .OrderByDescending(x => x.TransactionDate)
                .FirstOrDefault();
        }

        public void PreValidateMealAllowance(string formkey, string noreg)
        {
            var message = string.Empty;
            var formid = FormRepository.Fetch().Where(x => x.FormKey == formkey).FirstOrDefault().Id;
            var kelas = (from aoe in ActualOrganizationStructureRepository.Fetch()
                         join np in EmployeeSubgroupNPRepository.Fetch()
                         on aoe.EmployeeSubgroup equals np.EmployeeSubgroup
                         where aoe.NoReg == noreg                         
                         select new
                         {
                             NP = np.NP
                         }
                         ).FirstOrDefault();
            int kk = Convert.ToInt32(kelas.NP);

            var validate = (from vm in FormValidationMatrixRepository.Fetch()
                            join npfrom in EmployeeSubgroupNPRepository.Fetch()
                            on vm.FromClass equals npfrom.EmployeeSubgroup
                            join npto in EmployeeSubgroupNPRepository.Fetch()
                            on vm.ToClass equals npto.EmployeeSubgroup
                            where vm.FormId == formid && (kk >= npfrom.NP && kk <= npto.NP)
                            select vm.Id).Count();

            if (validate == 0)
                message = "Can not Create Request, Penggantian Uang Makan Dinas only for Class 3 - 6";

            if (!string.IsNullOrEmpty(message))
            {
                throw new Exception(message);
            }
        }

        public void PreValidateCpp(string noreg, string name)
        {
            var message = string.Empty;

            var dataMatrix = (from v in VehicleRepository.Fetch()
                              join vm in VehicleMatrixRepository.Fetch()
                              on v.Id equals vm.VehicleId
                              join a in ActualOrganizationStructureRepository.Fetch()
                              on vm.SequenceClass equals a.EmployeeSubgroup
                              where a.NoReg == noreg && v.CPP == true
                              orderby vm.Class
                              select vm.Class).Count();

            if (dataMatrix == 0)
            {
                message = $"User <b class=\"font-red\">{name}</b> is not in Class 7 - 8 Spv, cannot create request CPP Cash.";
            }
            //IViewLocalizer
            //message = IHtmlLocalizer["Reference Letter Request"].Value;


            //message = "cannot create request CPP Cash";

            if (!string.IsNullOrEmpty(message))
            {
                throw new Exception(message);
            }
        }

        public void PreValidateScp(string noreg, string name)
        {
            var message = string.Empty;

            var dataMatrix = (from v in VehicleRepository.Fetch()
                              join vm in VehicleMatrixRepository.Fetch()
                              on v.Id equals vm.VehicleId
                              join a in ActualOrganizationStructureRepository.Fetch()
                              on vm.SequenceClass equals a.EmployeeSubgroup
                              where a.NoReg == noreg && v.SCP == true
                              orderby vm.Class
                              select vm.Class).Count();

            if (dataMatrix == 0)
            {
                //message = $"User <b class=\"font-red\">{name}</b> is not in Class 7 - 8 Spv, cannot create request CPP Cash.";
                message = $"User <b class=\"font-red\">{name}</b> no vehicle to purchase for this class, cannot create request SCP.";

            }
            //IViewLocalizer
            //message = IHtmlLocalizer["Reference Letter Request"].Value;


            //message = "cannot create request SCP";

            if (!string.IsNullOrEmpty(message))
            {
                throw new Exception(message);
            }
        }

        public void PreValidatePostCpp(string noreg, DocumentRequestDetailViewModel<CppViewModel> requestDetailViewModel)
        {
            var message = string.Empty;
            //data approval matrix
            var validateMatirx = (from fv in FormValidationMatrixRepository.Fetch()
                                  join f in FormRepository.Fetch() on fv.FormId equals f.Id
                                  where f.FormKey == requestDetailViewModel.FormKey && fv.RequestType == "cpp"
                                  select fv).FirstOrDefault();
            int yearCop = validateMatirx.PeriodYear.HasValue ? validateMatirx.PeriodYear.Value : 0;
            int monthCop = validateMatirx.PeriodMonth.HasValue ? validateMatirx.PeriodMonth.Value : 0;

            //last cop complete
            var listPurchase = CarPurchaseRepository.Find(x => x.NoReg == noreg && x.CarPurchaseType == "CPP").ToList();
            if (listPurchase.Count() > 0)
            {
                DateTime ValidDatePurchase = listPurchase.Max(d => d.CreatedOn).AddMonths(monthCop).AddYears(yearCop);

                if (DateTime.Now < ValidDatePurchase)
                {
                    message = $"Cannot create request until " + ValidDatePurchase.ToShortDateString();
                }
            }

            if (!string.IsNullOrEmpty(message))
            {
                throw new Exception(message);
            }
        }

        public void PreValidatePostScp(string noreg, DocumentRequestDetailViewModel<ScpViewModel> requestDetailViewModel)
        {
            var message = string.Empty;
            //data approval matrix
            var validateMatirx = (from fv in FormValidationMatrixRepository.Fetch()
                                  join f in FormRepository.Fetch() on fv.FormId equals f.Id
                                  where f.FormKey == requestDetailViewModel.FormKey && fv.RequestType == "scp"
                                  select f).FirstOrDefault();

            //last cop complete
            var listPurchase = CarPurchaseRepository.Find(x => x.NoReg == noreg && x.CarPurchaseType == "SCP" && x.CreatedOn >= validateMatirx.StartDate.Value && x.CreatedOn <= validateMatirx.EndDate.Value).ToList();
            if (listPurchase.Count() > 0)
            {
                message = $"Cannot create request until " + validateMatirx.EndDate.Value.ToShortDateString();
            }

            if (!string.IsNullOrEmpty(message))
            {
                throw new Exception(message);
            }
        }

        //public void PreValidateClassCpp(string noreg)
        //{
        //    var message = string.Empty;

        //    var dataclass = (from aoe in ActualOrganizationStructureRepository.Fetch()
        //                     where aoe.NoReg == noreg
        //                     select aoe.EmployeeSubgroupText).FirstOrDefault();

        //    if (dataclass == "7" || dataclass == "08 Spv")
        //        message = "cannot create request CPP Cash";

        //    if (!string.IsNullOrEmpty(message))
        //    {
        //        throw new Exception(message);
        //    }
        //}

        public void PreValidateCop(string noreg, string name)
        {
            var message = string.Empty;

            var dataclass = (from v in VehicleRepository.Fetch()
                             join vm in VehicleMatrixRepository.Fetch()
                             on v.Id equals vm.VehicleId
                             join a in ActualOrganizationStructureRepository.Fetch()
                             on vm.SequenceClass equals a.EmployeeSubgroup
                             where a.NoReg == noreg && v.COP == true
                             orderby vm.Class
                             select vm.Class).Count();

            if (dataclass == 0) {
                message = $"User <b class=\"font-red\">{name}</b> is not in Class 8 AM above, cannot create request COP.";
            }

            //year validation 

            if (!string.IsNullOrEmpty(message))
            {
                throw new Exception(message);
            }
        }

        public void PreValidateCompanyLoan36(string noreg, string name)
        {
            var message = string.Empty;

            var data = (from aoe in ActualOrganizationStructureRepository.Fetch()
                        join np in EmployeeSubgroupNPRepository.Fetch()
                        on aoe.EmployeeSubgroup equals np.EmployeeSubgroup
                        where aoe.NoReg == noreg && np.RowStatus
                        orderby np.NP
                        select np.NP).FirstOrDefault();

            if (data >= 7 || data < 3)
            {
                message = $"User <b class=\"font-red\">{name}</b> is not in Between Class 3 or 6, cannot create request Company Loan.";
            }

            if (!string.IsNullOrEmpty(message))
            {
                throw new Exception(message);
            }
        }

        public void PreValidateCompanyLoan7Up(string noreg, string name)
        {
            var message = string.Empty;

            var data = (from aoe in ActualOrganizationStructureRepository.Fetch()
                        join np in EmployeeSubgroupNPRepository.Fetch()
                        on aoe.EmployeeSubgroup equals np.EmployeeSubgroup
                        where aoe.NoReg == noreg && np.RowStatus
                        orderby np.NP
                        select np.NP).FirstOrDefault();

            if (data < 7)
            {
                message = $"User <b class=\"font-red\">{name}</b> is not in Class 7 or higher, cannot create request Company Loan.";
            }

            if (!string.IsNullOrEmpty(message))
            {
                throw new Exception(message);
            }
        }

        public void PreValidateCopFuelAllowance(string noreg, string name)
        {
            var message = string.Empty;

            var dataCarPurchase = (from a in CarPurchaseRepository.Fetch()
                                   where a.NoReg == noreg && a.CarPurchaseType == "COP"
                                   select a.Id).Count();

            var dataGeneralClaim = (from a in ClaimGeneralRepository.Fetch()
                                   where a.NoReg == noreg && a.AllowanceType == "cop"
                                    select a.Id).Count();

            if (dataCarPurchase == 0 && dataGeneralClaim == 0)
            {
                message = $"Can not request Claim COP fuel, since <b class=\"font-red\">{name}</b> have never used COP Request";
                ////var aa = "Cannot create new request because it is not Period Active Ayo Sekolah.";
                //var aa = "Can not request Claim COP fuel, since";
                //message = aa + name;
            }


            //var dataBpkb = (from a in BpkbRepository.Fetch()
            //                where a.NoReg == noreg
            //                select a.Id).Count();

            //if (dataBpkb == 0)
            //{
            //    message = $"Can not request Claim COP fuel, since <b class=\"font-red\">{name}</b> have never used COP Request";
            //}

            if (!string.IsNullOrEmpty(message))
            {
                throw new Exception(message);
            }
        }

        public void PreValidatePostCop(string noreg, DocumentRequestDetailViewModel<CopViewModel> requestDetailViewModel)
        {
            var message = string.Empty;
            //data approval matrix
            var validateMatirx = (from fv in FormValidationMatrixRepository.Fetch()
                                 join f in FormRepository.Fetch() on fv.FormId equals f.Id
                                 where f.FormKey == requestDetailViewModel.FormKey && fv.RequestType == requestDetailViewModel.Object.SubmissionCode
                                 select fv).FirstOrDefault();
            int yearCop = validateMatirx.PeriodYear.HasValue ? validateMatirx.PeriodYear.Value : 0;
            int monthCop = validateMatirx.PeriodMonth.HasValue ? validateMatirx.PeriodMonth.Value : 0;

            //2022-02-14 | Roni | Hilangkan validasi sesuai request mas Faizal (2022-02-11)
            //last cop complete
            //var listPurchase = CarPurchaseRepository.Find(x => x.NoReg == noreg && x.CarPurchaseType == "COP").ToList();
            //if (listPurchase.Count() > 0)
            //{
            //    DateTime ValidDatePurchase = listPurchase.Max(d => d.CreatedOn).AddMonths(monthCop).AddYears(yearCop);

            //    if (DateTime.Now < ValidDatePurchase)
            //    {
            //        message = $"Cannot create request until " + ValidDatePurchase.ToShortDateString();
            //    }
            //}

            if (!string.IsNullOrEmpty(message))
            {
                throw new Exception(message);
            }
        }

        public void PreValidateMarriage(string noreg)
        {
            var message = string.Empty;

            var dataMarriageAllowance = from da in DocumentApprovalRepository.Fetch()
                                        join f in FormRepository.Fetch()
                                        on da.FormId equals f.Id
                                        where f.FormKey == "marriage-allowance" && da.CreatedBy == noreg && (da.DocumentStatusCode == "completed" || da.DocumentStatusCode == "inprogress")
                                        select da.Id;
            if (dataMarriageAllowance.Count() > 0)
                message = "Cant create request";

            if (!string.IsNullOrEmpty(message))
            {
                throw new Exception(message);
            }
        }

        public void PreValidateEyeglasess(string noreg)
        {
            var message = string.Empty;
            var now = DateTime.Now.Date;
            var objClaim = GetLastEyeGlassesClaim(noreg);
            var objFrame = objClaim.Where(x => x.AllowanceType == "frame").OrderByDescending(d => d.TransactionDate).FirstOrDefault();
            var objLensa = objClaim.Where(x => x.AllowanceType == "lensa").OrderByDescending(d => d.TransactionDate).FirstOrDefault();
            var currentCulture = System.Globalization.CultureInfo.CurrentUICulture;

            if (objFrame != null && objLensa != null)
            {
                var frameTransDate = objFrame.TransactionDate.AddYears(2).AddDays(-(objFrame.TransactionDate.Day - 1));
                var lensTransDate = objLensa.TransactionDate.AddYears(1).AddDays(-(objLensa.TransactionDate.Day - 1));

                if (frameTransDate > now && lensTransDate > now)
                {
                    message = _localizer["You can claim the lens again at"].Value + " " + objLensa.TransactionDate.AddYears(1).ToString("MMMM yyyy", currentCulture) + "</br>" +
                        _localizer["You can claim the frame again at"].Value + " " + objFrame.TransactionDate.AddYears(2).ToString("MMMM yyyy", currentCulture);
                }
            }

            if (!string.IsNullOrEmpty(message))
            {
                throw new Exception(message);
            }
        }
        
        public void PreValidateAyoSekolah (string noreg)
        {
            var message = string.Empty;
            var current = DateTime.Now;
                var now = current.Date.AddHours(current.Hour).AddMinutes(current.Minute);
                var form = FormRepository.Fetch()
                    .AsNoTracking()
                    .FirstOrDefault(x => x.FormKey == ApplicationForm.AyoSekolah);

                var startDate = form.StartDate ?? now.AddDays(-1);
                var endDate = form.EndDate ?? now.AddDays(1);
                var listStatus = new string[] { DocumentStatus.Draft, DocumentStatus.InProgress, DocumentStatus.Revised, DocumentStatus.Completed };

                var startDateTime = form.StartTime != null ? now.Date.Add(form.StartTime.Value) : now.Date;
                var endDateTime = form.EndTime != null ? now.Date.Add(form.EndTime.Value) : now.Date.AddDays(1).AddSeconds(-1);
            var isExist = DocumentApprovalRepository.Fetch()
                .Any(x => x.CreatedBy == noreg && x.FormId == form.Id && x.CreatedOn.Year == now.Year && (x.DocumentStatusCode == DocumentStatus.Draft
               || x.DocumentStatusCode == DocumentStatus.InProgress
               || x.DocumentStatusCode == DocumentStatus.Revised
               || x.DocumentStatusCode == DocumentStatus.Completed));



            if (isExist)
            {
                message = _localizer["Cannot create new request because Document is Already Created"].Value;
            }

            if (!string.IsNullOrEmpty(message))
            {
                throw new Exception(message);
            }
        }
        public void PreValidateCop(string noreg)
        {
            var message = string.Empty;
            //data
            var dataTax = (from da in DocumentApprovalRepository.Fetch()
                           join f in FormRepository.Fetch()
                           on da.FormId equals f.Id
                           where da.RowStatus && da.CreatedBy == noreg && (da.DocumentStatusCode == "draft" || da.DocumentStatusCode == "inprogress") &&
                           f.FormKey == "cop"
                           select da.Id).Count();

            if (dataTax > 0)
                message = _localizer["Your last COP request isn't complete yet"].Value;

            if (!string.IsNullOrEmpty(message))
            {
                throw new Exception(message);
            }
        }
        public void PreValidateCpp (string noreg)
        {
            var message = string.Empty;
            //data
            var dataTax = (from da in DocumentApprovalRepository.Fetch()
                           join f in FormRepository.Fetch()
                           on da.FormId equals f.Id
                           where da.RowStatus && da.CreatedBy == noreg && (da.DocumentStatusCode == "draft" || da.DocumentStatusCode == "inprogress") &&
                           f.FormKey == "cpp"
                           select da.Id).Count();

            if (dataTax > 0)
                message = _localizer["CPP Request must be completed before create request"].Value;

            if (!string.IsNullOrEmpty(message))
            {
                throw new Exception(message);
            }
        }

        public void PreValidateScp(string noreg)
        {
            var message = string.Empty;
            //data
            var dataTax = (from da in DocumentApprovalRepository.Fetch()
                           join f in FormRepository.Fetch()
                           on da.FormId equals f.Id
                           where da.RowStatus && da.CreatedBy == noreg && (da.DocumentStatusCode == "draft" || da.DocumentStatusCode == "inprogress") &&
                           f.FormKey == "scp"
                           select da.Id).Count();

            if (dataTax > 0)
                message = _localizer["SCP Request must be completed before create request"].Value;

            if (!string.IsNullOrEmpty(message))
            {
                throw new Exception(message);
            }
        }

        public void InsertAllowanceSeq(string noreg, LoanViewModel loanViewModel)
        {
            DateTime currPeriod = DateTime.Now;
            var dataSeq = CompanyLoanSeqRepository.Fetch().Where(x => x.PeriodYear == currPeriod.Year && x.PeriodMonth == currPeriod.Month);
            int currSequence = dataSeq.Count() == 0 ? 1 : dataSeq.Max(o => o.OrderSequence) + 1;
            var maxSeq = new ConfigService(UnitOfWork).GetConfig("MaxLoan.Sequence");

            while (currSequence > int.Parse(maxSeq.ConfigValue))
            {
                currPeriod = currPeriod.AddMonths(1);
                dataSeq = CompanyLoanSeqRepository.Fetch().Where(x => x.PeriodYear == currPeriod.Year && x.PeriodMonth == currPeriod.Month);
                currSequence = dataSeq.Count() == 0 ? 1 : dataSeq.Max(o => o.OrderSequence) + 1;
            }

            var dataAllowanceSeq = new CompanyLoanSeq()
            {
                Noreg = noreg,
                CompanyLoanId = null,
                PeriodYear = currPeriod.Year,
                PeriodMonth = currPeriod.Month,
                OrderSequence = currSequence,
                RequestStatus = "Pending"
            };

            CompanyLoanSeqRepository.Add(dataAllowanceSeq);
            UnitOfWork.SaveChanges();

            loanViewModel.Period = currPeriod;
            loanViewModel.Seq = currSequence;
        }

        public void InsertAllowanceSeq36(string noreg, Loan36ViewModel loanViewModel)
        {
            DateTime currPeriod = DateTime.Now;
            var dataSeq = CompanyLoanSeqRepository.Fetch().Where(x => x.PeriodYear == currPeriod.Year && x.PeriodMonth == currPeriod.Month);
            int currSequence = dataSeq.Count() == 0 ? 1 : dataSeq.Max(o => o.OrderSequence) + 1;
            var maxSeq = new ConfigService(UnitOfWork).GetConfig("MaxLoan.Sequence");

            while (currSequence > int.Parse(maxSeq.ConfigValue))
            {
                currPeriod = currPeriod.AddMonths(1);
                dataSeq = CompanyLoanSeqRepository.Fetch().Where(x => x.PeriodYear == currPeriod.Year && x.PeriodMonth == currPeriod.Month);
                currSequence = dataSeq.Count() == 0 ? 1 : dataSeq.Max(o => o.OrderSequence) + 1;
            }

            var dataAllowanceSeq = new CompanyLoanSeq()
            {
                Noreg = noreg,
                CompanyLoanId = null,
                PeriodYear = currPeriod.Year,
                PeriodMonth = currPeriod.Month,
                OrderSequence = currSequence,
                RequestStatus = "Pending"
            };

            CompanyLoanSeqRepository.Add(dataAllowanceSeq);
            UnitOfWork.SaveChanges();

            loanViewModel.Period = currPeriod;
            loanViewModel.Seq = currSequence;
        }

        public DpaRegisterViewModel GetLastDpa(string noreg)
        {
            //var data = (from da in DocumentApprovalRepository.Find(x => x.CreatedBy == noreg && x.DocumentStatusCode == "completed")
            //           join dd in DocumentRequestDetailRepository.Fetch() on da.Id equals dd.DocumentApprovalId
            //           join f in FormRepository.Fetch() on da.FormId equals f.Id
            //           where f.FormKey == "dpa-register"
            //           orderby da.CreatedOn
            //           select JsonConvert.DeserializeObject<DpaRegisterViewModel>(dd.ObjectValue)).FirstOrDefault();

            //return data;
            var objectValue = (from da in DocumentApprovalRepository.Fetch()
                               join dd in DocumentRequestDetailRepository.Fetch() on da.Id equals dd.DocumentApprovalId
                               join f in FormRepository.Fetch() on da.FormId equals f.Id
                               where da.CreatedBy == noreg &&
                                     da.DocumentStatusCode == "completed" &&
                                     f.FormKey == "dpa-register"
                               orderby da.CreatedOn descending
                               select dd.ObjectValue)
                      .Take(1)
                      .FirstOrDefault();

            return objectValue != null ? JsonConvert.DeserializeObject<DpaRegisterViewModel>(objectValue) : null;
        }

        public IQueryable<object> GetCutOff()
        {
            var data = from config in ConfigRepository.Fetch()
                       where config.ModuleCode == "claimbenefit" && config.ConfigKey == "Loan.CutOffDate"
                       select new
                       {
                           ConfigValue = config.ConfigValue
                       };
            return data;
        }
        public IQueryable<object> GetDateFullApproved( Guid id)
        {
            var data = from documentApproval in DocumentApprovalRepository.Fetch()
                       where documentApproval.Id == id 
                       select new
                       {
                           ModifiedOn = documentApproval.ModifiedOn
                       };
            return data;
        }

        public IQueryable<object> GetDescriptionLoan(String code)
        {
            var data = from generalCategory in GeneralCategoryRepository.Fetch()
                       where generalCategory.Code == code
                       select new
                       {
                           Description = generalCategory.Description
                       };
            return data;
        }


        #region RejectAction
        public void RejectLoanAllowance(string noregCurentApprover, DocumentApproval documentApproval)
        {
            DocumentRequestDetail documentRequestDetail = DocumentRequestDetailRepository.Fetch().Where(x => x.DocumentApprovalId == documentApproval.Id).FirstOrDefault();
            LoanViewModel obj = JsonConvert.DeserializeObject<LoanViewModel>(documentRequestDetail.ObjectValue);

            if (obj.Period.HasValue)
            {
                //remove sequence
                var dataSeq = CompanyLoanSeqRepository.Find(x => x.Noreg == documentApproval.CreatedBy &&
                                x.PeriodYear == obj.Period.Value.Year && x.PeriodMonth == obj.Period.Value.Month && x.OrderSequence == obj.Seq).FirstOrDefault();
                if (dataSeq != null)
                    CompanyLoanSeqRepository.Delete(dataSeq);

                //update Json period
                obj.Period = null;
                obj.Seq = 0;
                documentRequestDetail.ObjectValue = JsonConvert.SerializeObject(obj);
                DocumentRequestDetailRepository.Attach(documentRequestDetail);

                UnitOfWork.SaveChanges();
            }
        }

        public void RejectLoanAllowance36(string noregCurentApprover, DocumentApproval documentApproval)
        {
            DocumentRequestDetail documentRequestDetail = DocumentRequestDetailRepository.Fetch().Where(x => x.DocumentApprovalId == documentApproval.Id).FirstOrDefault();
            Loan36ViewModel obj = JsonConvert.DeserializeObject<Loan36ViewModel>(documentRequestDetail.ObjectValue);

            if (obj.Period.HasValue)
            {
                //remove sequence
                var dataSeq = CompanyLoanSeqRepository.Find(x => x.Noreg == documentApproval.CreatedBy &&
                                x.PeriodYear == obj.Period.Value.Year && x.PeriodMonth == obj.Period.Value.Month && x.OrderSequence == obj.Seq).FirstOrDefault();
                if (dataSeq != null)
                    CompanyLoanSeqRepository.Delete(dataSeq);

                //update Json period
                obj.Period = null;
                obj.Seq = 0;
                documentRequestDetail.ObjectValue = JsonConvert.SerializeObject(obj);
                DocumentRequestDetailRepository.Attach(documentRequestDetail);

                UnitOfWork.SaveChanges();
            }
        }
        #endregion

        #region CompleteAction
        public void CompleteEyeglassesAllowance(string noregCurentApprover, DocumentApproval documentApproval)
        {
            DocumentRequestDetail documentRequestDetail = DocumentRequestDetailRepository.Fetch().Where(x => x.DocumentApprovalId == documentApproval.Id).FirstOrDefault();
            EyeglassesAllowanceViewModel obj = JsonConvert.DeserializeObject<EyeglassesAllowanceViewModel>(documentRequestDetail.ObjectValue);

            //add
            if (obj.IsFrame)
            {
                General claimGeneral = new General()
                {
                    NoReg = documentApproval.CreatedBy,
                    AllowanceType = "frame",
                    Ammount = obj.AmountFrame,
                    TransactionDate = documentApproval.CreatedOn,
                    HardCopyStatus = true
                };
                ClaimGeneralRepository.Add(claimGeneral);
            }
            if (obj.IsLens)
            {
                General claimGeneral = new General()
                {
                    NoReg = documentApproval.CreatedBy,
                    AllowanceType = "lensa",
                    Ammount = obj.AmountLens,
                    TransactionDate = documentApproval.CreatedOn,
                    HardCopyStatus = true
                };
                ClaimGeneralRepository.Add(claimGeneral);
            }

            UnitOfWork.SaveChanges();
        }
        public void CompleteMarriageAllowance(string noregCurentApprover, DocumentApproval documentApproval)
        {
            DocumentRequestDetail documentRequestDetail = DocumentRequestDetailRepository.Fetch().Where(x => x.DocumentApprovalId == documentApproval.Id).FirstOrDefault();
            MarriageAllowanceViewModel obj = JsonConvert.DeserializeObject<MarriageAllowanceViewModel>(documentRequestDetail.ObjectValue);
                
            //add
            General General = new General()
            {
                NoReg = documentApproval.CreatedBy,
                AllowanceType = "marriageallowance",
                Ammount = new CoreService(UnitOfWork).GetAllowanceAmount(documentApproval.CreatedBy, "marriageallowance"),
                TransactionDate = obj.WeddingDate.Value,
                HardCopyStatus = true
            };

            ClaimGeneralRepository.Add(General);
            UnitOfWork.SaveChanges();
        }
        public void CompleteMisseryAllowance(string noregCurentApprover, DocumentApproval documentApproval)
        {
            var documentRequestDetail = DocumentRequestDetailRepository.Fetch().Where(x => x.DocumentApprovalId == documentApproval.Id).FirstOrDefault();
            var obj = JsonConvert.DeserializeObject<MisseryAllowanceViewModel>(documentRequestDetail.ObjectValue);
                
            decimal _ammount = 0;
            var data = from aoe in ActualOrganizationStructureRepository.Fetch().AsNoTracking()
                       join np in EmployeeSubgroupNPRepository.Fetch().AsNoTracking()
                       on aoe.EmployeeSubgroup equals np.EmployeeSubgroup
                       where aoe.NoReg == documentApproval.CreatedBy && np.RowStatus
                       select new
                       {
                           Name = aoe.Name,
                           Kelas = aoe.EmployeeSubgroupText,
                           NP = np.NP,
                       };

            if (data.FirstOrDefault().NP < 3 || data.FirstOrDefault().NP > 6)
            {
                _ammount = new CoreService(UnitOfWork).GetAllowanceAmount(documentApproval.CreatedBy, "miseryallowance", obj.IsMainFamily == "ya" ? "MainFamily" : "NonMainFamily");
            }
                
            var claimGeneral = new General()
            {
                NoReg = documentApproval.CreatedBy,
                AllowanceType = "miseryallowance",
                Ammount = _ammount,
                TransactionDate = obj.MisseryDate.Value,
                HardCopyStatus = false
            };

            ClaimGeneralRepository.Add(claimGeneral);
            UnitOfWork.SaveChanges();
        }

        public void UpdateVacationReport(VacationAllowanceViewModel vacationAllowance)
        {
            var documentApproval = DocumentApprovalRepository.Fetch()
                .FirstOrDefault(x => x.Id == vacationAllowance.DocumentApprovalId);

            var documentRequestDetail = DocumentRequestDetailRepository.Fetch()
                .FirstOrDefault(x => x.DocumentApprovalId == vacationAllowance.DocumentApprovalId);

            var obj = JsonConvert.DeserializeObject<VacationAllowanceViewModel>(documentRequestDetail.ObjectValue);

            obj.LocationActual = vacationAllowance.LocationActual;
            obj.VacationDateActual = vacationAllowance.VacationDateActual;
            obj.TotalParticipant = vacationAllowance.TotalParticipant;
            obj.AttachmentFilePath = vacationAllowance.AttachmentFilePath;
            documentRequestDetail.ObjectValue = JsonConvert.SerializeObject(obj);

            UnitOfWork.SaveChanges();
        }

        public void UpdateReimbursementPoint(ReimbursementPointViewModel point)
        {
            var documentApproval = DocumentApprovalRepository.Fetch()
                .FirstOrDefault(x => x.Id == point.DocumentApprovalId);

            Assert.ThrowIf(documentApproval == null, "Document is not found");
            Assert.ThrowIf(documentApproval.DocumentStatusCode != DocumentStatus.InProgress, "Cant update point, document status is not valid");

            var documentRequestDetail = DocumentRequestDetailRepository.Fetch()
                .FirstOrDefault(x => x.DocumentApprovalId == point.DocumentApprovalId);

            var obj = JsonConvert.DeserializeObject<ReimbursementViewModel>(documentRequestDetail.ObjectValue);

            obj.TotalClaim = point.TotalClaim;
            obj.TotalCompanyClaim = point.TotalCompanyClaim;

            documentApproval.CanSubmit = true;
            documentRequestDetail.ObjectValue = JsonConvert.SerializeObject(obj);

            UnitOfWork.SaveChanges();
        }
        public void ApprovalVacationAllowance(string noregCurentApprover, DocumentApproval documentApproval)
        {
            //2022-11-24 menambahkan validasi jika tracking approval sudah di approve payroll HR maka jalankan script ini
            var formData = FormRepository.Fetch().Where(x => x.FormKey == "vacation-allowance").FirstOrDefault();
            if(formData != null)
            {
                var approverHRAdmin = new ConfigService(UnitOfWork).GetConfig("VacationAllowance.HRAdminRole");
                var approvalMatrix = ApprovalMatrixRepository.Fetch().Where(x => x.FormId == formData.Id && x.Approver == approverHRAdmin.ConfigValue).FirstOrDefault();
                if (approvalMatrix != null)
                {
                    var documentTrackingApproval = TrackingApprovalRepository.Fetch()
                        .Where(x => x.DocumentApprovalId == documentApproval.Id && x.ApprovalMatrixId==approvalMatrix.Id 
                            && x.NoReg == noregCurentApprover)
                        .FirstOrDefault();

                    if (documentTrackingApproval != null)
                    {
                        var documentRequestDetail = DocumentRequestDetailRepository.Fetch().Where(x => x.DocumentApprovalId == documentApproval.Id).FirstOrDefault();
                        var obj = JsonConvert.DeserializeObject<VacationAllowanceViewModel>(documentRequestDetail.ObjectValue);
                        var personalDataBank = new PersonalDataBankAccount();

                        var checkRecreationReward = RecreationRewardRepository.Fetch().Where(x => x.NoReg == documentApproval.CreatedBy && x.EventDate == obj.VacationDate).FirstOrDefault();

                        if (checkRecreationReward == null)
                        {
                            if (string.IsNullOrEmpty(obj.AccountMore))
                            {
                                personalDataBank = PersonalDataBankAccountRepository.Find(x =>
                                    x.NoReg == documentApproval.CreatedBy &&
                                    x.RowStatus &&
                                    DateTime.Now >= x.StartDate &&
                                    DateTime.Now <= x.EndDate
                                ).FirstOrDefault();
                            }

                            var dataBank = BankRepository.Find(x => x.BankKey == personalDataBank.BankCode).FirstOrDefault();
                            var actualOrganizationStructure = new MdmService(UnitOfWork).GetActualOrganizationStructure(documentApproval.CreatedBy);

                            var recreationReward = new RecreationReward()
                            {
                                NoReg = documentApproval.CreatedBy,
                                BenefitType = "recreation",
                                Bank = string.IsNullOrEmpty(obj.AccountMore) ? dataBank.BankKey : obj.BankCode,
                                BankAccountNo = string.IsNullOrEmpty(obj.AccountMore) ? personalDataBank.AccountNumber : obj.AccountNumber,
                                BankAccountName = string.IsNullOrEmpty(obj.AccountMore) ? (personalDataBank.AccountName ?? actualOrganizationStructure.Name) : obj.AccountName,
                                EventDate = obj.VacationDate.Value,
                                Location = obj.Location,
                                TotalAmount = decimal.Parse(obj.Summaries.Sum(x => x.Total).ToString())
                            };

                            RecreationRewardRepository.Add(recreationReward);
                            UnitOfWork.SaveChanges();

                            foreach (var dept in obj.Departments)
                            {
                                if(dept.Employees == null || dept.Employees.Count() == 0)
                                {
                                    continue;
                                }  
                                foreach (var emp in dept.Employees)
                                {
                                    if (emp.HasVacation) { continue; }

                                    var recreationRewardMember = new RecreationRewardMember()
                                    {
                                        RecreationRewardId = recreationReward.Id,
                                        NoReg = emp.NoReg,
                                        BenefitSubType = "",
                                        Amount = new CoreService(UnitOfWork).GetAllowanceAmount(emp.NoReg, "recreation"),
                                        StartDate = DateTime.Now,
                                        EndDate = DateTime.Now
                                    };

                                    RecreationRewardMemberRepository.Add(recreationRewardMember);
                                }
                            }

                            UnitOfWork.SaveChanges();
                        }
                        
                    }
                }
            }
            
            
        }

        public void CompleteVacationAllowance(string noregCurentApprover, DocumentApproval documentApproval)
        {
            
            var documentRequestDetail = DocumentRequestDetailRepository.Fetch().Where(x => x.DocumentApprovalId == documentApproval.Id).FirstOrDefault();
            var obj = JsonConvert.DeserializeObject<VacationAllowanceViewModel>(documentRequestDetail.ObjectValue);
            
            var set = UnitOfWork.GetRepository<RecreationReward>();
            set.Fetch().Where(x => x.NoReg == documentApproval.CreatedBy && x.EventDate == obj.VacationDate)
                .Update(x => new RecreationReward
                {
                    DocumentApprovalId = documentApproval.Id
                });

            UnitOfWork.SaveChanges();

        }

        public void ResetClaimVacationAllowance(DocumentApproval documentApproval)
        {
            var formData = FormRepository.Fetch().Where(x => x.FormKey == "vacation-allowance").FirstOrDefault();
            if (formData != null)
            {
                var documentRequestDetail = DocumentRequestDetailRepository.Fetch().Where(x => x.DocumentApprovalId == documentApproval.Id).FirstOrDefault();
                var obj = JsonConvert.DeserializeObject<VacationAllowanceViewModel>(documentRequestDetail.ObjectValue);

                var checkRecreationReward = RecreationRewardRepository.Fetch().Where(x => x.NoReg == documentApproval.CreatedBy && x.EventDate == obj.VacationDate).FirstOrDefault();

                if(checkRecreationReward != null)
                {
                    var listDeleteMember = RecreationRewardMemberRepository.Fetch().Where(x => x.RecreationRewardId == checkRecreationReward.Id).ToList();
                    foreach(var member in listDeleteMember)
                    {
                        RecreationRewardMemberRepository.DeleteById(member.Id);
                    }

                    RecreationRewardRepository.DeleteById(checkRecreationReward.Id);

                    UnitOfWork.SaveChanges();
                }
            }
        }
        public void CompleteDistressedAllowance(string noregCurentApprover, DocumentApproval documentApproval)
        {
            DocumentRequestDetail documentRequestDetail = DocumentRequestDetailRepository.Fetch().Where(x => x.DocumentApprovalId == documentApproval.Id).FirstOrDefault();
            DistressedAllowanceViewModel obj = JsonConvert.DeserializeObject<DistressedAllowanceViewModel>(documentRequestDetail.ObjectValue);

            //add
            Distressed distressed = new Distressed()
            {
                NoReg = documentApproval.CreatedBy,
                Ammount = new CoreService(UnitOfWork).GetAllowanceAmount(documentApproval.CreatedBy, "distressedallownce"),
                Description = obj.Description,
                TransactionDate = obj.DateDistressed.Value
            };

            DistressedRepository.Add(distressed);
            UnitOfWork.SaveChanges();
        }
        
        public void CompletePtaAllowance(string noregCurentApprover, DocumentApproval documentApproval)
        {
            var documentRequestDetail = DocumentRequestDetailRepository.Fetch().Where(x => x.DocumentApprovalId == documentApproval.Id).FirstOrDefault();
            var obj = JsonConvert.DeserializeObject<PtaAllowanceViewModel>(documentRequestDetail.ObjectValue);
            var personalDataBank = new PersonalDataBankAccount();

            if (obj.AccountType == "rekening")
            {
                personalDataBank = PersonalDataBankAccountRepository.Find(x =>
                    x.NoReg == documentApproval.CreatedBy &&
                    x.RowStatus &&
                    DateTime.Now >= x.StartDate &&
                    DateTime.Now <= x.EndDate
                ).FirstOrDefault();
            }

            var dataBank = BankRepository.Find(x => x.BankKey == personalDataBank.BankCode).FirstOrDefault();
            var actualOrganizationStructure = new MdmService(UnitOfWork).GetActualOrganizationStructure(documentApproval.CreatedBy);

            foreach (var summary in obj.Summaries)
            {
                var recreationReward = new RecreationReward()
                {
                    NoReg = documentApproval.CreatedBy,
                    BenefitType = summary.Reward,
                    Bank = obj.AccountType == "rekening" ? dataBank.BankKey : obj.BankCode,
                    BankAccountNo = obj.AccountType == "rekening" ? personalDataBank.AccountNumber : obj.AccountNumber,
                    BankAccountName = obj.AccountType == "rekening" ? (personalDataBank.AccountName ?? actualOrganizationStructure.Name) : obj.AccountName,
                    EventDate = summary.EventDate,
                    TotalAmount = Convert.ToDecimal(summary.Total)
                };

                RecreationRewardRepository.Add(recreationReward);

                foreach (var emp in summary.Employees)
                {
                    var recreationRewardMember = new RecreationRewardMember()
                    {
                        RecreationRewardId = recreationReward.Id,
                        NoReg = emp.NoReg,
                        BenefitSubType = "",
                        Amount = (decimal)summary.Amount,
                        StartDate = DateTime.Now,
                        EndDate = DateTime.Now
                    };

                    RecreationRewardMemberRepository.Add(recreationRewardMember);
                }
            }

            UnitOfWork.SaveChanges();
        }
        public void CompleteIdeBerkonsepAllowance(string noregCurentApprover, DocumentApproval documentApproval)
        {
            var documentRequestDetail = DocumentRequestDetailRepository.Fetch().Where(x => x.DocumentApprovalId == documentApproval.Id).FirstOrDefault();
            var obj = JsonConvert.DeserializeObject<IdeBerkonsepViewModel>(documentRequestDetail.ObjectValue);

            var ConceptIdea = new ConceptIdea()
            {
                NoReg = documentApproval.CreatedBy,
                Criteria = obj.CriretiaCode,
                Title = obj.Title,
                Point = obj.Value,
                Ammount = obj.Amount
            };

            ConceptIdeaRepository.Add(ConceptIdea);

            UnitOfWork.SaveChanges();
        }

        public void CompleteRSAllowance(string noregCurentApprover, DocumentApproval documentApproval)
        {
            var documentRequestDetail = DocumentRequestDetailRepository.Fetch().Where(x => x.DocumentApprovalId == documentApproval.Id).FirstOrDefault();
            var obj = JsonConvert.DeserializeObject<ReimbursementViewModel>(documentRequestDetail.ObjectValue);

            var creator = documentApproval.CreatedBy;
            var keyDate = DateTime.Now;
            var bank = GetDataBank(creator, keyDate.Date).FirstOrDefault();

            if (bank != null)
            {
                obj.AccountName = bank.AccountName ?? bank.Name;
                obj.AccountNumber = bank.AccountNumber;
                obj.BankCode = bank.BankName;
            }

            var claimHospital = new ClaimHospital()
            {
                NoReg = documentApproval.CreatedBy,
                FamilyMemberType = obj.FamilyRelationship,
                PatientName = obj.FamilyRelationship == "rsanak" ? obj.PatientChildName : obj.PatientName,
                HospitalName = obj.IsOtherHospital ? obj.OtherHospital : obj.Hospital,
                HospitalAddress = obj.HospitalAddress,
                DateIn = obj.DateOfEntry.Value,
                DateOut = obj.DateOfOut.Value,
                ReimburshmentType = obj.InPatient,
                Cost = obj.Cost,
                InsuranceAmmount = obj.TotalClaim,
                Ammount = obj.TotalCompanyClaim,
                AccountName = obj.AccountName,
                AccountNumber = obj.AccountNumber,
                BankName = obj.BankCode,
                PhoneNumber = obj.PhoneNumber
            };

            ClaimHospitalRepository.Add(claimHospital);

            UnitOfWork.SaveChanges();
        }
        public void CompleteCopFuelAllowance(string noregCurentApprover, DocumentApproval documentApproval)
        {
            var documentRequestDetail = DocumentRequestDetailRepository.Fetch().Where(x => x.DocumentApprovalId == documentApproval.Id).FirstOrDefault();
            var obj = JsonConvert.DeserializeObject<CopFuelAllowanceViewModel>(documentRequestDetail.ObjectValue);

            foreach (var item in obj.data)
            {
                CopFuel copFuel = new CopFuel()
                {
                    NoReg = documentApproval.CreatedBy,
                    COPFuelDate = item.Date.Value,
                    Purpose = item.Necessity,
                    DestinationStart = item.Destination,
                    KMStart = item.Start,
                    DestinationEnd = item.Destination,
                    KMEnd = item.Back,
                    KMTotal = item.Back - item.Start
                };

                CopFuelRepository.Add(copFuel);
            }

            UnitOfWork.SaveChanges();
        }

        public void CompleteCOPAllowance(string noregCurentApprover, DocumentApproval documentApproval)
        {
            DocumentRequestDetail documentRequestDetail = DocumentRequestDetailRepository.Fetch().Where(x => x.DocumentApprovalId == documentApproval.Id).FirstOrDefault();
            CopViewModel obj = JsonConvert.DeserializeObject<CopViewModel>(documentRequestDetail.ObjectValue);

            //add
            CarPurchase carPurchase = new CarPurchase()
            {
                NoReg = documentApproval.CreatedBy,
                CarPurchaseType = "COP",
                PurschaseType = obj.SubmissionCode,
                CarModel = obj.Model,
                CarType = obj.TypeName,
                CarColor = obj.ColorCode,
                NIK = "",
                Ammount = 0,
                DTRRN = obj.DTRRN,
                DTMOCD = obj.DTMOCD,
                DTMOSX = obj.DTMOSX,
                DTEXTC = obj.DTEXTC,
                DTPLOD = obj.DTPLOD,
                DTFRNO = obj.DTFRNO,
                Dealer = obj.Dealer,
                DODate = obj.DoDate,
                StnkDate = obj.StnkDate,
                ServiceFee = obj.Jasa,
                PaymentMethod = obj.PaymentMethod,
            };

            CarPurchaseRepository.Add(carPurchase);
            
            UnitOfWork.SaveChanges();
        }

        public void CompleteCOPSTNKReady(string noregRequester, CopViewModel copViewModel)
        {
            Bpkb dataBpkb = new Bpkb()
            {
                NoReg = noregRequester,
                NoBPKB = "",
                LicensePlat = copViewModel.LisencePlat,
                Type = copViewModel.TypeName.Split('-')[0],
                Model = copViewModel.Model,
                CreatedYear = "",
                Color = copViewModel.DataUnitColorName,
                VINNo = copViewModel.DTFRNO,
                EngineNo = copViewModel.Engine,
                VehicleOwner = "",
                Address = "",
            };

            BpkbRepository.Add(dataBpkb);

            UnitOfWork.SaveChanges();
        }

        public void CompleteCPPAllowance(string noregCurentApprover, DocumentApproval documentApproval)
        {
            DocumentRequestDetail documentRequestDetail = DocumentRequestDetailRepository.Fetch().Where(x => x.DocumentApprovalId == documentApproval.Id).FirstOrDefault();
            CppViewModel obj = JsonConvert.DeserializeObject<CppViewModel>(documentRequestDetail.ObjectValue);

            //add
            CarPurchase carPurchase = new CarPurchase()
            {
                NoReg = documentApproval.CreatedBy,
                CarPurchaseType = "CPP",
                PurschaseType = obj.PurchaseTypeCode,
                CarModel = obj.Model,
                CarType = obj.TypeName,
                CarColor = obj.ColorCode,
                NIK = obj.PopulationNumber,
                Ammount = 0,
                DTRRN = obj.DTRRN,
                DTMOCD = obj.DTMOCD,
                DTMOSX = obj.DTMOSX,
                DTEXTC = obj.DTEXTC,
                DTPLOD = obj.DTPLOD,
                DTFRNO = obj.DTFRNO,
                Dealer = obj.Dealer,
                DODate = obj.DoDate,
                StnkDate = obj.StnkDate,
                ServiceFee = obj.Jasa,
                PaymentMethod = obj.PaymentMethod,
            };
            
            CarPurchaseRepository.Add(carPurchase);

            UnitOfWork.SaveChanges();
        }

        public void CompleteSCPAllowance(string noregCurentApprover, DocumentApproval documentApproval)
        {
            DocumentRequestDetail documentRequestDetail = DocumentRequestDetailRepository.Fetch().Where(x => x.DocumentApprovalId == documentApproval.Id).FirstOrDefault();
            ScpViewModel obj = JsonConvert.DeserializeObject<ScpViewModel>(documentRequestDetail.ObjectValue);

            //add
            CarPurchase carPurchase = new CarPurchase()
            {
                NoReg = documentApproval.CreatedBy,
                CarPurchaseType = "SCP",
                PurschaseType = obj.PurchaseTypeCode,
                CarModel = obj.Model,
                CarType = obj.TypeName,
                CarColor = obj.ColorCode,
                NIK = obj.PopulationNumber,
                Ammount = 0,
                DTRRN = obj.DTRRN,
                DTMOCD = obj.DTMOCD,
                DTMOSX = obj.DTMOSX,
                DTEXTC = obj.DTEXTC,
                DTPLOD = obj.DTPLOD,
                DTFRNO = obj.DTFRNO,
                Dealer = obj.Dealer,
                DODate = obj.DoDate,
                StnkDate = obj.StnkDate,
                ServiceFee = obj.Jasa,
                PaymentMethod = obj.PaymentMethod,
            };

            CarPurchaseRepository.Add(carPurchase);

            UnitOfWork.SaveChanges();
        }

        public void CompleteLoanAllowance(string noregCurentApprover, DocumentApproval documentApproval)
        {
            DocumentRequestDetail documentRequestDetail = DocumentRequestDetailRepository.Fetch().Where(x => x.DocumentApprovalId == documentApproval.Id).FirstOrDefault();
            LoanViewModel obj = JsonConvert.DeserializeObject<LoanViewModel>(documentRequestDetail.ObjectValue);

            //add
            CompanyLoan companyLoan = new CompanyLoan()
            {
                NoReg = documentApproval.CreatedBy,
                LoanType = obj.LoanType,
                AmmountTotal = obj.LoanAmount.Value,
                AmmountLoan = obj.CostNeeds.Value,
                TotalInstallment = obj.LoanPeriod,
                AmmountInterest = ((obj.Interest / 100) * (obj.LoanPeriod / 12) ) * obj.CostNeeds.Value,
                PercentInterest = obj.Interest
            };
            CompanyLoanRepository.Add(companyLoan);
            UnitOfWork.SaveChanges();
            CompanyLoanDetail companyLoanDetail = new CompanyLoanDetail()
            {
                CompanyLoadId = companyLoan.Id,
                Province = obj.Province,
                City = obj.City,
                District = obj.District,
                PostCode = obj.PostalCode,
                Address = obj.Address,
                RT = obj.RT,
                RW = obj.RW,
                Brand = obj.Brand,
                Description = ""
            };
            CompanyLoanDetailRepository.Add(companyLoanDetail);
            UnitOfWork.SaveChanges();

            var dataSeq = CompanyLoanSeqRepository.Find(x => x.Noreg == documentApproval.CreatedBy && x.PeriodYear == obj.Period.Value.Year && x.PeriodMonth == obj.Period.Value.Month && x.OrderSequence == obj.Seq).FirstOrDefault();

            dataSeq.CompanyLoanId = companyLoan.Id;
            dataSeq.RequestStatus = "Approve";

            UnitOfWork.SaveChanges();
        }

        public void CompleteLoanAllowance36(string noregCurentApprover, DocumentApproval documentApproval)
        {
            DocumentRequestDetail documentRequestDetail = DocumentRequestDetailRepository.Fetch().Where(x => x.DocumentApprovalId == documentApproval.Id).FirstOrDefault();
            Loan36ViewModel obj = JsonConvert.DeserializeObject<Loan36ViewModel>(documentRequestDetail.ObjectValue);

            //add
            CompanyLoan companyLoan = new CompanyLoan()
            {
                NoReg = documentApproval.CreatedBy,
                LoanType = obj.LoanType,
                AmmountTotal = obj.LoanAmount.Value,
                AmmountLoan = obj.CostNeeds.Value,
                TotalInstallment = obj.LoanPeriod,
                AmmountInterest = ((obj.Interest / 100) * (obj.LoanPeriod / 12)) * obj.CostNeeds.Value,
                PercentInterest = obj.Interest
            };
            CompanyLoanRepository.Add(companyLoan);
            UnitOfWork.SaveChanges();
            CompanyLoanDetail companyLoanDetail = new CompanyLoanDetail()
            {
                CompanyLoadId = companyLoan.Id,
                Province = obj.Province,
                City = obj.City,
                District = obj.District,
                PostCode = obj.PostalCode,
                Address = obj.Address,
                RT = obj.RT,
                RW = obj.RW,
                Brand = obj.Brand,
                Description = ""
            };
            CompanyLoanDetailRepository.Add(companyLoanDetail);
            UnitOfWork.SaveChanges();

            var dataSeq = CompanyLoanSeqRepository.Find(x => x.Noreg == documentApproval.CreatedBy && x.PeriodYear == obj.Period.Value.Year && x.PeriodMonth == obj.Period.Value.Month && x.OrderSequence == obj.Seq).FirstOrDefault();

            dataSeq.CompanyLoanId = companyLoan.Id;
            dataSeq.RequestStatus = "Approve";

            UnitOfWork.SaveChanges();
        }

        public void CompleteDpaRegister(string noregCurentApprover, DocumentApproval documentApproval)
        {
            DocumentRequestDetail documentRequestDetail = DocumentRequestDetailRepository.Fetch().Where(x => x.DocumentApprovalId == documentApproval.Id).FirstOrDefault();
            DpaRegisterViewModel obj = JsonConvert.DeserializeObject<DpaRegisterViewModel>(documentRequestDetail.ObjectValue);
            PersonalDataBankAccount personalDataBank = new PersonalDataBankAccount();
            Bank dataBank = new Bank();
            if (obj.AccountType == "rekening")
            {
                personalDataBank = PersonalDataBankAccountRepository.Find(x =>
                    x.NoReg == documentApproval.CreatedBy &&
                    x.RowStatus &&
                    DateTime.Now >= x.StartDate &&
                    DateTime.Now <= x.EndDate
                ).FirstOrDefault();
                dataBank = BankRepository.Find(x => x.BankKey == personalDataBank.BankCode).FirstOrDefault();
            }
            else
                dataBank = BankRepository.Find(x => x.BankKey == obj.BankCode).FirstOrDefault();

            ActualOrganizationStructure actualOrganizationStructure = new MdmService(UnitOfWork).GetActualOrganizationStructure(documentApproval.CreatedBy);
            //add
            Dpa dpa = DpaRepository.Fetch().Where(x => x.NoReg == documentApproval.CreatedBy).FirstOrDefault();
            if (dpa != null)
            {
                dpa.NoReg = documentApproval.CreatedBy;
                dpa.BankName = dataBank.BankName;
                dpa.BankBranch = obj.AccountType == "rekening" ? dataBank.Branch : obj.Branch;
                dpa.AccountNumber = obj.AccountType == "rekening" ? personalDataBank.AccountNumber : obj.AccountNumber;
                dpa.AccountName = obj.AccountType == "rekening" ? actualOrganizationStructure.Name : obj.AccountName;
                dpa.PersonalEmail = obj.Email;
                dpa.TelephoneNo = obj.HouseNumber.ToString();
                dpa.MobileNo = obj.MobilePhoneNumber.ToString();
                DpaRepository.Attach(dpa);
            }
            else
            {
                dpa = new Dpa()
                {
                    NoReg = documentApproval.CreatedBy,
                    BankName = dataBank.BankName,
                    BankBranch = obj.AccountType == "rekening" ? dataBank.Branch : obj.Branch,
                    AccountNumber = obj.AccountType == "rekening" ? personalDataBank.AccountNumber : obj.AccountNumber,
                    AccountName = obj.AccountType == "rekening" ? actualOrganizationStructure.Name : obj.AccountName,
                    PersonalEmail = obj.Email,
                    TelephoneNo = obj.HouseNumber.ToString(),
                    MobileNo = obj.MobilePhoneNumber.ToString()
                };
                DpaRepository.Add(dpa);
            }


            var listDelete = DpaMemberRepository.Fetch().Where(x => x.NoReg == documentApproval.CreatedBy);
            foreach (var item in listDelete)
            {
                DpaMemberRepository.Delete(item);
            }
            UnitOfWork.SaveChanges();

            foreach (var item in obj.AhliWaris)
            {
                DpaMember dpaMember = new DpaMember()
                {
                    NoReg = documentApproval.CreatedBy,
                    FamilyType = item.FamilyRelation,
                    BirthDate = item.BrithDate.Value,
                    Name = item.Name,
                    IsActive = true
                };
                DpaMemberRepository.Add(dpaMember);
            }
                
            UnitOfWork.SaveChanges();
        }

        public void CompleteDpaChange(string noregCurentApprover, DocumentApproval documentApproval)
        {
            DocumentRequestDetail documentRequestDetail = DocumentRequestDetailRepository.Fetch().Where(x => x.DocumentApprovalId == documentApproval.Id).FirstOrDefault();
            DpaChangeViewModel obj = JsonConvert.DeserializeObject<DpaChangeViewModel>(documentRequestDetail.ObjectValue);

            //add
            Dpa dpa = new Dpa()
            {
                NoReg = documentApproval.CreatedBy,
                BankName = obj.BankCode,
                BankBranch = obj.Branch,
                AccountNumber = obj.AccountNumber,
                AccountName = obj.AccountName,
                PersonalEmail = obj.Email,
                TelephoneNo = obj.MobilePhoneNumber
            };
            
            DpaRepository.Add(dpa);

            foreach (var item in obj.AhliWaris)
            {
                DpaMember dpaMember = new DpaMember()
                {
                    NoReg = documentApproval.CreatedBy,
                    FamilyType = item.FamilyRelation,
                    BirthDate = item.BrithDate.Value,
                    Name = item.Name,
                    IsActive = true
                };
                DpaMemberRepository.Add(dpaMember);
            }

            UnitOfWork.SaveChanges();
        }

        public void CompleteMealAllowanceShift(string noregCurentApprover, DocumentApproval documentApproval)
        {
            DocumentRequestDetail documentRequestDetail = DocumentRequestDetailRepository.Fetch().Where(x => x.DocumentApprovalId == documentApproval.Id).FirstOrDefault();
            ShiftMealAllowanceViewModel obj = JsonConvert.DeserializeObject<ShiftMealAllowanceViewModel>(documentRequestDetail.ObjectValue);

            //add
            foreach (var summary in obj.Summaries)
            {
                if (summary.Employees != null)
                {
                    foreach (var emp in summary.Employees)
                    {
                        MealAllowanceShitf mealAllowanceShitf = new MealAllowanceShitf()
                        {
                            NoReg = emp.NoReg,
                            PeriodYear = int.Parse(obj.StartPeriod),
                            PeriodMonth = int.Parse(obj.EndPeriod),
                            ShiftDate = summary.Date,
                            ShiftCode = emp.ShiftCode,
                            Ammount = emp.Amount
                        };

                        MealAllowanceShitfRepository.Add(mealAllowanceShitf);
                    }
                }
            }

            //MealAllowanceShitf mealAllowanceShitf = new MealAllowanceShitf()
            //{
            //    NoReg = documentApproval.CreatedBy,
            //    PeriodYear = int.Parse(obj.StartPeriod),
            //    PeriodMonth = int.Parse(obj.EndPeriod),
            //    ShiftDate = obj.date,
            //    ShiftCode = obj.ShiftCode,
            //    Ammount = obj.Ammount
            //};

            UnitOfWork.SaveChanges();
        }

        public void CompleteMealAllowance(string noregCurentApprover, DocumentApproval documentApproval)
        {
            DocumentRequestDetail documentRequestDetail = DocumentRequestDetailRepository.Fetch().Where(x => x.DocumentApprovalId == documentApproval.Id).FirstOrDefault();
            MealAllowanceViewModel obj = JsonConvert.DeserializeObject<MealAllowanceViewModel>(documentRequestDetail.ObjectValue);

            //add
            foreach (var item in obj.data)
            {
                MealAllowance mealAllowance = new MealAllowance()
                {
                    NoReg = documentApproval.CreatedBy,
                    PeriodDate = item.Date.Value,
                    TotalDays = 1,
                    Amount = item.Amount,
                    TotalAmount = item.Amount
                };
                MealAllowanceRepository.Add(mealAllowance);
            }

            UnitOfWork.SaveChanges();
        }

        public void CompleteBpkbRequest(string noregCurentApprover, DocumentApproval documentApproval)
        {
            DocumentRequestDetail documentRequestDetail = DocumentRequestDetailRepository.Fetch().Where(x => x.DocumentApprovalId == documentApproval.Id).FirstOrDefault();
            ReturnBpkbCOPViewModel obj = JsonConvert.DeserializeObject<ReturnBpkbCOPViewModel>(documentRequestDetail.ObjectValue);

            BpkbRequest bpkbRequest = new BpkbRequest()
            {
                RequestType = "get",
                NoReg = documentApproval.CreatedBy,
                BPKBId = obj.BpkpId,
                BPKBNo = obj.BPKBNo,
                BorrowPurpose = obj.Name,
                //BorrowDate = obj.LoanDate.Value,
                //ReturnDate = obj.ReturnDate.Value
                BorrowDate = documentApproval.CreatedOn,
                ReturnDate = DateTime.Now
            };

            BpkbRequestRepository.Add(bpkbRequest);
            
            UnitOfWork.SaveChanges();
        }

        public void CompleteBpkbBorrow(string noregCurentApprover, DocumentApproval documentApproval)
        {
            DocumentRequestDetail documentRequestDetail = DocumentRequestDetailRepository.Fetch().Where(x => x.DocumentApprovalId == documentApproval.Id).FirstOrDefault();
            GetBpkbCopViewModel obj = JsonConvert.DeserializeObject<GetBpkbCopViewModel>(documentRequestDetail.ObjectValue);

            BpkbRequest bpkbRequest = new BpkbRequest()
            {
                RequestType = "borrow",
                NoReg = documentApproval.CreatedBy,
                BPKBId = obj.BpkpId,
                BPKBNo = obj.BPKBNo,
                BorrowPurpose = obj.Name,
                BorrowDate = DateTime.Now,
                ReturnDate = DateTime.Now
            };

            BpkbRequestRepository.Add(bpkbRequest);
            UnitOfWork.SaveChanges();

            UnitOfWork.SaveChanges();
        }

        public void CompleteAyoSekolah(string noregCurentApprover, DocumentApproval documentApproval)
        {
            DocumentRequestDetail documentRequestDetail = DocumentRequestDetailRepository.Fetch().Where(x => x.DocumentApprovalId == documentApproval.Id).FirstOrDefault();
            AyoSekolahViewModel obj = JsonConvert.DeserializeObject<AyoSekolahViewModel>(documentRequestDetail.ObjectValue);

            //add
            AyoSekolah ayoSekolah = new AyoSekolah()
            {
                NoReg = documentApproval.CreatedBy,
                Amount = new CoreService(UnitOfWork).GetAllowanceAmount(documentApproval.CreatedBy, "ayosekolah"),
                StatusRequest =true
            };
        
            AyoSekolahRepository.Add(ayoSekolah);
            UnitOfWork.SaveChanges();
        }

        public void CompleteKBAllowance(string noregCurentApprover, DocumentApproval documentApproval)
        {
            var documentRequestDetail = DocumentRequestDetailRepository.Fetch().Where(x => x.DocumentApprovalId == documentApproval.Id).FirstOrDefault();
            var obj = JsonConvert.DeserializeObject<KbAllowanceViewModel>(documentRequestDetail.ObjectValue);

            var kbAllowance = new KBAllowance()
            {
                NoReg = documentApproval.CreatedBy,
                FamilyMemberType = obj.FamilyRelationship,
                PatientName = obj.PassienName,
                HospitalName = obj.Hospital,
                HospitalAddress = obj.HospitalAddress,
                TransactionDate = obj.ActionKBDate.Value,
                Ammount = obj.Cost
            };

            KBAllowanceRepository.Add(kbAllowance);

            UnitOfWork.SaveChanges();
        }
        public void CompleteLetterGuarantee(string noregCurentApprover, DocumentApproval documentApproval)
        {
            var documentRequestDetail = DocumentRequestDetailRepository.Fetch().Where(x => x.DocumentApprovalId == documentApproval.Id).FirstOrDefault();
            var obj = JsonConvert.DeserializeObject<LetterOfGuaranteeViewModel>(documentRequestDetail.ObjectValue);

            var letterGuarantee = new LetterGuarantee()
            {
                NoReg = documentApproval.CreatedBy,
                FamilyMemberType = obj.FamilyRelationship,
                PatientName = ObjectHelper.IsIn(obj.FamilyRelationship, new[] { "Anak", "rsanak" }) ? obj.PatientChildName : obj.PatientName,
                HospitalName = obj.Hospital,
                HospitalAddress = obj.HospitalAddress,
                HospitalCity = obj.HospitalCity,
                BenefitClassification = obj.BenefitClassification,
                DateIn = obj.StartDateOfCare.Value,
                DateOut = obj.EndDateOfCare.Value,
                ControlCriteria = obj.CriteriaControl,
                SurgeryType = obj.CriteriaControl == "pascarawatinap" ? obj.DiagnosaRawatInap : obj.Diagnosa,
                ControlNumber = string.IsNullOrEmpty(obj.CheckUpCount) ? 1 : int.Parse(obj.CheckUpCount),
            };

            LetterGuaranteeRepository.Add(letterGuarantee);

            UnitOfWork.SaveChanges();
        }
        #endregion

        #region Terbilang
        public string terbilang(int angka)
        {
            string strterbilang = "";
            string[] a = { "", "Satu", "Dua", "Tiga", "Empat", "Lima", "Enam", "Tujuh", "Delapan", "Sembilan", "Sepuluh", "Sebelas" };

            if (angka < 12)
            {
                strterbilang = " " + a[angka];
            }
            else if (angka < 20)
            {
                strterbilang = this.terbilang(angka - 10) + " Belas";
            }
            else if (angka < 100)
            {
                strterbilang = this.terbilang(angka / 10) + " Puluh" + this.terbilang(angka % 10);
            }
            else if (angka < 200)
            {
                strterbilang = " Seratus" + this.terbilang(angka - 100);
            }
            else if (angka < 1000)
            {
                strterbilang = this.terbilang(angka / 100) + " Ratus" + this.terbilang(angka % 10);
            }
            else if (angka < 2000)
            {
                strterbilang = " Seribu" + this.terbilang(angka - 1000);
            }
            else if (angka < 1000000)
            {
                strterbilang = this.terbilang(angka / 1000) + " Ribu" + this.terbilang(angka % 1000);
            }
            else if (angka < 1000000000)
            {
                strterbilang = this.terbilang(angka / 1000000) + " Juta" + this.terbilang(angka % 1000000);
            }

            // menghilangkan multiple space
            strterbilang = System.Text.RegularExpressions.Regex.Replace(strterbilang, @"^\s+|\s+$", " ");
            // mengembalikan hasil terbilang
            return strterbilang;
        }

        string[] satuan = new string[10] { "Nol", "Satu", "Dua", "Tiga", "Empat", "Lima", "Enam", "Tujuh", "Delapan", "Sembilan" };
        string[] belasan = new string[10] { "Sepuluh", "Sebelas", "Dua Belas", "Tiga Belas", "Empat Belas", "Lima Belas", "Enam Belas", "Tujuh Belas", "Delapan Belas", "Sembilan Belas" };
        string[] puluhan = new string[10] { "", "", "Dua Puluh", "Tiga Puluh", "Empat Puluh", "Lima Puluh", "Enam Puluh", "Tujuh Puluh", "Delapan Puluh", "Sembilan Puluh" };
        string[] ribuan = new string[5] { "", "Ribu", "Juta", "Milyar", "Triliyun" };
        public string Terbilang(Decimal d)
        {
            string strHasil = "";
            Decimal frac = d - Decimal.Truncate(d);

            if (Decimal.Compare(frac, 0.0m) != 0)
                strHasil = Terbilang(Decimal.Round(frac * 100)) + "";
            else
                strHasil = "Rupiah";
            int xDigit = 0;
            int xPosisi = 0;

            string strTemp = Decimal.Truncate(d).ToString();
            for (int i = strTemp.Length; i > 0; i--)
            {
                string tmpx = "";
                xDigit = Convert.ToInt32(strTemp.Substring(i - 1, 1));
                xPosisi = (strTemp.Length - i) + 1;
                switch (xPosisi % 3)
                {
                    case 1:
                        bool allNull = false;
                        if (i == 1)
                            tmpx = satuan[xDigit] + " ";
                        else if (strTemp.Substring(i - 2, 1) == "1")
                            tmpx = belasan[xDigit] + " ";
                        else if (xDigit > 0)
                            tmpx = satuan[xDigit] + " ";
                        else
                        {
                            allNull = true;
                            if (i > 1)
                                if (strTemp.Substring(i - 2, 1) != "0")
                                    allNull = false;
                            if (i > 2)
                                if (strTemp.Substring(i - 3, 1) != "0")
                                    allNull = false;
                            tmpx = "";
                        }

                        if ((!allNull) && (xPosisi > 1))
                            if ((strTemp.Length == 4) && (strTemp.Substring(0, 1) == "1"))
                                tmpx = "se" + ribuan[(int)Decimal.Round(xPosisi / 3m)] + " ";
                            else
                                tmpx = tmpx + ribuan[(int)Decimal.Round(xPosisi / 3)] + " ";
                        strHasil = tmpx + strHasil;
                        break;
                    case 2:
                        if (xDigit > 0)
                            strHasil = puluhan[xDigit] + " " + strHasil;
                        break;
                    case 0:
                        if (xDigit > 0)
                            if (xDigit == 1)
                                strHasil = "Seratus " + strHasil;
                            else
                                strHasil = satuan[xDigit] + " Ratus " + strHasil;
                        break;
                }
            }
            strHasil = strHasil.Trim().ToLower();
            if (strHasil.Length > 0)
            {
                strHasil = strHasil.Substring(0, 1).ToUpper() +
                  strHasil.Substring(1, strHasil.Length - 1);
            }
            return strHasil;
        }
        public string TerbilangAngka(Decimal d)
        {
            string strHasil = "";
            Decimal frac = d - Decimal.Truncate(d);

            if (Decimal.Compare(frac, 0.0m) != 0)
                strHasil = Terbilang(Decimal.Round(frac * 100)) + "";
            else
                strHasil = "";
            int xDigit = 0;
            int xPosisi = 0;

            string strTemp = Decimal.Truncate(d).ToString();
            for (int i = strTemp.Length; i > 0; i--)
            {
                string tmpx = "";
                xDigit = Convert.ToInt32(strTemp.Substring(i - 1, 1));
                xPosisi = (strTemp.Length - i) + 1;
                switch (xPosisi % 3)
                {
                    case 1:
                        bool allNull = false;
                        if (i == 1)
                            tmpx = satuan[xDigit] + " ";
                        else if (strTemp.Substring(i - 2, 1) == "1")
                            tmpx = belasan[xDigit] + " ";
                        else if (xDigit > 0)
                            tmpx = satuan[xDigit] + " ";
                        else
                        {
                            allNull = true;
                            if (i > 1)
                                if (strTemp.Substring(i - 2, 1) != "0")
                                    allNull = false;
                            if (i > 2)
                                if (strTemp.Substring(i - 3, 1) != "0")
                                    allNull = false;
                            tmpx = "";
                        }

                        if ((!allNull) && (xPosisi > 1))
                            if ((strTemp.Length == 4) && (strTemp.Substring(0, 1) == "1"))
                                tmpx = "se" + ribuan[(int)Decimal.Round(xPosisi / 3m)] + " ";
                            else
                                tmpx = tmpx + ribuan[(int)Decimal.Round(xPosisi / 3)] + " ";
                        strHasil = tmpx + strHasil;
                        break;
                    case 2:
                        if (xDigit > 0)
                            strHasil = puluhan[xDigit] + " " + strHasil;
                        break;
                    case 0:
                        if (xDigit > 0)
                            if (xDigit == 1)
                                strHasil = "Seratus " + strHasil;
                            else
                                strHasil = satuan[xDigit] + " Ratus " + strHasil;
                        break;
                }
            }
            strHasil = strHasil.Trim().ToLower();
            if (strHasil.Length > 0)
            {
                strHasil = strHasil.Substring(0, 1).ToUpper() +
                  strHasil.Substring(1, strHasil.Length - 1);
            }
            return strHasil;
        }
        #endregion

        public IEnumerable<SimpleDocumentRequestDetailViewModel<T>> GetDocumentRequestDetails<T>(string formKey, DateTime startDate, DateTime endDate) where T : class
        {
            var data = UnitOfWork.UdfQuery<DocumentRequestDetailStoredEntity>(new { startDate, endDate, formKey })
                .Select(x => new SimpleDocumentRequestDetailViewModel<T>(x));

            return data;
        }

        public IEnumerable<AyoSekolah> GetAyoSekolahReport(DateTime? startdate, DateTime? enddate)
        {
            var query = AyoSekolahRepository.Fetch().AsNoTracking();

            if (startdate.HasValue)
                query = query.Where(x => x.CreatedOn >= startdate.Value);

            if (enddate.HasValue)
                query = query.Where(x => x.CreatedOn <= enddate.Value);

            var userPositions = UserPosition.Fetch().AsNoTracking();

            var result = from a in query
                         join u in userPositions on a.NoReg equals u.NoReg into joined
                         from u in joined.DefaultIfEmpty() // biar gak error kalau gak ada match
                         orderby a.NoReg descending
                         select new AyoSekolah
                         {
                             NoReg = a.NoReg,
                             Name = u.Name,
                             PostName = u.PostName,
                             Amount = a.Amount,
                             CreatedOn = a.CreatedOn
                         };

            return result.ToList();
        }

        public DataSourceResult GetCopFuelDetailsReport(DataSourceRequest request,DateTime? startdate, DateTime? enddate)
        {
            var query = CopFuelRepository.Fetch().AsNoTracking();

            if (startdate.HasValue)
                query = query.Where(x => x.COPFuelDate >= startdate.Value);

            if (enddate.HasValue)
                query = query.Where(x => x.COPFuelDate <= enddate.Value);

            var userPositions = UserPosition.Fetch().AsNoTracking();

            var result = from a in query
                         join u in userPositions on a.NoReg equals u.NoReg into joined
                         from u in joined.DefaultIfEmpty()
                         orderby a.NoReg descending
                         select new CopFuel
                         {
                             NoReg = a.NoReg,
                             Name = u.Name,
                             PostName = u.PostName,
                             COPFuelDate = a.COPFuelDate,
                             DestinationStart = a.DestinationStart,
                             KMTotal = Convert.ToDouble(a.KMTotal)
                         };

            return result.ToDataSourceResult(request);
        }
    }
}