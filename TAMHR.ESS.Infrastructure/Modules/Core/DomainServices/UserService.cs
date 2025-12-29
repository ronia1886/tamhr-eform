using System;
using System.Linq;
using System.Security.Claims;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.Exceptions;
using TAMHR.ESS.Infrastructure.ViewModels;
using Agit.Common;
using Agit.Domain;
using Agit.Common.Utility;
using Agit.Common.Extensions;
using Agit.Domain.Extensions;
using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;
using Z.EntityFramework.Plus;
using System.Reflection;
using TAMHR.ESS.Domain.Models.Core.StoredEntities;
using Newtonsoft.Json;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    /// <summary>
    /// Service class that handle user and authentication.
    /// </summary>
    public class UserService : DomainServiceBase
    {
        #region Domain Repositories
        /// <summary>
        /// User repository object.
        /// </summary>
        protected IRepository<User> UserRepository => UnitOfWork.GetRepository<User>();
        protected IRepository<UserView> UserViewRepository => UnitOfWork.GetRepository<UserView>();

        /// <summary>
        /// Date specification readonly repository.
        /// </summary>
        protected IReadonlyRepository<DateSpecification> DateSpecificationReadonlyRepository => UnitOfWork.GetRepository<DateSpecification>();

        /// <summary>
        /// Time management readonly repository.
        /// </summary>
        protected IReadonlyRepository<TimeManagement> TimeManagementReadonlyRepository => UnitOfWork.GetRepository<TimeManagement>();

        /// <summary>
        /// User impersonation repository.
        /// </summary>
        protected IRepository<UserImpersonation> UserImpersonationRepository => UnitOfWork.GetRepository<UserImpersonation>();

        /// <summary>
        /// User role repository.
        /// </summary>
        protected IRepository<UserRole> UserRoleRepository => UnitOfWork.GetRepository<UserRole>();

        /// <summary>
        /// User position readonly repository.
        /// </summary>
        protected IReadonlyRepository<UserPositionView> UserPositionReadonlyRepository => UnitOfWork.GetRepository<UserPositionView>();

        /// <summary>
        /// Access role readonly repository.
        /// </summary>
        protected IReadonlyRepository<AccessRoleView> AccessRoleReadonlyRepository => UnitOfWork.GetRepository<AccessRoleView>();

        /// <summary>
        /// Personal data readonly repository.
        /// </summary>
        protected IReadonlyRepository<PersonalData> PersonalDataReadonlyRepository => UnitOfWork.GetRepository<PersonalData>();

        /// <summary>
        /// Personal data common attribute readonly repository.
        /// </summary>
        protected IReadonlyRepository<PersonalDataCommonAttribute> PersonalDataCommonAttributeReadonlyRepository => UnitOfWork.GetRepository<PersonalDataCommonAttribute>();
        #endregion

        #region Variables & Properties
        /// <summary>
        /// List of entity fields that can be updated.
        /// </summary>
        private readonly string[] _updatedProperties = new[] { "Name", "Username", "Email" }; 
        #endregion

        #region Constructor
        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="unitOfWork">This <see cref="IUnitOfWork"/> concrete object.</param>
        public UserService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Get list of users.
        /// </summary>
        /// <returns>This list of <see cref="UserPositionView"/> objects</returns>
        public IQueryable<UserPositionSafeDto> Gets()
        {
            // Get list of users without object tracking.
            //return UserPositionReadonlyRepository.Fetch()
            //    .AsNoTracking();
            var userSet = UserPositionReadonlyRepository.Fetch();

            var query = from u in userSet.AsNoTracking()
                        select new UserPositionSafeDto
                        {
                            Id = u.Id,
                            NoReg = u.NoReg,
                            Name = u.Name,
                            Username = u.Username,
                            Gender = u.Gender,
                            Email = u.Email,
                            PostCode = u.PostCode,
                            PostName = u.PostName,
                            Active = u.Active,
                            CreatedOn = u.CreatedOn,
                            CreatedBy = u.CreatedBy,
                            ModifiedOn = u.ModifiedOn,
                            ModifiedBy = u.ModifiedBy,
                            LastViewedOn = u.LastViewedOn,
                            RowStatus = u.RowStatus,
                        };

            return query;
        }

        /// <summary>
        /// Get list of active users.
        /// </summary>
        /// <returnsThis list of <see cref="UserPositionView"/> objects.</returns>
        public IQueryable<UserPositionSafeDto> GetActiveUsers()
        {
            // Get list of users filtered by active flag.
            return Gets().Where(x => x.Active);
        }

        /// <summary>
        /// Get list of active users by gender.
        /// </summary>
        /// <returns>This list of <see cref="UserPositionView"/> objects.</returns>
        public IQueryable<UserPositionSafeDto> GetActiveUsersByGender(string gender)
        {
            // Get list of active users by gender in lower case.
            return GetActiveUsers().Where(x => x.Gender.ToLower() == gender.ToLower());
        }

        /// <summary>
        /// Get list of leaves by type.
        /// </summary>
        /// <param name="noreg">This current user session noreg.</param>
        /// <param name="category">This absence category</param>
        /// <returns>This list of <see cref="TimeManagement"/> objects.</returns>
        public IEnumerable<EmployeeLeaveStoredEntity> GetLeaves(string noreg, string category)
        {
            // Get and set presence code from given category.
            var presenceCode = category == "long-leave" ? "8" : "7";

            var reasonLeave = category == "long-leave" ? "p-CutiPanjang" : "p-CutiYearly";

            var listCuti = new string[2]{ "cuti","cutipanjang"};

            // Get and set current date and time.
            var now = DateTime.Now.Date;

            // Get and set the first date of current year.
            var startDate = new DateTime(now.Year, 1, 1);

            // Get and set the last date of current year.
            var endDate = new DateTime(now.Year, 12, 31);

            // If the category was long leave then get the nearest start date from current year.
            if (category == "long-leave")
            {
                // Set long leave offset to 5 years.
                var offset = 5;

                // Get astra join date by current user session noreg.
                var joinAstraDate = DateSpecificationReadonlyRepository.Fetch()
                    .AsNoTracking()
                    .Where(x => x.NoReg == noreg)
                    .FirstOrDefaultIfEmpty()
                    .AstraDate;

                // Calculate the difference years.
                var diffYear = (int)((now - joinAstraDate).TotalDays / 365) / offset ;

                var leaveYears = (int)diffYear * offset;
                
                // Get and set the day.
                var day = joinAstraDate.Month != 2 ? joinAstraDate.Day : (joinAstraDate.Day >= 27 ? 27 : joinAstraDate.Day);

                // Set the nearest start date.
                startDate = new DateTime(joinAstraDate.Year + leaveYears, joinAstraDate.Month, day);

                // Set the end date.
                endDate = startDate.AddYears(offset);
            }

            var employeeLeaveData =  UnitOfWork.UdfQuery<EmployeeLeaveStoredEntity>(new { Noreg = noreg,StartDatePeriod = startDate,EndDatePeriod = endDate,AbsentStatus = presenceCode }).ToList();

            var approvalService = new ApprovalService(UnitOfWork, null, null);
            var timeManagementService = new TimeManagementService(UnitOfWork, null);

            var objLeaveOnprogress = approvalService.GetInprogressDraftRequestDetails("absence").Where(x => x.CreatedBy == noreg).Select(x => JsonConvert.DeserializeObject<AbsenceViewModel>(x.ObjectValue)).Where(x=>x.StartDate.Value > DateTime.Now.Date && Convert.ToInt32(x.TotalAbsence) > 0 && listCuti.Contains(x.ReasonType)).ToList();
            if (objLeaveOnprogress.Count > 0)
            {
                
                int currentYear = DateTime.Now.Year;

                foreach (var item in objLeaveOnprogress)
                {
                    
                    // Ambil start dan end date
                    var start = item.StartDate.Value;
                    var end = item.EndDate.Value;
                    

                    // Ambil list tanggal kerja dari service
                    var workDays = timeManagementService.GetListWorkSchEmp(noreg, start, end)
                        .Where(w => w.Date.Year == currentYear && !w.Off && !w.Holiday) // hanya hari kerja tahun ini
                        .Select(w => w.Date)
                        .ToArray();

                    var totalDuration = workDays.Length;

                    for(int i = 0;i < totalDuration; i++)
                    {
                        var newEmployeeLeaveData = new EmployeeLeaveStoredEntity
                        {
                            Noreg = noreg,
                            WorkingDate = workDays[i],
                            AbsentStatus = presenceCode,
                            ReasonCode = reasonLeave,
                            Description = item.Description,
                            DocumentStatus = "inprogress"
                        };
                        employeeLeaveData.Add(newEmployeeLeaveData);
                    }

                   
                }

            }

            return employeeLeaveData;
            //// Return the list without object tracking.
            //return TimeManagementReadonlyRepository.Fetch()
            //    .AsNoTracking()
            //    .Where(x => x.NoReg == noreg && x.WorkingDate >= startDate && x.WorkingDate <= endDate && x.AbsentStatus == presenceCode);
        }

        /// <summary>
        /// Get list of roles by user id.
        /// </summary>
        /// <param name="id">This user id.</param>
        /// <returns>This list of <see cref="UserRoleStoredEntity"/> objects.</returns>
        public IEnumerable<UserRoleStoredEntity> GetRoles(Guid id)
        {
            // Executed table-valued function with user id as parameter.
            return UnitOfWork.UdfQuery<UserRoleStoredEntity>(new { userId = id });
        }

        /// <summary>
        /// Get list of users by list of usernames.
        /// </summary>
        /// <param name="userNames">This list of usernames</param>
        /// <returns>This list of <see cref="User"/> objects.</returns>
        public IEnumerable<User> GetByUserNames(IEnumerable<string> userNames)
        {
            // Get list of users by list of usernames without object tracking.
            //return UserRepository.Fetch()
            //    .AsNoTracking()
            //    .Where(x => userNames.Contains(x.Username));
            var userNameQueryable = userNames.AsQueryable();
            var set = UnitOfWork.GetRepository<User>();

            var users = set.Fetch()
                .AsNoTracking();

            if (userNameQueryable.Any())
            {
                users = users.Join(
                    userNameQueryable,             // daftar username yang dicari
                    u => u.Username,               // kolom pada tabel User
                    name => name,                  // elemen pada IEnumerable
                    (u, name) => u                 // hasil join
                );
            }

            return users.ToList();
        }

        /// <summary>
        /// Get user by id.
        /// </summary>
        /// <param name="id">This user id.</param>
        /// <returns>This <see cref="User"/> object.</returns>
        public User Get(Guid id)
        {
            // Get single user object by id.
            return UserRepository.FindById(id);
        }

        /// <summary>
        /// Get user by noreg.
        /// </summary>
        /// <param name="noreg">This user noreg.</param>
        /// <returns>This <see cref="User"/> object.</returns>
        public User GetByNoReg(string noreg)
        {
            // Get single user object by noreg.
            return UserRepository.Fetch()
                .AsNoTracking()
                .FirstOrDefault(x => x.NoReg == noreg);
        }

        /// <summary>
        /// Get list of users by list of noregs.
        /// </summary>
        /// <param name="noregs">This list of user noregs.</param>
        /// <returns>This list of <see cref="User"/> objects.</returns>
        public IEnumerable<User> GetByNoRegs(string[] noregs)
        {
            // Get list of users by list of noregs without object tracking.
            return UserRepository.Fetch()
                .AsNoTracking()
                .Where(x => noregs.Contains(x.NoReg));
        }

        /// <summary>
        /// Get user by username.
        /// </summary>
        /// <param name="username">This username.</param>
        /// <returns>This <see cref="User"/> object.</returns>
        public User GetByUsername(string username)
        {
            // Get last user object from list without object tracking.
            return UserRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.Username == username)
                .OrderBy(x => x.NoReg)
                .LastOrDefault();
        }

        public UserView GetUVByUsername(string username)
        {
            // Get last user object from list without object tracking.
            return UserViewRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.Username == username)
                .OrderBy(x => x.NoReg)
                .LastOrDefault();
        }

        /// <summary>
        /// Update or insert user.
        /// </summary>
        /// <param name="user">This <see cref="User"/> object.</param>
        /// <returns>True if success, false otherwise.</returns>
        public bool Upsert(User user)
        {
            // Update or insert user with specified list of properties to update.
            UserRepository.Upsert<Guid>(user, _updatedProperties);

            // Push pending changes into database and return the boolean result.
            return UnitOfWork.SaveChanges() > 0;
        }

        /// <summary>
        /// Update user role.
        /// </summary>
        /// <param name="viewModel">This <see cref="UserViewModel"/> object.</param>
        /// <returns>True if success, false otherwise.</returns>
        public bool Update(UserViewModel viewModel)
        {
            // Create output variable with false as default value.
            var output = false;

            // Begin transaction with read uncommitted isolation level.
            UnitOfWork.Transact(() =>
            {
                // Delete all user roles filtered by user id.
                UserRoleRepository.Fetch()
                    .Where(x => x.UserId == viewModel.Id)
                    .Delete();

                // Enumerate list of roles from view model.
                foreach (var role in viewModel.Roles)
                {
                    // Create new user role object and add it into repository.
                    UserRoleRepository.Add(UserRole.Create(viewModel.Id, role));
                }

                // Push pending changes into database and save the boolean result into output variable.
                output = UnitOfWork.SaveChanges() > 0;
            }, System.Data.IsolationLevel.ReadUncommitted);

            // Return the output variable.
            return output;
        }

        /// <summary>
        /// Soft delete user by id.
        /// </summary>
        /// <param name="id">This user id.</param>
        /// <returns>True if success, false otherwise.</returns>
        public bool SoftDelete(Guid id)
        {
            // Create output variable with false as default value.
            var output = false;

            // Begin transaction.
            UnitOfWork.Transact(() => {
                // Delete list of user roles by user id.
                UserRoleRepository.Fetch()
                    .Where(x => x.UserId == id)
                    .Delete();

                // Mark user object as deleted.
                UserRepository.DeleteById(id);

                // Push pending changes into database and save the boolean result into output variable.
                output = UnitOfWork.SaveChanges() > 0;
            });

            // Return the output variable.
            return output;
        }

        /// <summary>
        /// Get list of role keys by user id.
        /// </summary>
        /// <param name="id">This user id.</param>
        /// <returns>This list of role keys</returns>
        public IEnumerable<string> GetUserRoles(Guid id)
        {
            // Get list of roles by user id without object tracking.
            var roles = UserRoleRepository
                .Fetch()
                .AsNoTracking()
                .Where(x => x.UserId == id)
                .Select(x => x.Role.RoleKey)
                .ToList();

            // Return the role keys.
            return roles;
        }

        /// <summary>
        /// Get list of users by specified role key.
        /// </summary>
        /// <param name="roleKey">This role key.</param>
        /// <returns>This list of <see cref="User"/> objects.</returns>
        public IEnumerable<User> GetUsersByRole(string roleKey)
        {
            // Get list of users by role key.
            var users = (
                from u in UserRepository.Fetch().AsNoTracking()
                join r in UserRoleRepository.Fetch().AsNoTracking()
                on u.Id equals r.UserId
                where r.Role.RoleKey.Equals(roleKey)
                select r.User
            ).ToList();
             
            // Return the list.
            return users;
        }

        /// <summary>
        /// Validate and get claims.
        /// </summary>
        /// <param name="username">This username.</param>
        /// <param name="password">This password.</param>
        /// <param name="ignorePassword">Determine whether password should be ignored or not (if using proxy as another user this parameter should be set to true).</param>
        /// <param name="originator">This current user session origin identifier.</param>
        /// <param name="postCode">This current user session position code.</param>
        /// <returns>This list of <see cref="Claim"/> objects.</returns>
        public IEnumerable<Claim> GetClaims(string username, string password, bool ignorePassword = false, string originator = "", string postCode = "")
        {
            // Throw an exception if the username is empty.
            Assert.HasText(username, "Username cannot be empty");

            // If password was not ignored then check the password text.
            if (!ignorePassword)
            {
                // Throw an exception if the password is empty.
                Assert.HasText(password, "Password cannot be empty");
            }

            // Get user by username.
            //var user = GetByUsername(username);
            var user = GetUVByUsername(username);

            var isExternalUser = user.NoReg.StartsWith("ext_");

            // Throw an exception if the user object does not exist.
            Assert.ThrowIf(user == null, new TitledException("Invalid Username", $"Username with name '{username}' is not found"));

            // If password was not ignored then match the password.
            if (!ignorePassword)
            {
                // Throw an exception if the password didnt match with existing.
                Assert.ThrowIf(Cryptography.ToMD5(password) != user.Password, new TitledException("Invalid Password", "You entered the wrong password"));
            }

            // Get list of user roles by user id.
            var userRoles = GetUserRoles(user.Id).ToList();

            // Get list of organization objects by user noreg.
            var organizations = UnitOfWork.GetRepository<ActualOrganizationStructure>().Fetch()
                .AsNoTracking()
                .Where(x => x.NoReg == user.NoReg)
                .ToList();

            // Get and set current date and time.
            var now = DateTime.Now;

            // Check whether current user session has impersonation or not.
            var hasImpersonation = UserImpersonationRepository.Fetch()
                .AsNoTracking()
                .Any(x => x.NoReg == user.NoReg && now >= x.StartDate && now <= x.EndDate);

            // Get actual organization structure object with 100% staffing (if postCode parameter was present then the staffing will be ignored).
            var actualOrganizationStructure = isExternalUser
                ? new ActualOrganizationStructure
                {
                    NoReg = user.NoReg,
                    Name = user.Name,
                    PostCode = "External position code",
                    PostName = "External position",
                    JobCode = "External job code",
                    JobName = "External job",
                    Staffing = 100
                }
                : organizations.Where(x => (!string.IsNullOrEmpty(postCode) && x.PostCode == postCode) || (string.IsNullOrEmpty(postCode) && x.Staffing == 100))
                    .FirstOrDefault();

            var isChief = false;
            var PostCode = "";
            var PostName = "";
            // Throw an exception if the organization object does not exist.
            if (user.Type == "Default")
            {
                Assert.ThrowIf(actualOrganizationStructure == null, $"Employee with name {user.Name} is not active");
                var isExpat = actualOrganizationStructure.WorkContract == "Z4";
                userRoles.Add(isExternalUser ? BuiltInRoles.ExternalUser : (isExpat ? BuiltInRoles.ExpatUser : BuiltInRoles.Default));

                if (actualOrganizationStructure.PostCode != null)
                    PostCode = actualOrganizationStructure.PostCode;
                if (actualOrganizationStructure.PostName != null)
                    PostName = actualOrganizationStructure.PostName;

                if (!isExpat)
                {
                    // Create list of criterions to get list of automatic roles.
                    var criterions = new[] {
                    // Get current user gender.
                    user.Gender,
                    // Get current user job code.
                    actualOrganizationStructure.JobCode,
                    // Get current user employee subgroup.
                    actualOrganizationStructure.EmployeeSubgroup,
                    // Get current user work contract.
                    "wc." + actualOrganizationStructure.WorkContract
                };
                    var criterionsQuery = criterions.AsQueryable();

                    // Get list of automatic roles by given criterions.
                    var additionalRoles = AccessRoleReadonlyRepository.Fetch()
                        .AsNoTracking()
                        //.Where(x => criterions.Contains(x.AccessCode))
                        .Join(criterionsQuery, // Gunakan IQueryable dari daftar ID
                          data => data.AccessCode,
                          idItem => idItem,
                          (data, idItem) => data)

                        .Select(x => x.RoleKey).ToList();

                    // Combine with current user session roles.
                    userRoles.AddRange(additionalRoles);
                }

                // Determine whether current user session was chief in organization or not.
                isChief = actualOrganizationStructure.Chief == 1;

                // If chief then add new chief role to the list.
                if (isChief)
                {
                    // Add chief role into the list.
                    userRoles.Add(isExpat ? BuiltInRoles.ExpatChief : BuiltInRoles.Chief);
                }
            }

            

            

            // Get total position of current user session in organization.
            var totalPosition = organizations.Count();

            // Create new list of user claims.
            var claims = new List<Claim>
            {
                new Claim("Type", user.Type),
                new Claim("Role", user.Role),
                new Claim("Id", user.Id.ToString()),
                // Create new claim to store current user session noreg information.
                new Claim("NoReg", user.NoReg),
                // Create new claim to store current user session username information.
                new Claim("Username", user.Username),
                // Create new claim to store current user session name information.
                new Claim("Name", user.Name),
                // Create new claim to store current user session total position information.
                new Claim("TotalPosition", totalPosition.ToString()),
                // Create new claim to store current user session position code information.
                new Claim("PostCode", PostCode),
                // Create new claim to store current user session position name information.
                new Claim("PostName", PostName),
                // Create new claim to store current user session has impersonation flag information.
                new Claim("HasImpersonation", hasImpersonation.ToString()),
                // Create new claim to store current user session chief flag information.
                new Claim("Chief", isChief.ToString()),
                // Create new claim to store current user session originator information.
                new Claim("Originator", !string.IsNullOrEmpty(originator) ? originator : user.Username),
                // Create new claim to store current user session roles information.
                new Claim("Roles", string.Join(",", userRoles.Distinct())),
                new Claim("Gender", user.Gender)
            };

            //// Begin try and catch.
            //try
            //{
            //    // Call stored procedure to generate absence.
            //    UnitOfWork.UspQuery("SP_GENERATE_PROXY", new { keyDate = now, noreg = user.NoReg });
            //}
            //// Catch an exception
            //catch
            //{
            //}

            // Return the list of user claims.
            return claims;
        }

        /// <summary>
        /// Proxy as another user and get list of claims.
        /// </summary>
        /// <param name="username">This username.</param>
        /// <param name="originator">This current user session origin identifier.</param>
        /// <param name="postCode">This current user session position code.</param>
        /// <returns>This list of <see cref="Claim"/> objects.</returns>
        public IEnumerable<Claim> ProxyAs(string username, string originator = "", string postCode = "")
        {
            // Get list of claims.
            return GetClaims(username, null, true, originator, postCode);
        }
        #endregion
    }
}
