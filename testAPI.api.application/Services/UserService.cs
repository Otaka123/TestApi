using testAPI.api.application.ServiceInterfaces;
using testAPI.api.domain.DTOs.Signature;
using testAPI.api.domain.DTOs.User;
using testAPI.api.domain.Entities;
using testAPI.api.domain.Enums;
using testAPI.api.infrastructure.Data;
using testAPI.api.infrastructure.Identity;
using testAPI.api.infrastructure.Persistence.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace testAPI.api.application.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IRoleService _roleService;
        private readonly AppDbContext _context;
        private readonly IUserRepository _userRepository;
        private readonly ISignatureService _signatureService;
        private readonly IExcelExportService _excelExportService;
        private readonly ICurrentUserService _currentUser;

        public UserService(
            UserManager<AppUser> userManager,
            IRoleService roleService,
            AppDbContext context,
            IUserRepository userRepository,
            ISignatureService signatureService,
            IExcelExportService excelExportService,
            ICurrentUserService currentUser)
        {
            _userManager = userManager;
            _roleService = roleService;
            _context = context;
            _userRepository = userRepository;
            _signatureService = signatureService;
            _excelExportService = excelExportService;
            _currentUser = currentUser;
        }

        public async Task<List<UserResponseDto>> GetAllAsync(bool? includeDeleted = true, CancellationToken cancellationToken = default)
            => await _userRepository.GetAllAsync(includeDeleted, cancellationToken);

        public async Task<List<UserResponseDto>> GetAllUsersNamesAsync(int? userType)
            => await _userRepository.GetAllNamesWithIdsAsync(userType);

        public async Task<List<UserResponseDto>> GetUsersAsync(UsersFilterRequestDto filter)
            => await _userRepository.GetUsersAsync(filter);

        public async Task<UserResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => await _userRepository.GetByIdAsync(id, cancellationToken);

        public async Task<UserResponse> AddAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
        {
            //request.PhoneNumber = NormalizePhoneNumber(request.PhoneNumber);

            if (await _userManager.Users.AnyAsync(x => x.Email == request.Email, cancellationToken))
                return null!;
            if (await _userManager.Users.AnyAsync(x => x.PhoneNumber == request.PhoneNumber, cancellationToken))
                return null!;
            if (await _userManager.Users.AnyAsync(x => x.UserName == request.UserName, cancellationToken))
                return null!;
            if (request.Password != request.ConfirmPassword)
                return null!;

            var roleExists = await _roleService.GetRoleByIdAsync(request.RoleId);
            if (roleExists == null) return null!;

            var user = new AppUser
            {
                FullName = request.FullName,
                UserName = request.UserName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                isDeleted = false,
                UserTypeId = request.UserTypeId,
                AuthorityId = request.AuthorityId,
                DeveloperCode = request.DeveloperCode,
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded) return null!;

            await _userManager.AddToRoleAsync(user, roleExists.Name!);

            if (request.NewSignatureFile != null)
            {
                try
                {
                    await _signatureService.AddSignatureAsync(new CreateSignatureDto
                    {
                        SignatureImage = request.NewSignatureFile,
                        UserId = user.Id
                    }, cancellationToken);
                }
                catch { /* لا نوقف العملية بسبب فشل التوقيع */ }
            }

            var getUserType = await _context.UserTypes.FirstOrDefaultAsync(ut => ut.Id == request.UserTypeId, cancellationToken);

            return new UserResponse
            {
                Id = user.Id,
                FullName = user.FullName,
                UserName = user.UserName!,
                Email = user.Email!,
                PhoneNumber = user.PhoneNumber!,
                UserType = getUserType?.TypeName ?? "",
                RoleName = roleExists.Name!,
            };
        }

        public async Task<UserResponse> EditAsync(EditUserDto request, CancellationToken cancellationToken = default)
        {
            //request.PhoneNumber = NormalizePhoneNumber(request.PhoneNumber);

            if (await _userManager.Users.AnyAsync(x => x.Email == request.Email && x.Id != request.Id, cancellationToken))
                return null!;
            if (await _userManager.Users.AnyAsync(x => x.UserName == request.UserName && x.Id != request.Id, cancellationToken))
                return null!;
            if (await _userManager.Users.AnyAsync(x => x.PhoneNumber == request.PhoneNumber && x.Id != request.Id, cancellationToken))
                return null!;

            var user = await _userManager.FindByIdAsync(request.Id.ToString());
            if (user == null) return null!;

            user.UserName = request.UserName;
            user.Email = request.Email;
            user.PhoneNumber = request.PhoneNumber;
            user.FullName = request.FullName;
            user.UserTypeId = request.UserTypeId;
            user.AuthorityId = request.AuthorityId;
            user.DeveloperCode = request.DeveloperCode;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded) return null!;

            var currentRoles = await _userManager.GetRolesAsync(user);
            var getRoleName = await _roleService.GetRoleByIdAsync(request.RoleId);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (getRoleName != null)
                await _userManager.AddToRoleAsync(user, getRoleName.Name!);

            var getUserType = await _context.UserTypes.FirstOrDefaultAsync(ut => ut.Id == request.UserTypeId, cancellationToken);

            return new UserResponse
            {
                Id = user.Id,
                FullName = user.FullName,
                UserName = user.UserName!,
                Email = user.Email!,
                PhoneNumber = user.PhoneNumber!,
                UserType = getUserType?.TypeName ?? "",
                RoleName = getRoleName?.Name ?? "",
            };
        }

        public async Task<bool> SoftDeleteAsync(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return false;

            if (await _context.Histories.AnyAsync(h => h.UserId == id))
                return false;

            user.isDeleted = true;
            await _userManager.UpdateAsync(user);
            return true;
        }

        public async Task<bool> HardDeleteAsync(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return false;

            if (await _context.Histories.AnyAsync(h => h.UserId == id))
                return false;

            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded;
        }

        public async Task<List<UserType>> GetUserTypes()
            => await _userRepository.GetUserTypes();

        public async Task<(bool success, bool isLocked, string message)> ToggleUserLockAsync(long id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return (false, false, "المستخدم غير موجود");

            var isCurrentlyLocked = await _userManager.IsLockedOutAsync(user);

            if (isCurrentlyLocked)
            {
                await _userManager.SetLockoutEndDateAsync(user, null);
                await _userManager.ResetAccessFailedCountAsync(user);
                return (true, false, "تم فك قفل الحساب بنجاح");
            }
            else
            {
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
                await _userManager.UpdateSecurityStampAsync(user);
                return (true, true, "تم قفل الحساب بنجاح");
            }
        }

        public async Task<byte[]> ExportUsersToExcelAsync(UsersFilterRequestDto filter)
        {
            var users = await GetUsersAsync(filter);
            var exportData = users.Select(u => new
            {
                الاسم_الكامل = u.FullName,
                اسم_المستخدم = u.UserName,
                البريد_الإلكتروني = u.Email,
                رقم_الهاتف = u.PhoneNumber,
                الصلاحية = u.RoleName ?? "",
                نوع_المستخدم = u.UserType ?? ""
            }).ToList();

            var headers = new List<string> { "الاسم الكامل", "اسم المستخدم", "البريد الإلكتروني", "رقم الهاتف", "الصلاحية", "نوع المستخدم" };
            return _excelExportService.ExportToExcel(exportData, headers, "المستخدمين");
        }

        public async Task<UserType> GetCurrentUserTypeAsync()
        {
            var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
            var user = await _userManager.Users.Include(u => u.UserType).FirstOrDefaultAsync(u => u.Id == userId);
            return user?.UserType ?? throw new UnauthorizedAccessException("User type not found");
        }

        public async Task<AppUser> GetCurrentUserAsync()
        {
            var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
            var user = await _userManager.Users.Include(u => u.UserType).Include(u => u.Authority).FirstOrDefaultAsync(u => u.Id == userId);
            return user ?? throw new UnauthorizedAccessException("User not found");
        }

        public async Task<int?> GetCurrentUserTypeIdAsync()
        {
            var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
            return await _userManager.Users.Where(u => u.Id == userId).Select(u => u.UserTypeId).FirstOrDefaultAsync();
        }

        public async Task<(bool success, bool is2faEnabled, string message)> ToggleTwoFactorAsync(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return (false, false, "المستخدم غير موجود");

            bool currentStatus = await _userManager.GetTwoFactorEnabledAsync(user);

            if (currentStatus)
            {
                await _userManager.SetTwoFactorEnabledAsync(user, false);
                user.TwoFactorPolicy = TwoFactorPolicy.UserChoice;
                user.HasCompletedEnforcedSetup = false;
                await _userManager.ResetAuthenticatorKeyAsync(user);
                await _userManager.UpdateSecurityStampAsync(user);
                await _userManager.UpdateAsync(user);
                return (true, false, "تم تعطيل المصادقة الثنائية بنجاح");
            }
            else
            {
                user.TwoFactorPolicy = TwoFactorPolicy.AdminEnforced;
                user.HasCompletedEnforcedSetup = false;
                await _userManager.ResetAuthenticatorKeyAsync(user);
                await _userManager.UpdateSecurityStampAsync(user);
                await _userManager.UpdateAsync(user);
                return (true, true, "تم تفعيل المصادقة الثنائية بنجاح");
            }
        }

        public async Task<string?> GetUserFullNameAsync(int userId)
            => await _userManager.Users.Where(u => u.Id == userId).Select(u => u.FullName).FirstOrDefaultAsync();

        public async Task<string?> GetAuthorityNameByIdAsync(int authorityId)
            => await _context.Authorities.Where(a => a.Id == authorityId).Select(a => a.Name).FirstOrDefaultAsync();

        //private string NormalizePhoneNumber(string phoneNumber)
        //{
        //    if (string.IsNullOrWhiteSpace(phoneNumber)) return phoneNumber;
        //    var cleaned = phoneNumber.Trim();
        //    if (cleaned.StartsWith("971")) cleaned = cleaned.Substring(3);
        //    if (cleaned.StartsWith("0")) cleaned = cleaned.Substring(1);
        //    if (string.IsNullOrEmpty(cleaned)) return phoneNumber;
        //    return "971" + cleaned;
        //}
    }
}
