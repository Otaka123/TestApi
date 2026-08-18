using testAPI.api.application.Config;
using testAPI.api.application.ServiceInterfaces;
using testAPI.api.domain.DTOs.User;
using testAPI.api.infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace testAPI.api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IRoleService _roleService;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        IUserService userService,
        IRoleService roleService,
        UserManager<AppUser> userManager,
        ILogger<UsersController> logger)
    {
        _userService = userService;
        _roleService = roleService;
        _userManager = userManager;
        _logger = logger;
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdClaim, out int userId) ? userId : (int?)null;
    }

    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.ViewUsersPolicy)]
    public async Task<IActionResult> GetAll([FromQuery] UsersFilterRequestDto filter, CancellationToken cancellationToken = default)
    {
        try
        {
            var users = await _userService.GetUsersAsync(filter);
            return Ok(new { Succeeded = true, Count = users.Count, Data = users });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users");
            return StatusCode(500, new { Succeeded = false, Message = "حدث خطأ أثناء استرجاع بيانات المستخدمين" });
        }
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = AuthorizationPolicies.ViewUsersPolicy)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userService.GetByIdAsync(id, cancellationToken);
            if (user == null)
                return NotFound(new { Succeeded = false, Message = "المستخدم غير موجود" });

            return Ok(new { Succeeded = true, Data = user });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user {UserId}", id);
            return StatusCode(500, new { Succeeded = false, Message = "حدث خطأ أثناء استرجاع بيانات المستخدم" });
        }
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.CreateUserPolicy)]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { Succeeded = false, Message = "بيانات غير صالحة" });

        try
        {
            var result = await _userService.AddAsync(request, cancellationToken);
            if (result == null)
                return BadRequest(new { Succeeded = false, Message = "فشل إنشاء المستخدم." });

            return CreatedAtAction(nameof(GetById), new { id = result.Id }, new
            {
                Succeeded = true,
                Message = "تم إنشاء المستخدم بنجاح",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return StatusCode(500, new { Succeeded = false, Message = "حدث خطأ أثناء إنشاء المستخدم" });
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = AuthorizationPolicies.EditUserPolicy)]
    public async Task<IActionResult> Update(int id, [FromBody] EditUserDto request, CancellationToken cancellationToken = default)
    {
        if (id != request.Id)
            return BadRequest(new { Succeeded = false, Message = "المعرف غير متطابق" });

        if (!ModelState.IsValid)
            return BadRequest(new { Succeeded = false, Message = "بيانات غير صالحة" });

        try
        {
            var existingUser = await _userService.GetByIdAsync(id, cancellationToken);
            if (existingUser == null)
                return NotFound(new { Succeeded = false, Message = "المستخدم غير موجود" });

            var result = await _userService.EditAsync(request, cancellationToken);
            if (result == null)
                return BadRequest(new { Succeeded = false, Message = "فشل تحديث المستخدم" });

            return Ok(new { Succeeded = true, Message = "تم تحديث المستخدم بنجاح", Data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", id);
            return StatusCode(500, new { Succeeded = false, Message = "حدث خطأ أثناء تحديث المستخدم" });
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = AuthorizationPolicies.DeleteUserPolicy)]
    public async Task<IActionResult> Delete(int id, [FromQuery] bool permanent = false)
    {
        try
        {
            var existingUser = await _userService.GetByIdAsync(id);
            if (existingUser == null)
                return NotFound(new { Succeeded = false, Message = "المستخدم غير موجود" });

            bool result = permanent
                ? await _userService.HardDeleteAsync(id)
                : await _userService.SoftDeleteAsync(id);

            if (!result)
                return BadRequest(new { Succeeded = false, Message = "فشل حذف المستخدم" });

            return Ok(new { Succeeded = true, Message = permanent ? "تم حذف المستخدم نهائياً" : "تم حذف المستخدم بنجاح" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", id);
            return StatusCode(500, new { Succeeded = false, Message = "حدث خطأ أثناء حذف المستخدم" });
        }
    }

    [HttpPost("{id:int}/toggle-lock")]
    [Authorize(Policy = AuthorizationPolicies.EditUserPolicy)]
    public async Task<IActionResult> ToggleLock(int id)
    {
        try
        {
            var (success, isLocked, message) = await _userService.ToggleUserLockAsync(id);
            if (!success)
                return NotFound(new { Succeeded = false, Message = message });

            return Ok(new { Succeeded = true, IsLocked = isLocked, Message = message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling lock for user {UserId}", id);
            return StatusCode(500, new { Succeeded = false, Message = "حدث خطأ أثناء تحديث حالة قفل المستخدم" });
        }
    }

    [HttpPost("{id:int}/toggle-2fa")]
    [Authorize(Policy = AuthorizationPolicies.EditUserPolicy)]
    public async Task<IActionResult> ToggleTwoFactor(int id)
    {
        try
        {
            var (success, is2faEnabled, message) = await _userService.ToggleTwoFactorAsync(id);
            if (!success)
                return NotFound(new { Succeeded = false, Message = message });

            return Ok(new { Succeeded = true, IsTwoFactorEnabled = is2faEnabled, Message = message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling 2FA for user {UserId}", id);
            return StatusCode(500, new { Succeeded = false, Message = "حدث خطأ أثناء تحديث حالة المصادقة الثنائية" });
        }
    }

    [HttpPost("{id:int}/reset-password")]
    [Authorize(Policy = AuthorizationPolicies.ResetPasswordPolicy)]
    public async Task<IActionResult> ResetPassword(int id, [FromBody] ResetPasswordRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { Succeeded = false, Message = "بيانات غير صالحة" });

        try
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null || user.isDeleted)
                return NotFound(new { Succeeded = false, Message = "المستخدم غير موجود" });

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);

            if (!result.Succeeded)
                return BadRequest(new { Succeeded = false, Message = "فشل إعادة تعيين كلمة المرور", Errors = result.Errors.Select(e => e.Description) });

            return Ok(new { Succeeded = true, Message = "تم إعادة تعيين كلمة المرور بنجاح" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password for user {UserId}", id);
            return StatusCode(500, new { Succeeded = false, Message = "حدث خطأ أثناء إعادة تعيين كلمة المرور" });
        }
    }

    [HttpGet("user-types")]
    [AllowAnonymous]
    public async Task<IActionResult> GetUserTypes()
    {
        try
        {
            var userTypes = await _userService.GetUserTypes();
            return Ok(new { Succeeded = true, Data = userTypes });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user types");
            return StatusCode(500, new { Succeeded = false, Message = "حدث خطأ أثناء استرجاع أنواع المستخدمين" });
        }
    }

    [HttpGet("{id:int}/permissions")]
    [Authorize(Policy = AuthorizationPolicies.ViewUsersPolicy)]
    public async Task<IActionResult> GetUserPermissions(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null || user.isDeleted)
                return NotFound(new { Succeeded = false, Message = "المستخدم غير موجود" });

            var roles = await _userManager.GetRolesAsync(user);
            var claims = await _userManager.GetClaimsAsync(user);

            return Ok(new
            {
                Succeeded = true,
                Roles = roles,
                Claims = claims.Select(c => new { c.Type, c.Value }),
                Permissions = claims.Select(c => c.Type).Distinct()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving permissions for user {UserId}", id);
            return StatusCode(500, new { Succeeded = false, Message = "حدث خطأ أثناء استرجاع صلاحيات المستخدم" });
        }
    }
}

public class ResetPasswordRequest
{
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}
