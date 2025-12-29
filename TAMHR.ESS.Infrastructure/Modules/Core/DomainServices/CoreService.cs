using System;
using System.Web;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.Helpers;
using TAMHR.ESS.Infrastructure.ViewModels;
using Agit.Common;
using Agit.Domain;
using Agit.Common.Utility;
using Agit.Common.Extensions;
using Agit.Domain.Extensions;
using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;
using Z.EntityFramework.Plus;
using Newtonsoft.Json;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    /// <summary>
    /// Service class that handle role, menu, permission, notification, dashboard, language, email template, mail queue, and events calendar.
    /// </summary>
    public class CoreService : DomainServiceBase
    {
        #region Repositories
        /// <summary>
        /// User repository.
        /// </summary>
        protected IRepository<User> UserRepository => UnitOfWork.GetRepository<User>();

        /// <summary>
        /// User impersonation repository.
        /// </summary>
        protected IRepository<UserImpersonation> UserImpersonationRepository => UnitOfWork.GetRepository<UserImpersonation>();

        /// <summary>
        /// User impersonation readonly repository.
        /// </summary>
        protected IReadonlyRepository<UserImpersonationView> UserImpersonationReadonlyRepository => UnitOfWork.GetRepository<UserImpersonationView>();

        /// <summary>
        /// Menu repository.
        /// </summary>
        protected IRepository<Menu> MenuRepository => UnitOfWork.GetRepository<Menu>();

        /// <summary>
        /// Menu permission readonly repository.
        /// </summary>
        protected IReadonlyRepository<MenuPermissionView> MenuPermissionRepository => UnitOfWork.GetRepository<MenuPermissionView>();

        /// <summary>
        /// Language repository.
        /// </summary>
        protected IRepository<Language> LanguageRepository => UnitOfWork.GetRepository<Language>();

        /// Role repository.
        /// </summary>
        protected IRepository<Role> RoleRepository => UnitOfWork.GetRepository<Role>();

        /// <summary>
        /// Permission repository.
        protected IRepository<Permission> PermissionRepository => UnitOfWork.GetRepository<Permission>();

        /// <summary>
        /// Readonly role permission repository.
        /// </summary>
        protected IReadonlyRepository<RolePermissionView> RolePermissionReadonlyRepository => UnitOfWork.GetRepository<RolePermissionView>();

        /// <summary>
        /// User role repository.
        /// </summary>
        protected IRepository<UserRole> UserRoleRepository => UnitOfWork.GetRepository<UserRole>();

        /// <summary>
        /// Access role repository.
        /// </summary>
        protected IRepository<AccessRole> AccessRoleRepository => UnitOfWork.GetRepository<AccessRole>();

        /// <summary>
        /// Favourite menu repository.
        /// </summary>
        protected IRepository<FavouriteMenu> FavouriteMenuRepository => UnitOfWork.GetRepository<FavouriteMenu>();

        /// <summary>
        /// Role permission repository.
        /// </summary>
        protected IRepository<RolePermission> RolePermissionRepository => UnitOfWork.GetRepository<RolePermission>();

        /// <summary>
        /// Notification repository.
        /// </summary>
        protected IRepository<Notification> NotificationRepository => UnitOfWork.GetRepository<Notification>();

        /// <summary>
        /// Events calendar repository.
        /// </summary>
        protected IRepository<EventsCalendar> EventsCalendarRepository => UnitOfWork.GetRepository<EventsCalendar>();

        /// <summary>
        /// Faskes repository.
        /// </summary>
        protected IRepository<Faskes> FaskesRepository => UnitOfWork.GetRepository<Faskes>();

        /// <summary>
        /// Vehicle repository.
        /// </summary>
        protected IRepository<Vehicle> VehicleRepository => UnitOfWork.GetRepository<Vehicle>();

        /// <summary>
        /// Vehicle matrix repository.
        /// </summary>
        protected IRepository<VehicleMatrix> VehicleMatrixRepository => UnitOfWork.GetRepository<VehicleMatrix>();

        /// <summary>
        /// Absence repository.
        /// </summary>
        protected IRepository<Absence> AbsenceRepository => UnitOfWork.GetRepository<Absence>();

        /// <summary>
        /// Bank repository.
        /// </summary>
        protected IRepository<Bank> BankRepository => UnitOfWork.GetRepository<Bank>();

        /// <summary>
        /// BPKB repository.
        /// </summary>
        protected IRepository<Bpkb> BpkbRepository => UnitOfWork.GetRepository<Bpkb>();

        /// <summary>
        /// Hospital repository.
        /// </summary>
        protected IRepository<Hospital> HospitalRepository => UnitOfWork.GetRepository<Hospital>();

        /// <summary>
        /// Guideline repository.
        /// </summary>
        protected IRepository<Guideline> GuidelineRepository => UnitOfWork.GetRepository<Guideline>();

        /// <summary>
        /// Email template repository.
        /// </summary>
        protected IRepository<EmailTemplate> EmailTemplateRepository => UnitOfWork.GetRepository<EmailTemplate>();

        /// <summary>
        /// Mail queue repository.
        /// </summary>
        protected IRepository<MailQueue> MailQueueRepository => UnitOfWork.GetRepository<MailQueue>();

        /// <summary>
        /// Mail Sent Log repository.
        /// </summary>
        protected IRepository<MailSentLog> MailSentLogRepository => UnitOfWork.GetRepository<MailSentLog>();

        /// <summary>
        /// Form repository.
        /// </summary>
        protected IRepository<Form> FormRepository => UnitOfWork.GetRepository<Form>();

        /// <summary>
        /// Approval matrix repository.
        /// </summary>
        protected IRepository<ApprovalMatrix> ApprovalMatrixRepository => UnitOfWork.GetRepository<ApprovalMatrix>();

        /// <summary>
        /// Actual organization structure repository.
        /// </summary>
        protected IRepository<ActualOrganizationStructure> ActualOrganizationStructureRepository => UnitOfWork.GetRepository<ActualOrganizationStructure>();

        /// <summary>
        /// Allowance repository.
        /// </summary>
        protected IRepository<AllowanceDetail> AllowanceDetailRepository => UnitOfWork.GetRepository<AllowanceDetail>();

        /// <summary>
        /// Personal data repository.
        /// </summary>
        protected IRepository<PersonalData> PersonalDataRepository => UnitOfWork.GetRepository<PersonalData>();

        /// <summary>
        /// Employee subgroup repository.
        /// </summary>
        protected IRepository<EmployeeSubgroupNP> EmployeeSubgroupNPRepository => UnitOfWork.GetRepository<EmployeeSubgroupNP>();

        /// <summary>
        /// Personal data tax status repository.
        /// </summary>
        protected IRepository<PersonalDataTaxStatus> PersonalDataTaxStatusRepository => UnitOfWork.GetRepository<PersonalDataTaxStatus>();

        /// <summary>
        /// Document approval repository.
        /// </summary>
        protected IRepository<DocumentApproval> DocumentApprovalRepository => UnitOfWork.GetRepository<DocumentApproval>();

        /// <summary>
        /// Document approval history repository.
        /// </summary>
        protected IRepository<DocumentApprovalHistory> DocumentApprovalHistoryRepository => UnitOfWork.GetRepository<DocumentApprovalHistory>();

        /// <summary>
        /// Proxy repository.
        /// </summary>
        protected IRepository<ProxyLog> ProxyLogRepository => UnitOfWork.GetRepository<ProxyLog>();

        /// <summary>
        /// Tracking approval repository.
        /// </summary>
        protected IReadonlyRepository<TrackingApproval> TrackingApprovalReadonlyRepository => UnitOfWork.GetRepository<TrackingApproval>();

        /// <summary>
        /// User activity log repository.
        /// </summary>
        protected IRepository<UserActivityLog> UserActivityLogRepository => UnitOfWork.GetRepository<UserActivityLog>();
        #endregion

        /// <summary>
        /// Vaccine repository.
        /// </summary>
        protected IRepository<Vaccine> VaccineRepository => UnitOfWork.GetRepository<Vaccine>();

        /// <summary>
        /// Vaccine repository.
        /// </summary>
        protected IRepository<VaccineSummaryStoredEntity> VaccineSummaryStoredEntityRepository => UnitOfWork.GetRepository<VaccineSummaryStoredEntity>();

        /// <summary>
        /// Vaccine Schedule repository.
        /// </summary>
        protected IRepository<VaccineSchedule> VaccineScheduleRepository => UnitOfWork.GetRepository<VaccineSchedule>();
        #region Private Properties
        /// <summary>
        /// Field that hold tag labels for menu caching.
        /// </summary>
        private readonly string[] _menuTags = new[] { "menus" };

        /// <summary>
        /// Field that hold tag labels for caching.
        /// </summary>
        private readonly string[] _tags = new[] { "roles", "permissions" };

        /// <summary>
        /// Field that hold properties that can be updated for menu entity.
        /// </summary>
        private readonly string[] _menuProperties = new[] { "PermissionId", "Title", "Url", "Description", "IconClass", "Visible", "EnableOtp", "Params" };

        /// <summary>
        /// Field that hold properties that can be updated for role entity.
        /// </summary>
        private readonly string[] _roleProperties = new[] { "RoleKey", "Title", "Description", "RoleTypeCode" };

        /// <summary>
        /// Field that hold properties that can be updated for permission entity.
        /// </summary>
        private readonly string[] _permissionProperties = new[] { "PermissionKey", "Description", "PermissionTypeCode" };

        /// <summary>
        /// Field that hold properties that can be updated for user impersonation entity.
        /// </summary>
        private readonly string[] _userImpersonationProperties = new[] { "NoReg", "PostCode", "StartDate", "EndDate", "Description" };

        /// <summary>
        /// Field that hold properties that can be updated for events calendar entity.
        /// </summary>
        private readonly string[] _eventsCalendarProperties = new[] { "Title", "Description", "EventTypeCode", "StartDate", "EndDate" };

        /// <summary>
        /// Field that hold properties that can be updated for guideline entity.
        /// </summary>
        private readonly string[] _guidelineProperties = new[] { "Title", "Description", "StartDate", "EndDate" };

        /// <summary>
        /// Field that hold properties that can be updated for email template entity.
        /// </summary>
        private readonly string[] _emailTemplateProperties = new[] { "ModuleCode", "MailKey", "MailFrom", "DisplayName", "Title", "Subject", "MailContent" };

        /// <summary>
        /// Field that hold properties that can be updated for language entity.
        /// </summary>
        private readonly string[] _languageProperties = new[] { "CultureCode", "TranslateKey", "TranslateValue" };

        /// <summary>
        /// Field that hold properties that can be updated for proxy log entity.
        /// </summary>
        private readonly string[] _proxyLogProperties = new[] { "Originator", "TargetUsername", "IpAddress" };
        #endregion

        #region Constructor
        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="unitOfWork">Unit of Work Object</param>
        public CoreService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        #endregion

        #region User Impersonation Area
        /// <summary>
        /// Get list of user impersonations.
        /// </summary>
        /// <returns>This list of <see cref="IUserImpersonation"/> objects.</returns>
        public IEnumerable<IUserImpersonation> GetUserImpersonations()
        {
            // Get list of user impersonations without object tracking.
            return UserImpersonationReadonlyRepository.Fetch()
                .AsNoTracking();
        }

        /// <summary>
        /// Get user impersonation by id.
        /// </summary>
        /// <param name="id">This user impersonation id.</param>
        /// <returns>This <see cref="UserImpersonationView"/> object.</returns>
        public UserImpersonationView GetUserImpersonation(Guid id)
        {
            // Get user impersonation by id without object tracking and return default if empty.
            return UserImpersonationReadonlyRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.Id == id)
                .FirstOrDefaultIfEmpty();
        }

        /// <summary>
        /// Get list of user impersonations by noreg.
        /// </summary>
        /// <param name="noreg">This current user session noreg.</param>
        /// <returns>This list of <see cref="UserImpersonationView"/> objects.</returns>
        public IEnumerable<UserImpersonationView> GetUserImpersonations(string noreg)
        {
            // Get and set current date and time.
            var now = DateTime.Now;

            // Return list of user impersonations by noreg without object tracking.
            return UserImpersonationReadonlyRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.NoReg == noreg && now >= x.StartDate && now <= x.EndDate && x.RowStatus)
                .ToList();
        }

        /// <summary>
        /// Check whether user can impersonate as other user by its position code.
        /// </summary>
        /// <param name="noreg">This current user session noreg.</param>
        /// <param name="userName">This current user session username.</param>
        /// <returns>True if can impersonate, false otherwise.</returns>
        public bool CanImpersonateAs(string noreg, string username)
        {
            // Get and set current date and time.
            var now = DateTime.Now;

            // Determine whether user impersonation with given noreg and username exist or not.
            return UserImpersonationReadonlyRepository.Fetch()
                .AsNoTracking()
                .Any(x => x.NoReg == noreg && x.ImpersonateUsername == username && now >= x.StartDate && now <= x.EndDate && x.RowStatus);
        }

        /// <summary>
        /// Update or insert user impersonation.
        /// </summary>
        /// <param name="userImpersonation">This <see cref="UserImpersonation"/> object.</param>
        /// <returns>True if success, false otherwise.</returns>
        public bool UpsertUserImpersonation(UserImpersonation userImpersonation)
        {
            // Update or insert user impersonation with specified list of properties to update.
            UserImpersonationRepository.Upsert<Guid>(userImpersonation, _userImpersonationProperties);

            // Push pending changes into database and return the boolean result.
            return UnitOfWork.SaveChanges() > 0;
        }

        /// <summary>
        /// Soft delete user impersonation by id and its dependencies if any.
        /// </summary>
        /// <param name="id">This user impersonation id.</param>
        /// <returns>True if success, false otherwise.</returns>
        public bool SoftDeleteUserImpersonation(Guid id)
        {
            // Create output variable with false as default value.
            var output = false;

            // Begin transaction.
            UnitOfWork.Transact(() =>
            {
                // Get user impersonation by id.
                var userImpersonation = UserImpersonationRepository.FindById(id);

                // Update the row status value to false.
                userImpersonation.RowStatus = false;

                // Return the output variable.
                output = UnitOfWork.SaveChanges() > 0;
            });

            // Return the output variable.
            return output;
        }

        /// <summary>
        /// Delete user impersonation by id and its dependencies if any.
        /// </summary>
        /// <param name="id">This user impersonation id.</param>
        /// <returns>True if success, false otherwise.</returns>
        public bool DeleteUserImpersonation(Guid id)
        {
            // Create output variable with false as default value.
            var output = false;

            // Begin transaction.
            UnitOfWork.Transact(() =>
            {
                // Mark user impersonation object as deleted.
                UserImpersonationRepository.DeleteById(id);

                // Push pending changes into database and save the boolean result into output variable.
                output = UnitOfWork.SaveChanges() > 0;
            });

            // Return the output variable.
            return output;
        }
        #endregion

        #region Menu Area
        /// <summary>
        /// Get list of menu permissions by list of roles.
        /// </summary>
        /// <param name="roles">This list of roles.</param>
        /// <returns>This list of <see cref="MenuPermissionView"/> objects.</returns>
        public IEnumerable<MenuPermissionView> GetMenuByRoles(string[] roles)
        {
            // Get list of menu permissions from cache.
            var menus = MenuPermissionRepository.Fetch()
                .AsNoTracking()
                .FromCache(_menuTags);

            // Filter by list of roles.
            return menus.Where(x => roles.Any(y => x.Roles.Contains("(" + y + ")")));
        }

        /// <summary>
        /// Get menu by roles asynchronously.
        /// </summary>
        /// <param name="roles">This list of roles.</param>
        /// <returns>This list of <see cref="MenuPermissionView"/> objects.</returns>
        public async Task<IEnumerable<MenuPermissionView>> GetMenuByRolesAsync(string[] roles)
        {
            // Get list of menu permissions from cache.
            var menus = await MenuPermissionRepository.Fetch()
                .AsNoTracking()
                .FromCacheAsync(_menuTags)
                .ConfigureAwait(false);

            // Filter by list of roles.
            return menus.Where(x => roles.Any(y => x.Roles.Contains("(" + y + ")")));
        }

        /// <summary>
        /// Get list of menus.
        /// </summary>
        /// <param name="cache">Cache flag (if true get from cache).</param>
        /// <returns>This list of <see cref="Menu"/> objects.</returns>
        public IEnumerable<Menu> GetMenus(bool cache = false)
        {
            // Get menu query objects without object tracking.
            var menus = MenuRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.RowStatus);
            
            // If cache was enabled then get from cache.
            return cache
                ? menus.FromCache(_menuTags)
                : menus;
        }
        
        /// <summary>
        /// Get list of menus from cache asynchronously.
        /// </summary>
        /// <returns>This list of <see cref="Menu"/> objects.</returns>
        public Task<IEnumerable<Menu>> GetMenusAsync()
        {
            // Get menu query objects without object tracking.
            var menus = MenuRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.RowStatus);

            // Cache if it new and return the cache if exist.
            return menus.FromCacheAsync(_menuTags);
        }

        /// <summary>
        /// Get list of menus from cache by menu group code.
        /// </summary>
        /// <param name="menuGroupCode">This menu group code.</param>
        /// <returns>This list of <see cref="Menu"/> objects.</returns>
        public IEnumerable<Menu> GetMenuByGroup(string menuGroupCode, bool includeEmptyUrl = false)
        {
            var menus = GetMenus(true);

            return menus.Where(x => x.MenuGroupCode == menuGroupCode && (includeEmptyUrl || (!includeEmptyUrl && !string.IsNullOrEmpty(x.Url)))).OrderBy(x => x.OrderIndex);
        }

        /// <summary>
        /// Get list of menus from cache by menu group code and roles.
        /// </summary>
        /// <param name="menuGroupCode">This menu group code.</param>
        /// <param name="roles">This list of roles.</param>
        /// <returns>This list of <see cref="MenuPermissionView"/> objects.</returns>
        public IEnumerable<MenuPermissionView> GetMenuByGroup(string menuGroupCode, string[] roles, bool includeEmptyUrl = false)
        {
            var menus = GetMenuByRoles(roles);

            return menus.Where(x => x.MenuGroupCode == menuGroupCode && (includeEmptyUrl || (!includeEmptyUrl && !string.IsNullOrEmpty(x.Url)))).OrderBy(x => x.OrderIndex);
        }

        /// <summary>
        /// Get menu from cache async by menu group code and string filter.
        /// </summary>
        /// <param name="menuGroupCode">Menu Group Code</param>
        /// <param name="endWith">End With String</param>
        /// <returns>Menu Object</returns>
        public async Task<Menu> GetMenuByGroupAsync(string menuGroupCode, string endWith)
        {
            var menus = await GetMenusAsync().ConfigureAwait(false);

            return menus.FirstOrDefault(x => x.MenuGroupCode == menuGroupCode && !string.IsNullOrEmpty(x.Url) && x.Url.EndsWith(endWith));
        }

        /// <summary>
        /// Get list of favourite menu asynchronously.
        /// </summary>
        /// <param name="noreg">This current user session noreg.</param>
        /// <returns>This list of <see cref="Menu"/> objects.</returns>
        public Task<List<Menu>> GetFavouriteMenusAsync(string noreg)
        {
            return FavouriteMenuRepository
                .Fetch()
                .AsNoTracking()
                .Where(x => x.NoReg == noreg && x.RowStatus)
                .OrderBy(x => x.MenuOrder)
                .Select(x => x.Menu)
                .Where(x => x.Visible && x.RowStatus)
                .ToListAsync();
        }

        public async Task<IEnumerable<IMenu>> GetSubMenuAsync(Guid id, string[] roles)
        {
            var cached = await GetMenuByRolesAsync(roles).ConfigureAwait(false);
            var subMenus = cached.Where(x => x.ParentId == id && x.Visible).OrderBy(x => x.OrderIndex);

            return subMenus;
        }

        public async Task<IEnumerable<MenuViewModel>> GetHomeMenusAsync(string noreg, string[] roles)
        {
            var cached = await GetMenuByRolesAsync(roles).ConfigureAwait(false);
            var parentMenus = cached.Where(x => !x.ParentId.HasValue && x.MenuGroupCode == "main" && x.Visible).OrderBy(x => x.OrderIndex);
            var favouriteMenus = await GetFavouriteMenusAsync(noreg).ConfigureAwait(false);
            var viewModel = new List<MenuViewModel>
            {
                new MenuViewModel("Favourite Menu", 0, favouriteMenus ?? new List<Menu>(), true),
                new MenuViewModel("Main Menu", 1, parentMenus)
            };

            return viewModel;
        }

        public async Task<IEnumerable<MenuViewModel>> GetFixedMenusAsync(string noreg, string[] roles)
        {
            var cached = await GetMenuByRolesAsync(roles).ConfigureAwait(false);
            var favouriteMenus = await GetFavouriteMenusAsync(noreg).ConfigureAwait(false);

            var dicts = cached.ToDictionary(x => x.Id);
            var categories = cached.Where(x => x.Visible && x.MenuGroupCode == "main" && x.ParentId.HasValue).GroupBy(x => x.ParentId.Value);
            var viewModel = new List<MenuViewModel>
            {
                new MenuViewModel("Favourite Menu", 0, favouriteMenus ?? new List<Menu>(), true)
            };

            foreach (var category in categories)
            {
                if (!dicts.ContainsKey(category.Key)) continue;

                var parent = dicts[category.Key];
                viewModel.Add(new MenuViewModel(parent.Title, parent.OrderIndex, category.OrderBy(x => x.OrderIndex)));
            }

            return viewModel.OrderBy(x => x.OrderIndex);
        }

        private bool MatchUrlAndQueryString(string url, string targetUrl)
        {
            var safeUrl = (url ?? string.Empty).ToLower();
            var safeTargetUrl = (targetUrl ?? string.Empty).ToLower();
            var sourceIndex = safeUrl.IndexOf('?');
            var targetIndex = safeTargetUrl.IndexOf('?');

            var sourceQueryString = HttpUtility.ParseQueryString(safeUrl);
            var targetQueryString = HttpUtility.ParseQueryString(safeTargetUrl);

            return safeUrl.Substring(0, sourceIndex < 0 ? safeUrl.Length : sourceIndex) == safeTargetUrl.Substring(0, targetIndex < 0 ? targetUrl.Length : targetIndex) && sourceQueryString.AllKeys.All(x => sourceQueryString[x] == targetQueryString[x]);
        }

        /// <summary>
        /// Get list of parent menus including it self
        /// </summary>
        /// <param name="url">Menu Url</param>
        /// <param name="queryString">Query String</param>
        /// <returns>List of Parent Menus</returns>
        public async Task<IEnumerable<Menu>> GetParentMenus(string url, string queryString = null)
        {
            var menus = await GetMenusAsync().ConfigureAwait(false);
            var menu = menus.FirstOrDefault(x => x.Url?.ToLower() == url.ToLower());
            if (!string.IsNullOrEmpty(queryString))
            {
                menu = menu ?? menus.FirstOrDefault(x => MatchUrlAndQueryString(x.Url, (url + queryString)));
                //menu = menu ?? menus.FirstOrDefault(x => x.Url?.ToLower() == (url.ToLower() + queryString.ToLower()));
            }

            if (menu == null)
            {
                return null;
            }

            var list = new List<Menu>();
            var current = menu;
            list.Add(current);

            while (current.ParentId.HasValue)
            {
                var parentId = current.ParentId;
                current = menus.FirstOrDefault(x => x.Id == parentId);
                list.Add(current);
            }
            
            return list;
        }

        /// <summary>
        /// Get menu by id.
        /// </summary>
        /// <param name="id">This menu id.</param>
        /// <returns>This <see cref="Menu"/> object.</returns>
        public Menu GetMenu(Guid id)
        {
            // Get menu by id without object tracking and return default if empty.
            return MenuRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.Id == id)
                .FirstOrDefaultIfEmpty();
        }

        /// <summary>
        /// Rename menu
        /// </summary>
        /// <param name="viewModel">This <see cref="RenameMenuViewModel"/> object.</param>
        /// <returns>True if success, false otherwise.</returns>
        public bool RenameMenu(RenameMenuViewModel viewModel)
        {
            // Get menu by id.
            var menu = MenuRepository.FindById(viewModel.Id);

            // Update menu title.
            menu.Title = viewModel.Title;

            // Clear menu cache.
            QueryCacheManager.ExpireTag(_menuTags);

            // Push pending changes into database and return the boolean result.
            return UnitOfWork.SaveChanges() > 0;
        }

        /// <summary>
        /// Toggle menu visibility.
        /// </summary>
        /// <param name="id">This menu id.</param>
        /// <returns>True if success, false otherwise.</returns>
        public bool ToggleMenu(Guid id)
        {
            // Get menu by id.
            var menu = MenuRepository.FindById(id);

            // Update menu visibility.
            menu.Visible = !menu.Visible;

            // Clear menu cache.
            QueryCacheManager.ExpireTag(_menuTags);

            // Push pending changes into database and return the boolean result.
            return UnitOfWork.SaveChanges() > 0;
        }

        /// <summary>
        /// Update or insert menu.
        /// </summary>
        /// <param name="menu">This <see cref="Menu"/> object</param>
        /// <returns>True if success, false otherwise.</returns>
        public bool UpsertMenu(Menu menu)
        {
            // Update or insert menu with specified list of properties to update.
            MenuRepository.Upsert<Guid>(menu, _menuProperties);

            // Clear menu cache.
            QueryCacheManager.ExpireTag(_menuTags);

            // Push pending changes into database and return the boolean result.
            return UnitOfWork.SaveChanges() > 0;
        }

        /// <summary>
        /// Set parent and menu order index.
        /// </summary>
        /// <param name="viewModel">This <see cref="ParentMenuViewModel"/> object.</param>
        /// <returns>True if success, false otherwise.</returns>
        public bool SetParent(ParentMenuViewModel viewModel)
        {
            // Create new stored procedure parameters.
            var parameters = new {
                // Get and set menu id.
                id = viewModel.Id,
                // Get and set menu parent id.
                parentId = viewModel.ParentId,
                // Get and set menu group code.
                menuGroupCode = viewModel.MenuGroupCode,
                // Get and set menu order index.
                orderIndex = viewModel.OrderIndex
            };

            // Execute stored procedure with specified parameters.
            UnitOfWork.UspQuery("dbo.SP_SET_PARENT_MENU", parameters);

            // Clear menu cache.
            QueryCacheManager.ExpireTag(_menuTags);

            // Push pending changes into database and return the boolean result.
            return UnitOfWork.SaveChanges() > 0;
        }

        /// <summary>
        /// Update favourite menu.
        /// </summary>
        /// <param name="noreg">This current user session noreg.</param>
        /// <param name="ids">This list of menu ids.</param>
        /// <returns>True if success, false otherwise.</returns>
        public bool UpdateFavouriteMenu(string noreg, Guid[] ids)
        {
            // Create output variable with false as default value.
            var output = false;

            // Create config service object.
            var configService = new ConfigService(UnitOfWork);

            // Get favourite menu max length from configuration.
            var config = configService.GetConfig("Menu.FavouriteMenuCount");

            // Set default value if not exist.
            var max = config != null ? int.Parse(config.ConfigValue) : 8;

            // Throw an exception if the number of favourite items was greater than the max value.
            Assert.ThrowIf(ids.Count() > max, "Cannot set favourite menus, maximum is " + max);

            // Begin transaction.
            UnitOfWork.Transact(() =>
            {
                // Set the default order to 1.
                var order = 1;

                // Delete all favourite menus by given noreg.
                FavouriteMenuRepository.Fetch()
                    .Where(x => x.NoReg == noreg)
                    .Delete();

                // Add new list of favourite menus into the repository.
                ids.ForEach(id => FavouriteMenuRepository.Add(FavouriteMenu.Create(noreg, id, order++)));

                // Push pending changes into database and save the boolean result into output variable.
                output = UnitOfWork.SaveChanges() > 0;
            });

            // Return the output variable.
            return output;
        }

        /// <summary>
        /// Change menu order.
        /// </summary>
        /// <param name="id">This menu id.</param>
        /// <param name="newOrderIndex">New order index.</param>
        /// <returns>True if success, false otherwise.</returns>
        public bool ChangeMenuOrder(Guid id, int newOrderIndex)
        {
            // Create output variable with false as default value.
            var output = false;

            // Begin transaction.
            UnitOfWork.Transact(() =>
            {
                // Get menu by id.
                var menu = MenuRepository.FindById(id);

                // Update order index with given new order index parameter.
                menu.OrderIndex = newOrderIndex;

                // Push pending changes into database and save the boolean result into output variable.
                output = UnitOfWork.SaveChanges() > 0;
            });

            // Return the output variable.
            return output;
        }

        /// <summary>
        /// Soft delete menu by id and its dependencies if any.
        /// </summary>
        /// <param name="id">This menu id.</param>
        /// <returns>True if success, false otherwise.</returns>
        public bool SoftDeleteMenu(Guid id)
        {
            // Get menu by id.
            var menu = MenuRepository.FindById(id);

            // Update the row status value to false.
            menu.RowStatus = false;

            // Clear menu cache.
            QueryCacheManager.ExpireTag(_menuTags);

            // Push pending changes into database and return the boolean result.
            return UnitOfWork.SaveChanges() > 0;
        }

        /// <summary>
        /// Delete menu by id and its dependencies if any.
        /// </summary>
        /// <param name="id">This menu id.</param>
        /// <returns>True if success, false otherwise.</returns>
        public bool DeleteMenu(Guid id)
        {
            // Mark given menu with specified id as deleted.
            MenuRepository.DeleteById(id);

            // Clear menu cache.
            QueryCacheManager.ExpireTag(_menuTags);

            // Push pending changes into database and return the boolean result.
            return UnitOfWork.SaveChanges() > 0;
        }
        #endregion

        #region Permission Area
        /// <summary>
        /// Get list of permissions.
        /// </summary>
        /// <param name="cache">Cache flag (if true get from cache).</param>
        /// <returns>This list of <see cref="Permission"/> objects.</returns>
        public IEnumerable<Permission> GetPermissions(bool cache = false)
        {
            // Get permission query objects without object tracking.
            var permissions = PermissionRepository.Fetch()
                .AsNoTracking()
                .OrderBy(x => x.PermissionKey);

            // If cache was enabled then get from cache.
            return cache
                ? permissions.FromCache(_tags)
                : permissions;
        }

        /// <summary>
        /// Get list of role permissions.
        /// </summary>
        /// <param name="cache">Cache flag (if true get from cache).</param>
        /// <returns>This list of <see cref="RolePermissionView"/> objects.</returns>
        public IEnumerable<RolePermissionView> GetRolePermissions(bool cache = false)
        {
            // Get role permission query objects without object tracking.
            var rolePermissions = RolePermissionReadonlyRepository.Fetch()
                .AsNoTracking();

            // If cache was enabled then get from cache.
            return cache
                ? rolePermissions.FromCache("role_permissions")
                : rolePermissions;
        }

        /// <summary>
        /// Get permission by id.
        /// </summary>
        /// <param name="id">This permission id.</param>
        /// <returns>This <see cref="Permission"/> object.</returns>
        public Permission GetPermission(Guid id)
        {
            // Get permission by id without object tracking and return default if empty.
            return PermissionRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.Id == id)
                .FirstOrDefaultIfEmpty();
        }

        /// <summary>
        /// Check whether roles has permission or not.
        /// </summary>
        /// <param name="permissionKey">This permission key.</param>
        /// <param name="roles">This list of roles.</param>
        /// <returns>True if roles has permission, false otherwise.</returns>
        public bool HasPermission(string permissionKey, string[] roles)
        {
            // Determine whether permission with given key and list of roles exist or not.
            return GetRolePermissions(true).Any(x => x.PermissionKey.ToLower() == permissionKey.ToLower() && roles.Contains(x.RoleKey));
        }

        /// <summary>
        /// Update or insert permission.
        /// </summary>
        /// <param name="permission">This <see cref="Permission"/> object.</param>
        /// <returns>True if success, false otherwise.</returns>
        public bool UpsertPermission(Permission permission)
        {
            // Update or insert permission with specified list of properties to update.
            PermissionRepository.Upsert<Guid>(permission, _permissionProperties);

            // Push pending changes into database and return the boolean result.
            return UnitOfWork.SaveChanges() > 0;
        }

        /// <summary>
        /// Soft delete permission by id and its dependencies if any.
        /// </summary>
        /// <param name="id">This permission id.</param>
        /// <returns>True if success, false otherwise.</returns>
        public bool SoftDeletePermission(Guid id)
        {
            // Create output variable with false as default value.
            var output = false;

            // Begin transcation.
            UnitOfWork.Transact(() =>
            {
                // Get permission by id.
                var permission = PermissionRepository.FindById(id);

                RolePermissionRepository.Fetch()
                    .Where(x => x.PermissionId == id)
                    .Update(x => new RolePermission { RowStatus = false });

                var menus = MenuRepository.Fetch().Where(x => x.PermissionId == id);
                menus.Update(x => new Menu { RowStatus = false });

                var ids = menus.Select(x => x.Id).ToList();

                FavouriteMenuRepository.Fetch()
                    .Where(x => ids.Contains(x.MenuId))
                    .Update(x => new FavouriteMenu { RowStatus = false });

                // Update row status value to false.
                permission.RowStatus = false;

                // Push pending changes into database and save the boolean result into output variable.
                output = UnitOfWork.SaveChanges() > 0;
            });

            // Return the output variable.
            return output;
        }

        /// <summary>
        /// Delete permission by id and its dependencies
        /// </summary>
        /// <param name="id">Permission Id</param>
        public bool DeletePermission(Guid id)
        {
            // Create output variable with false as default value.
            var output = false;

            UnitOfWork.Transact(() =>
            {
                //RolePermissionRepository.Fetch()
                //    .Where(x => x.PermissionId == id)
                //    .Delete();
                var rolePermissions = RolePermissionRepository.Fetch()
                        .Where(x => x.PermissionId == id)
                        .ToList(); // materialize query

                foreach (var rp in rolePermissions)
                {
                    RolePermissionRepository.Delete(rp);
                }

                //var menus = MenuRepository.Fetch().Where(x => x.PermissionId == id);

                //var ids = menus.Select(x => x.Id).ToList();

                //FavouriteMenuRepository.Fetch()
                //    .Where(x => ids.Contains(x.MenuId))
                //    .Delete();

                //menus.Delete();
                var menus = MenuRepository.Fetch()
                    .Where(x => x.PermissionId == id)
                    .ToList();

                var menuIds = menus.Select(x => x.Id).ToList();

                var favourites = FavouriteMenuRepository.Fetch()
                                    .Where(x => menuIds.Contains(x.MenuId))
                                    .ToList();
                foreach (var fav in favourites)
                {
                    FavouriteMenuRepository.Delete(fav);
                }

                foreach (var menu in menus)
                {
                    MenuRepository.Delete(menu);
                }

                PermissionRepository.DeleteById(id);

                // Push pending changes into database and save the boolean result into output variable.
                output = UnitOfWork.SaveChanges() > 0;
            });

            // Return the output variable.
            return output;
        }
        #endregion

        #region Role Area
        /// <summary>
        /// Get list of roles.
        /// </summary>
        /// <returns>This list of <see cref="Role"/> objects.</returns>
        public IEnumerable<Role> GetRoles()
        {
            return RoleRepository.FindAll();
        }

        /// <summary>
        /// Get list of permissions by role id
        /// </summary>
        /// <param name="id">Role Id</param>
        /// <returns>List of User Role Object</returns>
        public IEnumerable<RolePermissionStoredEntity> GetPermissions(Guid id)
        {
            return UnitOfWork.UdfQuery<RolePermissionStoredEntity>(new { roleId = id });
        }

        /// <summary>
        /// Get role by id
        /// </summary>
        /// <param name="id">Role Id</param>
        /// <returns>Role Object</returns>
        public Role GetRole(Guid id)
        {
            return RoleRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.Id == id)
                .FirstOrDefaultIfEmpty();
        }

        /// <summary>
        /// Update or insert role
        /// </summary>
        /// <param name="viewModel">Role View Model</param>
        public void UpsertRole(RoleViewModel viewModel)
        {
            var role = RoleRepository.FindById(viewModel.Id);

            UnitOfWork.Transact(() =>
            {
                if (role != null)
                {
                    role.Title = viewModel.Title;
                    role.Description = viewModel.Description;
                    role.RoleTypeCode = viewModel.RoleTypeCode;
                    role.RoleKey = viewModel.RoleKey;

                    RolePermissionRepository.Fetch().Where(x => x.RoleId == role.Id).Delete();
                }
                else
                {
                    role = new Role
                    {
                        RoleKey = viewModel.RoleKey,
                        Title = viewModel.Title,
                        Description = viewModel.Description,
                        RoleTypeCode = viewModel.RoleTypeCode
                    };

                    RoleRepository.Add(role);

                    UnitOfWork.SaveChanges();

                    viewModel.Id = role.Id;
                }

                if (viewModel.Permissions != null)
                {
                    foreach (var permission in viewModel.Permissions)
                    {
                        RolePermissionRepository.Add(new RolePermission { RoleId = role.Id, PermissionId = permission });
                    }
                }

                UnitOfWork.SaveChanges();

                QueryCacheManager.ExpireTag("role_permissions");
                QueryCacheManager.ExpireTag(_menuTags);
            }, System.Data.IsolationLevel.ReadUncommitted);
        }

        /// <summary>
        /// Soft delete role by id and its dependencies if any
        /// </summary>
        /// <param name="id">Role Id</param>
        public void SoftDeleteRole(Guid id)
        {
            UnitOfWork.Transact(() =>
            {
                var role = RoleRepository.FindById(id);

                AccessRoleRepository.Fetch().Where(x => x.RoleId == id).Update(x => new AccessRole { RowStatus = false });
                RolePermissionRepository.Fetch().Where(x => x.RoleId == id).Update(x => new RolePermission { RowStatus = false });
                UserRoleRepository.Fetch().Where(x => x.RoleId == id).Update(x => new UserRole { RowStatus = false });

                role.RowStatus = false;

                UnitOfWork.SaveChanges();

                QueryCacheManager.ExpireTag("role_permissions");
                QueryCacheManager.ExpireTag(_menuTags);
            });
        }

        /// <summary>
        /// Delete role by id and its dependencies if any
        /// </summary>
        /// <param name="id">Role Id</param>
        public void DeleteRole(Guid id)
        {
            UnitOfWork.Transact(() =>
            {
                //AccessRoleRepository.Fetch().Where(x => x.RoleId == id).Delete();
                //RolePermissionRepository.Fetch().Where(x => x.RoleId == id).Delete();
                //UserRoleRepository.Fetch().Where(x => x.RoleId == id).Delete();
                foreach (var ar in AccessRoleRepository.Fetch().Where(x => x.RoleId == id).ToList())
                    AccessRoleRepository.Delete(ar);

                foreach (var rp in RolePermissionRepository.Fetch().Where(x => x.RoleId == id).ToList())
                    RolePermissionRepository.Delete(rp);

                foreach (var ur in UserRoleRepository.Fetch().Where(x => x.RoleId == id).ToList())
                    UserRoleRepository.Delete(ur);

                RoleRepository.DeleteById(id);

                UnitOfWork.SaveChanges();

                QueryCacheManager.ExpireTag("role_permissions");
                QueryCacheManager.ExpireTag(_menuTags);
            });
        }
        #endregion

        #region Notification Area
        /// <summary>
        /// Read notifications by noreg asynchronously
        /// </summary>
        /// <param name="noreg">NoReg</param>
        /// <returns>Async Task Object</returns>
        public async Task ReadNotifications(string noreg)
        {
            var now = DateTime.Now;

            await NotificationRepository.Fetch()
                .Where(x => x.ToNoReg == noreg && x.RowStatus && x.ModifiedOn == null && (x.TriggerDate == null || x.TriggerDate <= now))
                .UpdateAsync(x => new Notification { ModifiedBy = noreg, ModifiedOn = DateTime.Now })
                .ConfigureAwait(false);

            UnitOfWork.SaveChanges();
        }

        /// <summary>
        /// Read notifications by noreg and type asynchronously
        /// </summary>
        /// <param name="noreg">NoReg</param>
        /// <param name="type">Type</param>
        /// <returns>Async Task Object</returns>
        public async Task ReadNotifications(string noreg, string type)
        {
            var now = DateTime.Now;

            await NotificationRepository.Fetch()
                .Where(x => x.ToNoReg == noreg && x.NotificationTypeCode == type && x.RowStatus && x.ModifiedOn == null && (x.TriggerDate == null || x.TriggerDate <= now))
                .UpdateAsync(x => new Notification { ModifiedBy = noreg, ModifiedOn = DateTime.Now })
                .ConfigureAwait(false);

            UnitOfWork.SaveChanges();
        }

        /// <summary>
        /// Get latest notifications by noreg
        /// </summary>
        /// <param name="noreg">NoReg</param>
        /// <param name="limit">Limit Count</param>
        /// <returns>List of Notifications Object</returns>
        public NotificationViewModel GetLatestNotifications(string noreg, int limit)
        {
            var now = DateTime.Now;

            var totalUnread = NotificationRepository.Count(x => x.ToNoReg == noreg && !x.ModifiedOn.HasValue && x.RowStatus && (x.TriggerDate == null || x.TriggerDate <= now));
            var latestNotifications = NotificationRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.ToNoReg == noreg && x.RowStatus)
                .OrderByDescending(x => x.CreatedOn)
                .Take(limit)
                .Select(x => new { x.Message, CreatedOn = x.CreatedOn.ToString("dd MMM yyyy - hh:mm") });

            var notificationViewModel = new NotificationViewModel { Total = totalUnread, Messages = latestNotifications };

            return notificationViewModel;
        }

        /// <summary>
        /// Get notification query by noreg
        /// </summary>
        /// <param name="noreg">NoReg</param>
        /// <returns>Notification Query</returns>
        public IQueryable<Notification> GetNotifications(string noreg)
        {
            var now = DateTime.Now;

            return NotificationRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.ToNoReg == noreg && x.RowStatus && (x.TriggerDate == null || x.TriggerDate <= now));
        }

        /// <summary>
        /// Get notification query by noreg and type
        /// </summary>
        /// <param name="noreg">NoReg</param>
        /// <returns>Notification Query</returns>
        public IQueryable<Notification> GetNotifications(string noreg, string type)
        {
            var now = DateTime.Now;

            return NotificationRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.ToNoReg == noreg && x.NotificationTypeCode == type && x.RowStatus && (x.TriggerDate == null || x.TriggerDate <= now));
        }

        /// <summary>
        /// Get dashboard view model asynchronously
        /// </summary>
        /// <param name="noreg">NoReg</param>
        /// <returns>Dashboard View Model Object</returns>
        public async Task<DashboardHomeViewModel> GetDashboardViewModel(string noreg)
        {
            try
            {
                UnitOfWork.UspQuery("SP_GENERATE_PROXY", new { keyDate = DateTime.Now, noreg = noreg });
            }
            catch
            {

            }

            var approvalService = new ApprovalService(UnitOfWork,null,null);
            var timeManagementService = new TimeManagementService(UnitOfWork, null);
            var now = DateTime.Now;
            var keyDate = now.Date;
            var user = await UserRepository.Fetch().AsNoTracking().FirstOrDefaultAsync(x => x.NoReg == noreg).ConfigureAwait(false);
            var totalNotice = await NotificationRepository.CountAsync(x => x.ToNoReg == noreg && x.NotificationTypeCode == "notice" && !x.ModifiedOn.HasValue && x.RowStatus && (x.TriggerDate == null || x.TriggerDate <= now)).ConfigureAwait(false);
            var totalTask = await DocumentApprovalRepository.CountAsync(x => x.CurrentApprover != null && x.CurrentApprover.Contains("(" + user.Username + ")") && x.RowStatus && x.VisibleInHistory).ConfigureAwait(false);
            var timeManagementDashboard = UnitOfWork.UdfQuery<TimeManagementDashboardStoredEntity>(new { noreg, keyDate });
            var timeManagement = UnitOfWork.SqlQuery<TimeManagementView>(new { NoReg = noreg, WorkingDate = keyDate }).FirstOrDefaultIfEmpty();
            var employeeLeave = UnitOfWork.SqlQuery<EmployeeLeave>(new { NoReg = noreg }).FirstOrDefaultIfEmpty();

            var objLeaveOnprogress = approvalService.GetInprogressDraftRequestDetails("absence").Where(x => x.CreatedBy == noreg).Select(x => JsonConvert.DeserializeObject<AbsenceViewModel>(x.ObjectValue));
            if (objLeaveOnprogress != null)
            {
                var totalCuti = 0;
                var totalCutiPanjang = 0;
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
                        .ToList();

                    int validDaysCount = workDays.Count;

                    if (item.ReasonType == "cuti")
                    {
                        totalCuti += validDaysCount;
                    }

                    if (item.ReasonType == "cutipanjang")
                    {
                        totalCutiPanjang += validDaysCount;
                    }
                }

                employeeLeave.AnnualLeave -= totalCuti;
                employeeLeave.LongLeave -= totalCutiPanjang;
            }


            return new DashboardHomeViewModel {
                TotalTask = totalTask,
                TotalNotice = totalNotice,
                ClockIn = timeManagement.WorkingTimeIn?.ToString("HH:mm"),
                ClockOut = timeManagement.WorkingTimeOut?.ToString("HH:mm"),
                ShiftCode = timeManagement.ShiftCode,
                AnnualLeave = employeeLeave.AnnualLeave,
                LongLeave = employeeLeave.LongLeave,
                TimeManagementDashboard = timeManagementDashboard
            };
        }

        /// <summary>
        /// Create notification
        /// </summary>
        /// <param name="notification">Notification Object</param>
        public void CreateNotification(Notification notification)
        {
            NotificationRepository.Add(notification);

            UnitOfWork.SaveChanges();
        }

        /// <summary>
        /// Create list of notifications
        /// </summary>
        /// <param name="notifications">List of Notificatoins Object</param>
        public void CreateNotifications(IEnumerable<Notification> notifications)
        {
            notifications.ForEach(x => NotificationRepository.Add(x));

            UnitOfWork.SaveChanges();
        }

        /// <summary>
        /// clear notifications by noreg asynchronously
        /// </summary>
        /// <param name="noreg">NoReg</param>
        /// <returns>Async Task Object</returns>
        public async Task ClearNotifications(string noreg)
        {
            var now = DateTime.Now;

            await NotificationRepository.Fetch()
                .Where(x => x.ToNoReg == noreg && x.RowStatus && (x.TriggerDate == null || x.TriggerDate <= now))
                .UpdateAsync(x => new Notification { RowStatus = false, ModifiedBy = noreg, ModifiedOn = now })
                .ConfigureAwait(false);

            UnitOfWork.SaveChanges();
        }
        #endregion

        #region Events Calendar Area
        /// <summary>
        /// Get events calendars query
        /// </summary>
        /// <returns>Events Calendar Query Object</returns>
        public IQueryable<EventsCalendar> GetEventsCalendars() => EventsCalendarRepository.Fetch();

        /// <summary>
        /// Get events calendars query
        /// </summary>
        /// <param name="year">Year Period</param>
        /// <returns>Events Calendar Query Object</returns>
        public IQueryable<EventsCalendar> GetEventsCalendars(int year) => GetEventsCalendars().Where(x => x.StartDate.Year == year || x.EndDate.Year == year);

        /// <summary>
        /// Get list of events calendars by year period
        /// </summary>
        /// <param name="year">Year Period</param>
        /// <returns>List of Events Calendars Object</returns>
        public async Task<IEnumerable<EventsCalendarViewModel>> GetEventsCalendar(int year)
        {
            var eventsCalendar = await EventsCalendarRepository.Fetch()
                .Where(x => x.StartDate.Year == year || x.EndDate.Year == year)
                .ToListAsync()
                .ConfigureAwait(false);

            return eventsCalendar.GroupBy(x => x.StartDate.Month, (x, val) => new EventsCalendarViewModel(x - 1, val.Select(y => new EventsViewModel(y.StartDate, y.EndDate, y.Description))));
        }

        /// <summary>
        /// Get list of events calendars by noreg and year period
        /// </summary>
        /// <param name="noreg">NoReg</param>
        /// <param name="year">Year Period</param>
        /// <returns>List of Events Calendars Object</returns>
        public IEnumerable<EventsCalendarViewModel> GetEventsCalendar(string noreg, int year)
        {
            var eventsCalendar = UnitOfWork.UdfQuery<EventsCalendarStoredEntity>(new { noreg, period = year });

            return eventsCalendar.GroupBy(x => x.Date.Month, (x, val) => new EventsCalendarViewModel(x - 1, val.Select(y => new EventsViewModel(y.Date, y.Date, y.EventDescription, y.ShiftCode))));
        }

        /// <summary>
        /// Get list of events calendars by work schedule rule and year period
        /// </summary>
        /// <param name="noreg">NoReg</param>
        /// <param name="year">Year Period</param>
        /// <returns>List of Events Calendars Object</returns>
        public IEnumerable<EventsCalendarViewModel> GetEventsCalendarByRule(string workScheduleRule, int year)
        {
            var workScheduleRepository = UnitOfWork.GetRepository<WorkSchedule>();
            var yearStr = year.ToString();

            var eventsCalendar =
                from ws in workScheduleRepository.Fetch().AsNoTracking()
                join es in EventsCalendarRepository.Fetch().AsNoTracking() on ws.Date equals es.StartDate into wes
                from es in wes.DefaultIfEmpty()
                where ws.WorkScheduleRule == workScheduleRule && ws.YearPeriod == yearStr
                select new { ws.Date, ws.ShiftCode, Description = es == null ? string.Empty : es.Description };

            return eventsCalendar.ToList().GroupBy(x => x.Date.Month, (x, val) => new EventsCalendarViewModel(x - 1, val.Select(y => new EventsViewModel(y.Date, y.Date, y.Description, y.ShiftCode))));
        }

        /// <summary>
        /// Get events calendar by id
        /// </summary>
        /// <param name="id">Events Calendar Id</param>
        /// <returns>Events Calendar Object</returns>
        public EventsCalendar GetEventsCalendar(Guid id) => EventsCalendarRepository.Find(x => x.Id == id).FirstOrDefaultIfEmpty();

        /// <summary>
        /// Update or insert events calendar
        /// </summary>
        /// <param name="eventsCalendar">Events Calendar Object</param>
        public void UpsertEventsCalendar(EventsCalendar eventsCalendar)
        {
            EventsCalendarRepository.Upsert<Guid>(eventsCalendar, _eventsCalendarProperties);

            UnitOfWork.SaveChanges();
        }

        /// <summary>
        /// Soft delete events calendar by id and its dependencies if any
        /// </summary>
        /// <param name="id">Events Calendar Id</param>
        public void SoftDeleteEventsCalendar(Guid id)
        {
            var eventsCalendar = EventsCalendarRepository.FindById(id);

            eventsCalendar.RowStatus = false;

            UnitOfWork.SaveChanges();
        }

        /// <summary>
        /// Delete events calendar by id and its dependencies if any
        /// </summary>
        /// <param name="id">Events Calendar Id</param>
        public void DeleteEventsCalendar(Guid id)
        {
            EventsCalendarRepository.DeleteById(id);

            UnitOfWork.SaveChanges();
        }
        #endregion

        #region Faskes Area
        /// <summary>
        /// Get list of faskes from cache async
        /// </summary>
        /// <param name="cache">Cache flag (if true get from cache).</param>
        /// <returns>List of faskes</returns>
        public IEnumerable<Faskes> GetFaskes(bool cache = false)
        {
            var dbitems = FaskesRepository.Fetch().Where(x => x.RowStatus);

            return cache ? dbitems.FromCache(_tags) : dbitems;
        }
        #endregion

        #region Vehicle Area
        /// <summary>
        /// Get list of Vehicle from cache async
        /// </summary>
        /// <param name="cache">Cache flag (if true get from cache).</param>
        /// <returns>List of Vehicle</returns>
        public IEnumerable<Vehicle> GetTypeCOP(bool cache = false)
        {
            var dbitems = VehicleRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.RowStatus && x.COP== true);

            return cache ? dbitems.FromCache(_tags) : dbitems;
        }

        public IEnumerable<Vehicle> GetTypeCPP(bool cache = false)
        {
            var dbitems = VehicleRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.RowStatus && x.CPP == true);

            return cache ? dbitems.FromCache(_tags) : dbitems;
        }

        public IEnumerable<object> GetTypeNameVehicleCop(string kelas, bool cache = false)
        {
            //var dbitems = VehicleRepository.Fetch()
            //    .AsNoTracking()
            //    .Where(x => x.RowStatus && x.COP == true)
            //    .OrderBy(x => x.TypeName)
            //    .Select(x => new { Typename = x.TypeName } )
            //    .Distinct();

            var dbitems = (from vr in VehicleRepository.Fetch()
                           join vrm in VehicleMatrixRepository.Fetch()
                           on vr.Id equals vrm.VehicleId
                           //where vrm.RowStatus && vr.COP == true
                           where vrm.RowStatus && vrm.SequenceClass == kelas && vr.COP == true
                           orderby vr.TypeName
                           select new { Typename = vr.TypeName }
                           ).Distinct();

            return cache ? dbitems.FromCache(_tags) : dbitems;
        }

        public IEnumerable<string> GetVehicleCopByNameTypeCode(string Noreg, Guid VehicleId)
        {
            var dbitems = (from v in VehicleRepository.Fetch()
                           join vm in VehicleMatrixRepository.Fetch()
                           on v.Id equals vm.VehicleId
                           join a in ActualOrganizationStructureRepository.Fetch()
                           on vm.SequenceClass equals a.EmployeeSubgroup
                           where a.NoReg == Noreg && v.COP == true
                           && v.Id == VehicleId 
                           orderby v.TypeName
                           select vm.Class).Distinct();

            return dbitems;
        }
        public IEnumerable<bool?> GetVehicleCopByNameTypeCode1(string kelas, Guid VehicleId)
        {
            var dbitems = (from  vm in VehicleMatrixRepository.Fetch()
                           where vm.Class == kelas
                           && vm.VehicleId == VehicleId
                           orderby vm.CreatedOn
                           select vm.IsUpgrade).Distinct();

            return dbitems;
        }
        public IEnumerable<object> GetTypeNameAllVehicleCop()
        {
            var output = VehicleRepository.Fetch()
                .AsNoTracking()

                .Where(x => x.COP == true && x.RowStatus)
                //.OrderBy(x => x.TypeName)
                .Select(x => new { Typename = x.TypeName });
                //.Distinct();
                


            return output;
        }

        public IEnumerable<object> GetTypeVehicleByModel(string employeeSubgroup, string model, string type, bool cache = false)
        {
            //var matrix = from vm in VehicleMatrixRepository.Fetch().AsNoTracking()
            // join v in VehicleRepository.Fetch().AsNoTracking() on vm.VehicleId equals v.Id
            //where vm.RowStatus && v.RowStatus && vm.SequenceClass == employeeSubgroup && v.TypeName == model && (type == "COP" ? v.COP == true : (type == "CPP" ? v.CPP == true : (type == "SCP" ? v.SCP == true : false) ) )
            //select new
            // {
            //  TypeVechicle = v.Type + " - " + v.ModelCode + " " + v.Suffix,
            //  VechicleId = v.Id
            // };
            /*            var matrix = from v in VehicleRepository.Fetch().AsNoTracking()
                        where v.RowStatus && v.TypeName == model && (type == "COP" ? v.COP == true : (type == "CPP" ? v.CPP == true : (type == "SCP" ? v.SCP == true : false) ) )*/
            var matrix = from vm in VehicleMatrixRepository.Fetch().AsNoTracking()
                         join v in VehicleRepository.Fetch().AsNoTracking() on vm.VehicleId equals v.Id
                         where vm.RowStatus && v.RowStatus && vm.SequenceClass == employeeSubgroup && v.TypeName == model && (type == "COP" ? v.COP == true : (type == "CPP" ? v.CPP == true : (type == "SCP" ? v.SCP == true : false)))
                         select new
             {
             TypeVechicle = v.Type + " - " + v.ModelCode + " " + v.Suffix,
              VechicleId = v.Id
             };

            return cache ? matrix.FromCache(_tags) : matrix;
        }

        public IEnumerable<object> GetTypeVehicles(string model, string type)
        {
            return VehicleRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.RowStatus && x.TypeName == model && (type == "COP" ? x.COP == true : (type == "CPP" ? x.CPP == true : (type == "SCP" ? x.SCP == true : false))))
                .Select(x => new {
                    TypeVechicle = x.Type + " - " + x.ModelCode + " " + x.Suffix,
                    VechicleId = x.Id
                });
        }

        public Vehicle GetTypeVehicleByKelas(string kelas, bool cache = false)
        {
            var dbitems = (from vr in VehicleRepository.Fetch()
                           join vrm in VehicleMatrixRepository.Fetch()
                           on vr.Id equals vrm.VehicleId
                           where vrm.RowStatus && vrm.SequenceClass == kelas
                           select vr);

            return dbitems.FirstOrDefault();
        }

        public Vehicle GetVehicleById(Guid id)
        {
            return VehicleRepository.Fetch().FirstOrDefault( x => x.Id == id);
        }

        public VehicleMatrix GetTypeVehicleMatrixByKelas(string kelas, bool cache = false)
        {
            var dbitems = (from vr in VehicleRepository.Fetch()
                           join vrm in VehicleMatrixRepository.Fetch()
                           on vr.Id equals vrm.VehicleId
                           where vrm.RowStatus && vrm.SequenceClass == kelas
                           select vrm);

            return dbitems.FirstOrDefault();
        }

        public VehicleMatrix GetTypeVehicleMatrixById(Guid id, string kelas, bool cache = false)
        {
            var dbitems = (from vr in VehicleRepository.Fetch()
                           join vrm in VehicleMatrixRepository.Fetch()
                           on vr.Id equals vrm.VehicleId
                           where vrm.RowStatus && vrm.VehicleId == id //&& vrm.SequenceClass == kelas
                           select vrm);

            return dbitems.FirstOrDefault();
        }

        public IEnumerable<object> GetById(Guid id, bool cache = false)
        {
            var dbitems = VehicleRepository.Fetch().Where(x => x.RowStatus && x.Id == id).Select(x =>
                new {
                    Id = x.Id,
                    FinalPrice = x.FinalPrice,
                    Type = x.Type
                });
            return cache ? dbitems.FromCache(_tags) : dbitems;
        }

        public IEnumerable<object> GetVehicleByClass(string kelas)
        {
            var dbitems = (from vr in VehicleRepository.Fetch()
                           join vrm in VehicleMatrixRepository.Fetch()
                           on vr.Id equals vrm.VehicleId
                           where vrm.RowStatus && vrm.SequenceClass == kelas
                           select new
                           {
                               FinalPrice = vr.FinalPrice
                           }).OrderByDescending(s => s.FinalPrice);

            return dbitems;
        }

        public IEnumerable<Vehicle> GetVehicleTypesByModel(string model, bool cache = false)
        {
            var dbitems = VehicleRepository.Fetch().Where(x => x.RowStatus && x.TypeName == model);

            return cache ? dbitems.FromCache(_tags) : dbitems;
        }

        public IEnumerable<object> GetVehicleCpp(string Noreg)
        {
            var dbitems = (from v in VehicleRepository.Fetch()
                     join vm in VehicleMatrixRepository.Fetch()
                     on v.Id equals vm.VehicleId
                     join a in ActualOrganizationStructureRepository.Fetch()
                     on vm.SequenceClass equals a.EmployeeSubgroup
                     where a.NoReg == Noreg && v.CPP == true
                     orderby v.TypeName
                     select v.TypeName).Distinct();
                    
            return dbitems;
        }

        public IEnumerable<object> GetVehicleScp(string Noreg)
        {
            var dbitems = (from v in VehicleRepository.Fetch()
                           join vm in VehicleMatrixRepository.Fetch()
                           on v.Id equals vm.VehicleId
                           join a in ActualOrganizationStructureRepository.Fetch()
                           on vm.SequenceClass equals a.EmployeeSubgroup
                           where a.NoReg == Noreg && v.SCP == true
                           orderby v.TypeName
                           select new { Typename = v.TypeName }).Distinct();

            return dbitems;
        }
        #endregion

        #region Absence Area
        /// <summary>
        /// Get list of faskes from cache async
        /// </summary>
        /// <param name="cache">Cache flag (if true get from cache).</param>
        /// <returns>List of faskes</returns>
        public IEnumerable<Absence> GetCategoryAbsence(bool cache = false)
        {
            var dbitems = AbsenceRepository.Fetch().Where(x => x.RowStatus).OrderBy(o => o.CodePresensi);

            return cache ? dbitems.FromCache(_tags) : dbitems;
        }

        public Absence GetAbsenceById(Guid Id)
        {
            var dbitems = AbsenceRepository.Fetch().Where(x => x.RowStatus && x.Id == Id).FirstOrDefault();

            return dbitems;
        }

        public IQueryable<Absence> GetAbsenceQuery()
        {
            var dbitems = AbsenceRepository.Fetch().AsNoTracking();

            return dbitems;
        }

        public IEnumerable<Absence> GetAbsenceByPlan(bool IsPlan)
        {
            var dbitems = AbsenceRepository.Fetch()
                .Where(x => x.RowStatus && ((x.AbsenceType != "default" && x.AbsenceType != "cutihamil") || string.IsNullOrEmpty(x.AbsenceType))
                && (x.Planning == IsPlan || x.Unplanning == !IsPlan)).OrderBy(o => o.CodePresensi);

            return dbitems;
        }
        #endregion

        #region Bank Area
        /// <summary>
        /// Get list of Bank from cache async
        /// </summary>
        /// <param name="cache">Cache flag (if true get from cache).</param>
        /// <returns>List of Bank</returns>
        public IEnumerable<Bank> GetBank(bool cache = false)
        {
            var dbitems = BankRepository.Fetch().Where(x => x.RowStatus);

            return cache ? dbitems.FromCache(_tags) : dbitems;
        }

        public IEnumerable<Bank> GetBankDistict(bool cache = false)
        {
            //var dbitems = BankRepository.Fetch().GroupBy(x => x.BankName).Select(y => y.First()).Where(x => x.RowStatus);
            var dbitems = BankRepository.Fetch()
                         .Where(x => x.RowStatus)
                         .GroupBy(x => x.BankName)
                         .Select(g => g.OrderBy(x => x.Id).FirstOrDefault());

            return cache ? dbitems.FromCache(_tags) : dbitems;
        }
        #endregion

        #region Hospital Area
        /// <summary>
        /// Get list of Bank from cache async
        /// </summary>
        /// <param name="cache">Cache flag (if true get from cache).</param>
        /// <returns>List of Hospital</returns>
        public IEnumerable<Hospital> GetHospital(bool cache = false)
        {
            var dbitems = HospitalRepository.Fetch().Where(x => x.RowStatus);

            return cache ? dbitems.FromCache(_tags) : dbitems;
        }
        #endregion

        #region Bpkb Area
        /// <summary>
        /// Get list of Bpkb from cache async
        /// </summary>
        /// <param name="cache">Cache flag (if true get from cache).</param>
        /// <returns>List of Bpkb</returns>
        public IEnumerable<Bpkb> GetBpkb(bool cache = false)
        {
            var dbitems = BpkbRepository.Fetch().Where(x => x.RowStatus);

            return cache ? dbitems.FromCache(_tags) : dbitems;
        }
        #endregion

        #region Guideline Area
        /// <summary>
        /// Get guideline by id
        /// </summary>
        /// <param name="id">Guideline Id</param>
        /// <returns>Guideline Object</returns>
        public Guideline GetGuideline(Guid id)
        {
            return GetGuidelinesQuery()
                .Where(x => x.Id == id)
                .FirstOrDefaultIfEmpty();
        }

        public IQueryable<Guideline> GetGuidelinesQuery()
        {
            return GuidelineRepository.Fetch()
                .AsNoTracking();
        }

        public async Task<IEnumerable<Guideline>> GetGuidelines()
        {
            var now = DateTime.Now.Date;

            return await GetGuidelinesQuery()
                .Where(x => x.StartDate <= now && x.EndDate >= now)
                .ToListAsync();
        }

        public void UpsertGuideline(Guideline guideline)
        {
            GuidelineRepository.Upsert<Guid>(guideline, _guidelineProperties);

            UnitOfWork.SaveChanges();
        }

        /// <summary>
        /// Soft delete guideline by id and its dependencies if any
        /// </summary>
        /// <param name="id">Guideline Id</param>
        public void SoftDeleteGuideline(Guid id)
        {
            UnitOfWork.Transact(() =>
            {
                var guideline = GuidelineRepository.FindById(id);

                guideline.RowStatus = false;

                UnitOfWork.SaveChanges();
            });
        }

        /// <summary>
        /// Delete guideline by id and its dependencies
        /// </summary>
        /// <param name="id">Guideline Id</param>
        public void DeleteGuideline(Guid id)
        {
            UnitOfWork.Transact(() =>
            {
                GuidelineRepository.DeleteById(id);

                UnitOfWork.SaveChanges();
            });
        }
        #endregion

        #region Language Area
        /// <summary>
        /// Get list of languages from cache
        /// </summary>
        /// <returns>List of Languages Object</returns>
        public Dictionary<string, string> GetLanguages()
        {
            // Get list of languages and stored it to cache and group by culture code and key.
            //using (var dbContext = new MainDbContext())
            //{
            //    var tes = dbContext.Languages.ToList();
            //}
            var languages = LanguageRepository.Fetch()
                .AsNoTracking()
                .FromCache("language")
                .GroupBy(x => x.CultureCode + "|" + x.TranslateKey)
                .Select(x => new { x.Key, Value = x.First().TranslateValue });

            // Return language in dictionary.
            return languages.ToDictionary(x => x.Key, x => x.Value);
        }

        /// <summary>
        /// Determine whether language with specified culture and key exist or not except language with given id.
        /// </summary>
        /// <param name="id">This language id.</param>
        /// <param name="cultureCode">This culture code.</param>
        /// <param name="translateKey">This translate key.</param>
        /// <returns>True if exist, false otherwise.</returns>
        public bool CheckIfLangugeKeyExist(Guid id, string cultureCode, string translateKey)
        {
            // Check whether language with given culture and key exist or not.
            return LanguageRepository.Fetch()
                .AsNoTracking()
                .Any(x => x.Id != id && x.CultureCode == cultureCode && x.TranslateKey == translateKey);
        }

        /// <summary>
        /// Get language by id.
        /// </summary>
        /// <returns>This <see cref="Language"/> object.</returns>
        public Language GetLanguage(Guid id)
        {
            var languages = LanguageRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.Id == id)
                .FirstOrDefaultIfEmpty();

            return languages;
        }

        /// <summary>
        /// Update or insert language.
        /// </summary>
        /// <param name="language">This <see cref="Language"/> object.</param>
        public void UpsertLanguage(Language language)
        {
            LanguageRepository.Upsert<Guid>(language, _languageProperties);

            UnitOfWork.SaveChanges();
        }

        /// <summary>
        /// Get languages query.
        /// </summary>
        /// <returns>This <see cref="IQueryable{Language}"/> objects.</returns>
        public IQueryable<Language> GetLanguagesQuery()
        {
            return LanguageRepository.Fetch().AsNoTracking();
        }

        /// <summary>
        /// Soft delete language by id and its dependencies if any.
        /// </summary>
        /// <param name="id">This language id.</param>
        public void SoftDeleteLanguage(Guid id)
        {
            UnitOfWork.Transact(() =>
            {
                var language = LanguageRepository.FindById(id);

                language.RowStatus = false;

                UnitOfWork.SaveChanges();
            });
        }

        /// <summary>
        /// Delete language by id and its dependencies if any.
        /// </summary>
        /// <param name="id">This language id.</param>
        public void DeleteLanguage(Guid id)
        {
            UnitOfWork.Transact(() =>
            {
                LanguageRepository.DeleteById(id);

                UnitOfWork.SaveChanges();
            });
        }
        #endregion

        #region Email Template Area
        /// <summary>
        /// Get list of email templates
        /// </summary>
        /// <returns>List of Email Templates</returns>
        public IQueryable<EmailTemplate> GetEmailTemplates()
        {
            return EmailTemplateRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.RowStatus);
        }

        /// <summary>
        /// Get email template by id
        /// </summary>
        /// <param name="id">Email Template Id</param>
        /// <returns>Email Template Object</returns>
        public EmailTemplate GetEmailTemplate(Guid id)
        {
            return GetEmailTemplates().Where(x => x.Id == id)
                .FirstOrDefaultIfEmpty();
        }

        /// <summary>
        /// Get email template by key
        /// </summary>
        /// <param name="key">Email Template Key</param>
        /// <returns>Email Template Object</returns>
        public EmailTemplate GetEmailTemplate(string key) => EmailTemplateRepository.Find(x => x.MailKey.ToLower() == key.ToLower()).FirstOrDefault();

        /// <summary>
        /// Check whether email template is exist by key
        /// </summary>
        /// <param name="key">Email Template Key</param>
        /// <returns>True if exist, false otherwise</returns>
        public bool HasEmailTemplate(string key) => EmailTemplateRepository.Fetch().AsNoTracking().Any(x => x.MailKey.ToLower() == key.ToLower());

        /// <summary>
        /// Update or insert email template
        /// </summary>
        /// <param name="emailTemplate">Email Template Object</param>
        public void UpsertEmailTemplate(EmailTemplate emailTemplate)
        {
            EmailTemplateRepository.Upsert<Guid>(emailTemplate, _emailTemplateProperties);

            UnitOfWork.SaveChanges();
        }

        /// <summary>
        /// Soft delete email template by id and its dependencies if any
        /// </summary>
        /// <param name="id">Email Template Id</param>
        public void SoftDeleteEmailTemplate(Guid id)
        {
            UnitOfWork.Transact(() =>
            {
                var emailTemplate = EmailTemplateRepository.FindById(id);

                emailTemplate.RowStatus = false;

                UnitOfWork.SaveChanges();
            });
        }

        /// <summary>
        /// Delete email template by id and its dependencies if any
        /// </summary>
        /// <param name="id">Email Template Id</param>
        public void DeleteEmailTemplate(Guid id)
        {
            UnitOfWork.Transact(() =>
            {
                EmailTemplateRepository.DeleteById(id);

                UnitOfWork.SaveChanges();
            });
        }
        #endregion

        #region Mail Queue Area
        public IQueryable<MailQueue> GetMailQueues()
        {
            return MailQueueRepository.Fetch()
                .AsNoTracking();
        }

        public MailQueueSummaryViewModel GetMailQueueSummary()
        {
            var now = DateTime.Now;
            var currentYear = now.Year;
            var currentMonth = now.Month;

            var summary = MailQueueRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.CreatedOn.Year == currentYear)
                .GroupBy(x => x.MailStatusCode)
                .Select(x => new { x.Key, Total = x.Count() });

            var totalPending = summary.FirstOrDefault(x => x.Key == "pending")?.Total ?? 0;
            var totalSent = summary.FirstOrDefault(x => x.Key == "sent")?.Total ?? 0;

            var newSummary = MailQueueRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.CreatedOn.Year == currentYear && x.CreatedOn.Month == currentMonth)
                .GroupBy(x => x.MailStatusCode)
                .Select(x => new { x.Key, Total = x.Count() });

            var totalNewPending = newSummary.FirstOrDefault(x => x.Key == "pending")?.Total ?? 0;
            var totalNewSent = newSummary.FirstOrDefault(x => x.Key == "sent")?.Total ?? 0;
            var totalNew = totalNewPending + totalNewSent;

            var average = totalNew / now.Day;
            var pendingTaskRepository = UnitOfWork.GetRepository<PendingTaskView>();
            var pendingCount = pendingTaskRepository.Fetch().AsNoTracking().Count();

            var mailQueueSummary = new MailQueueSummaryViewModel(totalPending, totalSent, totalNewPending, totalNewSent, average, pendingCount, 0);

            return mailQueueSummary;
        }

        public void CreateMailQueue(MailQueue mailQueue)
        {
            MailQueueRepository.Add(mailQueue);

            UnitOfWork.SaveChanges();
        }

        public void CreateMailQueues(IEnumerable<MailQueue> mailQueues)
        {
            mailQueues.ForEach(x => MailQueueRepository.Add(x));

            UnitOfWork.SaveChanges();
        }

        public IEnumerable<EmailStoredEntity> GetEmails(string filter, string type)
        {
            return UnitOfWork.UdfQuery<EmailStoredEntity>(new { filter, type });
        }
        #endregion

        #region Allowance Detail
        public decimal GetAllowanceAmount(string Noreg, string allowanceType, string allowanceSubType = null)
        {
            var data = (from aoe in ActualOrganizationStructureRepository.Fetch()
                        join np in EmployeeSubgroupNPRepository.Fetch()
                        on aoe.EmployeeSubgroup equals np.EmployeeSubgroup
                        from ad in AllowanceDetailRepository.Fetch()
                        where aoe.NoReg == Noreg && (np.NP >= ad.ClassFrom && np.NP <= ad.ClassTo) && ad.Type == allowanceType && (ad.SubType == allowanceSubType || string.IsNullOrEmpty(allowanceSubType))
                        select ad.Ammount).FirstOrDefault();

            return data;
        }

        public object GetInfoAllowance(string Noreg, string allowanceType, string allowanceSubType = null)
        {
            var data = (from aoe in ActualOrganizationStructureRepository.Fetch()
                       join np in EmployeeSubgroupNPRepository.Fetch()
                       on aoe.EmployeeSubgroup equals np.EmployeeSubgroup
                       from ts in PersonalDataTaxStatusRepository.Fetch()
                       from ad in AllowanceDetailRepository.Fetch()
                       where aoe.NoReg == Noreg && np.RowStatus && (ts.NoReg == Noreg && ts.RowStatus) && ts.StartDate <= DateTime.Now && ts.EndDate >= DateTime.Now &&
                       (np.NP >= ad.ClassFrom && np.NP <= ad.ClassTo) && ad.Type == allowanceType && (ad.SubType == allowanceSubType || string.IsNullOrEmpty(allowanceSubType))
                       select new
                       {
                           Name = aoe.Name,
                           Kelas = aoe.EmployeeSubgroupText,
                           NP = np.NP,
                           Ammount = ad.Ammount,
                           TaxStatus = ts.TaxStatus
                       }).FirstOrDefault();

            return data;
        }

        public IEnumerable<object> GetListInfoAllowance(string Noreg, string allowanceType, string allowanceSubType = null)
        {
            var data = (from aoe in ActualOrganizationStructureRepository.Fetch()
                        join np in EmployeeSubgroupNPRepository.Fetch()
                        on aoe.EmployeeSubgroup equals np.EmployeeSubgroup
                        from ad in AllowanceDetailRepository.Fetch()
                        where aoe.NoReg == Noreg && np.RowStatus && (np.NP >= ad.ClassFrom && np.NP <= ad.ClassTo) && ad.Type == allowanceType && (ad.SubType == allowanceSubType || string.IsNullOrEmpty(allowanceSubType))
                        select new
                        {
                            Name = aoe.Name,
                            Kelas = aoe.EmployeeSubgroupText,
                            NP = np.NP,
                            Ammount = ad.Ammount,
                            SubType = ad.SubType
                        });

            return data;
        }
        #endregion

        #region ARS Matrix
        public IEnumerable<ArsMatrixViewModel> GetArsMatrices()
        {
            var set = UnitOfWork.GetRepository<JsonCategory>();

            var list = set.Fetch()
                .AsNoTracking()
                .Where(x => x.Category == "ArsMatrix")
                .OrderBy(x => x.OrderIndex)
                .ToList()
                .Select(x => new ArsMatrixViewModel
                {
                    Title = x.Title,
                    Items = JsonConvert.DeserializeObject<ArsMatrixItemViewModel[]>(x.JsonValues)
                });

            return list;
        }

        public IEnumerable<ArsMatrixDetailStoredEntity> GetArsMatrix(string key)
        {
            return UnitOfWork.UspQuery<ArsMatrixDetailStoredEntity>(new { key });
        }
        #endregion

        #region Proxy Log Area
        /// <summary>
        /// Get list of proxy logs.
        /// </summary>
        /// <returns>List of Proxy Logs</returns>
        public IQueryable<ProxyLog> GetProxyLogs()
        {
            return ProxyLogRepository.Fetch()
                .AsNoTracking();
        }

        /// <summary>
        /// Get proxy log by id
        /// </summary>
        /// <param name="id">Proxy Log Id</param>
        /// <returns>Proxy Log Object</returns>
        public ProxyLog GetProxyLog(Guid id)
        {
            return GetProxyLogs().Where(x => x.Id == id)
                .FirstOrDefaultIfEmpty();
        }

        /// <summary>
        /// Update or insert proxy log
        /// </summary>
        /// <param name="proxyLog">Proxy Log Object</param>
        public void UpsertProxyLog(ProxyLog proxyLog)
        {
            ProxyLogRepository.Attach(proxyLog, _proxyLogProperties);

            UnitOfWork.SaveChanges();
        }

        /// <summary>
        /// Soft delete proxy log by id and its dependencies if any
        /// </summary>
        /// <param name="id">Proxy Log Id</param>
        public void SoftDeleteProxyLog(Guid id)
        {
            UnitOfWork.Transact(() =>
            {
                var proxyLo = ProxyLogRepository.FindById(id);

                proxyLo.RowStatus = false;

                UnitOfWork.SaveChanges();
            });
        }

        /// <summary>
        /// Delete proxy log by id and its dependencies if any
        /// </summary>
        /// <param name="id">Proxy Log Id</param>
        public void DeleteProxyLog(Guid id)
        {
            ProxyLogRepository.DeleteById(id);

            UnitOfWork.SaveChanges();
        }
        #endregion

        #region Others Area
        /// <summary>
        /// Get list of printout by document approval id.
        /// </summary>
        /// <param name="documentApprovalId">This document approval id.</param>
        /// <returns>This list of <see cref="PrintOutEntity"/> objects.</returns>
        public IEnumerable<PrintOutEntity> GetPrintOut(Guid documentApprovalId)
        {
            return UnitOfWork.UspQuery<PrintOutEntity>(new { documentApprovalId });
        }

        /// <summary>
        /// Create document approval histories for given list of document approval history objects.
        /// </summary>
        /// <param name="documentApprovalHistories">This list of document approval history objects.</param>
        public void CreateDocumentApprovalHistory(IEnumerable<DocumentApprovalHistory> documentApprovalHistories)
        {
            foreach (var documentApprovalHistory in documentApprovalHistories)
            {
                DocumentApprovalHistoryRepository.Add(documentApprovalHistory);
            }

            UnitOfWork.SaveChanges();
        }

        /// <summary>
        /// Get list of distinct email from document tracking approval by document approval id.
        /// </summary>
        /// <param name="documentApprovalId">This document approval id.</param>
        /// <param name="noregs">List of exclude noreg.</param>
        /// <param name="checkForAction">Check whether approval action with null value should be included or not.</param>
        /// <returns>List of distinct email.</returns>
        public IEnumerable<string> GetEmailsFromTrackingApprovals(Guid documentApprovalId, string[] noregs, bool checkForAction = false)
        {
            //var query = from ta in TrackingApprovalReadonlyRepository.Fetch().AsNoTracking()
            //            join u in UserRepository.Fetch().AsNoTracking() on ta.NoReg equals u.NoReg
            //            where !noregs.Contains(ta.NoReg) && ta.DocumentApprovalId == documentApprovalId && (!checkForAction || ta.ApprovalActionCode != null)
            //            select u.Email;

            // Ambil semua data dulu dari DB (memory)
            var taList = TrackingApprovalReadonlyRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.DocumentApprovalId == documentApprovalId && (!checkForAction || x.ApprovalActionCode != null))
                .ToList();

            var userList = UserRepository.Fetch()
                .AsNoTracking()
                .ToList();

            // Filter menggunakan Join dan pengecekan manual
            var query = from ta in taList
                        join u in userList on ta.NoReg equals u.NoReg
                        where !noregs.Any(n => n == ta.NoReg) // ganti Contains dengan Any
                        select u.Email;

            return query.Distinct()
                .ToList();
        }

        /// <summary>
        /// Generate time management absent records between two dates.
        /// </summary>
        /// <param name="startDate">This start date.</param>
        /// <param name="endDate">This end date.</param>
        /// <param name="noregs">This list of employee noreg.</param>
        /// <returns>This asynchronous operation.</returns>
        public Task GenerateRangeProxy(DateTime startDate, DateTime endDate, string[] noregs)
        {
            var configService = new ConfigService(UnitOfWork);
            var offset = configService.GetConfigValue("Core.RangeProxyOffset", 30, true);
            var diff = (endDate - startDate).TotalDays;

            Assert.ThrowIf(diff < 0, "End date must be greater than start date");
            Assert.ThrowIf(diff > offset, $"The different days cannot be greater than {offset}");

            foreach (var noreg in noregs)
            {
                UnitOfWork.UspQuery("dbo.SP_GENERATE_RANGE_PROXY", new { startDate, endDate, noreg });
            }

            return Task.FromResult(UnitOfWork.SaveChanges() > 0);
        }

        /// <summary>
        /// Update document approval tracking.
        /// </summary>
        /// <returns>This asynchronous operation.</returns>
        public Task UpdateDocumentApproval()
        {
            UnitOfWork.UspQuery("dbo.SP_UPDATE_DOCUMENT_APPROVALS");

            return Task.FromResult(UnitOfWork.SaveChanges() > 0);
        }

        /// <summary>
        /// Update spkl document approval tracking.
        /// </summary>
        /// <returns>This asynchronous operation.</returns>
        public Task UpdateSpklDocumentApproval(string[] noregs)
        {
            foreach (var noreg in noregs)
            {
                UnitOfWork.UspQuery("dbo.SP_UPDATE_SPKL_DOCUMENT_APPROVALS", new { cretor = noreg });
            }

            UnitOfWork.UspQuery("dbo.SP_UPDATE_WRONG_APPROVERS");

            return Task.FromResult(UnitOfWork.SaveChanges() > 0);
        }
        #endregion

        #region User Activity Log Area
        /// <summary>
        /// Get list of user activity logs.
        /// </summary>
        /// <returns>This list of <see cref="UserActivityLog"/> objects.</returns>
        public IQueryable<UserActivityLog> GetUserActivityLogs(Expression<Func<UserActivityLog, bool>> expression)
        {
            // Get list of user activity logs without object tracking.
            return UserActivityLogRepository.Fetch()
                .AsNoTracking()
                .Where(expression);
        }

        /// <summary>
        /// Update or insert user activity log.
        /// </summary>
        /// <param name="noreg">This current user session noreg.</param>
        /// <param name="logTypeCode">This log type code.</param>
        /// <param name="description">This log description.</param>
        /// <param name="jsonParams">This log json params if any.</param>
        /// <returns>True if success, false otherwise.</returns>
        public bool CreateUserActivityLog(string noreg, string logTypeCode, string description, string jsonParams = null)
        {
            // Create new user activity log data.
            var userActivityLog = new UserActivityLog
            {
                // Get and set noreg.
                NoReg = noreg,
                // Get and set log type code.
                LogTypeCode = logTypeCode,
                // Get and set log description.
                Description = description,
                // Get and set log json params.
                JsonParams = jsonParams
            };

            // Add user activity log into repository.
            UserActivityLogRepository.Add(userActivityLog);

            // Push insert operation and return the boolean result.
            return UnitOfWork.SaveChanges() > 0;
        }

        /// <summary>
        /// Soft delete user activity log by id and its dependencies if any.
        /// </summary>
        /// <param name="id">This user activity log id.</param>
        /// <returns>True if success, false otherwise.</returns>
        public bool SoftDeleteUserActivityLog(Guid id)
        {
            // Get user activity log by id.
            var userActivityLog = UserActivityLogRepository.FindById(id);

            // Update row status to false.
            userActivityLog.RowStatus = false;

            // Push update operation.
            return UnitOfWork.SaveChanges() > 0;
        }

        /// <summary>
        /// Delete user activity log by id and its dependencies if any.
        /// </summary>
        /// <param name="id">This user activity log id.</param>
        /// <returns>True if success, false otherwise.</returns>
        public bool DeleteUserActivityLog(Guid id)
        {
            // Delete user activity log by id.
            UserActivityLogRepository.DeleteById(id);

            // Push delete opeartion.
            return UnitOfWork.SaveChanges() > 0;
        }
        #endregion

        #region Vaccine Area
        /// <summary>
        /// Get list of Bank from cache async
        /// </summary>
        /// <param name="cache">Cache flag (if true get from cache).</param>
        /// <returns>List of Vaccine</returns>
        public IEnumerable<Vaccine> GetVaccine(bool cache = false)
        {
            var dbitems = VaccineRepository.Fetch().Where(x => x.RowStatus);

            return cache ? dbitems.FromCache(_tags) : dbitems;
        }
        #endregion
        #region User Hash Area
        /// <summary>
        /// Get list of user hashes.
        /// </summary>
        /// <returns>This list of <see cref="UserHash"/> objects.</returns>
        public IQueryable<object> GetUserHashes()
        {
            // Create user hash repository object.
            var set = UnitOfWork.GetRepository<UserHash>();

            // Create user repository object.
            var userSet = UnitOfWork.GetRepository<User>();

            var query = from us in userSet.Fetch().AsNoTracking()
                        join uh in set.Fetch().AsNoTracking() on us.NoReg equals uh.NoReg
                        select new
                        {
                            uh.Id,
                            uh.NoReg,
                            us.Name,
                            uh.TypeCode,
                            uh.HashValue,
                            uh.CreatedBy,
                            uh.CreatedOn,
                            uh.ModifiedBy,
                            uh.ModifiedOn,
                            uh.RowStatus
                        };

            return query;
        }

        /// <summary>
        /// Validate user hash by noreg and type code.
        /// </summary>
        /// <param name="noreg">This current user session noreg.</param>
        /// <param name="typeCode">This user hash type code.</param>
        /// <param name="inputString">This clean input string to be hashed</param>
        /// <returns>True if valid, false otherwise.</returns>
        public bool ValidHash(string noreg, string typeCode, string inputString, string configurationPassword)
        {
            // Get and set hash key.
            var key = noreg;

            // Create user hash repository object.
            var set = UnitOfWork.GetRepository<UserHash>();

            // Get user hash by noreg and type code.
            var userHash = set.Fetch()
                .FirstOrDefault(x => x.NoReg == noreg && x.TypeCode == typeCode);

            // Create temporary variable for hash to be compare.
            var hashToBeCompare = string.Empty;

            // If user hash object is not null then update the hash to be compare to user hash value.
            if (userHash != null)
            {
                // Update temporary variable with user hash value.
                hashToBeCompare = userHash.HashValue;
            }
            // Else get default user hash from configuration.
            else
            {
                // Create config service object.
                var configService = new ConfigService(UnitOfWork);

                // Create personal data service object.
                var personalDataService = new PersonalDataService(UnitOfWork, null);

                // Get personal data common attribute by noreg.
                var personalDataCommonAttribute = personalDataService.GetPersonalDataAttribute(noreg);

                // Create new parameters.
                var parameters = new Dictionary<string, object>
                {
                    // Get and set noreg.
                    ["noreg"] = noreg,
                    // Get and set NIK.
                    ["nik"] = personalDataCommonAttribute != null
                        ? (string.IsNullOrEmpty(personalDataCommonAttribute.Nik) ? noreg
                        : personalDataCommonAttribute.Nik) : noreg
                };

                // Get default user hash from configuration with given parameters.
                var defaultInputString = StringHelper.Format(configService.GetConfigValue(configurationPassword, string.Empty), parameters);

                // Encrypt the hash.
                hashToBeCompare = Cryptography.EncryptWithHash(key, defaultInputString);
            }

            // Create hash value from input string with specified key.
            var hashValue = Cryptography.EncryptWithHash(key, inputString);

            // Hash are valid when hash value is match with MD5 hash from input string.
            return hashToBeCompare == hashValue;
        }

        /// <summary>
        /// Update current user session hash by type.
        /// </summary>
        /// <param name="noreg">This current user session noreg.</param>
        /// <param name="typeCode">This user hash type code.</param>
        /// <param name="inputString">This clean input string to be hashed.</param>
        /// <param name="expiredOn">This expired on if any.</param>
        /// <returns>True if success, false otherwise.</returns>
        public bool UpdateHash(string noreg, string typeCode, string inputString, DateTime? expiredOn = null)
        {
            // Get and set hash key.
            var key = noreg;

            // Create user hash repository object.
            var set = UnitOfWork.GetRepository<UserHash>();

            // Get user hash by noreg and type code.
            var userHash = set.Fetch()
                .FirstOrDefault(x => x.NoReg == noreg && x.TypeCode == typeCode);

            // Generate hash value from input string with specified key.
            var hashValue = Cryptography.EncryptWithHash(key, inputString);

            // If user hash object is not null then do update operation.
            if (userHash != null)
            {
                // Update hash value.
                userHash.HashValue = hashValue;

                // Update expired on.
                userHash.ExpiredOn = expiredOn;
            }
            // Else add new user hash object into repository.
            else
            {
                // Add new user hash object into repository.
                set.Add(new UserHash
                {
                    // Get and set noreg.
                    NoReg = noreg,
                    // Get and set type code.
                    TypeCode = typeCode,
                    // Get and set hash value.
                    HashValue = hashValue,
                    // Get and set expired on.
                    ExpiredOn = expiredOn
                });
            }

            // Push update or insert operation and return the boolean result.
            return UnitOfWork.SaveChanges() > 0;
        }

        /// <summary>
        /// Delete user hash by id.
        /// </summary>
        /// <param name="id">This user hash id.</param>
        /// <returns>True if success, false otherwise.</returns>
        public bool DeleteHash(Guid id)
        {
            // Create user hash repository object.
            var set = UnitOfWork.GetRepository<UserHash>();

            // Get user hash by id.
            var userHash = set.FindById(id);

            // Delete hash by object.
            set.Delete(userHash);

            // Log the user activity.
            CreateUserActivityLog(userHash.NoReg, userHash.TypeCode, "Your password has been reset by administrator");

            // Return delete operation.
            return UnitOfWork.SaveChanges() > 0;
        }
        #endregion

        #region Vaccine Schedule Area
        /// <summary>
        /// Get list of Bank from cache async
        /// </summary>
        /// <param name="cache">Cache flag (if true get from cache).</param>
        /// <returns>List of Bank</returns>
        public IEnumerable<VaccineSchedule> GetVaccineSchedule(bool cache = false)
        {
            var dbitems = VaccineScheduleRepository.Fetch().Where(x => x.RowStatus);

            return cache ? dbitems.FromCache(_tags) : dbitems;
        }
        #endregion

        #region EmailSentLog
        public void CreateMailSentLog(MailSentLog mailSentLog)
        {
            MailSentLogRepository.Add(mailSentLog);

            UnitOfWork.SaveChanges();
        }
        #endregion

        public T QueryFirstOrDefault<T>(Expression<Func<T, bool>> expression) where T : class
        {
            return Query(expression).FirstOrDefaultIfEmpty();
        }

        public IEnumerable<T> Query<T>(Expression<Func<T, bool>> expression) where T : class
        {
            var set = UnitOfWork.GetRepository<T>();

            return set.Fetch()
                .AsNoTracking()
                .Where(expression);
        }

        public bool DynamicAdd<T>(T data) where T : class
        {
            var set = UnitOfWork.GetRepository<T>();

            set.Add(data);

            return UnitOfWork.SaveChanges() > 0;
        }
    }
}
