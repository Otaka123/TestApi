using testAPI.api.application.Config;
using testAPI.api.application.Exceptions;
using testAPI.api.application.ServiceInterfaces;
using testAPI.api.domain.DTOs.Auth;
using testAPI.api.domain.DTOs.User;
using testAPI.api.infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace testAPI.api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly RoleManager<AppRole> _roleManager;
    private readonly ITokenService _tokenService;
    private readonly IUserService _userService;
    private readonly IRoleService _roleService;

    public AuthController(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        RoleManager<AppRole> roleManager,
        ITokenService tokenService,
        IUserService userService,
        IRoleService roleService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _tokenService = tokenService;
        _userService = userService;
        _roleService = roleService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(new AuthResultDto
            {
                Succeeded = false,
                Message = "بيانات غير صالحة",
                Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
            });

        try
        {
            var username = request.Username.Trim();
            var password = request.Password?.Replace(" ", "").Trim() ?? string.Empty;

            var user = await _userManager.FindByNameAsync(username);
            if (user == null || user.isDeleted)
                return Unauthorized(new AuthResultDto { Succeeded = false, Message = "اسم المستخدم أو كلمة المرور غير صحيحة" });

            if (await _userManager.IsLockedOutAsync(user))
                return Unauthorized(new AuthResultDto { Succeeded = false, Message = "الحساب مقفول. يرجى المحاولة لاحقاً." });

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, password);
            if (!isPasswordValid)
            {
                await _userManager.AccessFailedAsync(user);
                if (await _userManager.IsLockedOutAsync(user))
                    return Unauthorized(new AuthResultDto { Succeeded = false, Message = "تم قفل حسابك بسبب محاولات خاطئة متكررة." });

                return Unauthorized(new AuthResultDto { Succeeded = false, Message = "اسم المستخدم أو كلمة المرور غير صحيحة" });
            }

            await _userManager.ResetAccessFailedCountAsync(user);

            if (user.TwoFactorEnabled)
                return BadRequest(new AuthResultDto { Succeeded = false, Message = "المصادقة الثنائية مفعلة. يرجى استخدام واجهة الويب لتسجيل الدخول." });

            var token = await _tokenService.CreateTokenAsync(user, cancellationToken);

            return Ok(new AuthResultDto { Succeeded = true, Message = "تم تسجيل الدخول بنجاح", Token = token });
        }
        catch (ServiceException ex)
        {
            return BadRequest(new AuthResultDto { Succeeded = false, Message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new AuthResultDto { Succeeded = false, Message = "حدث خطأ غير متوقع أثناء محاولة تسجيل الدخول" });
        }
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(new AuthResultDto
            {
                Succeeded = false,
                Message = "بيانات غير صالحة",
                Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
            });

        try
        {
            var createRequest = new CreateUserRequest
            {
                FullName = request.FullName,
                UserName = request.Username,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Password = request.Password,
                ConfirmPassword = request.ConfirmPassword,
                UserTypeId = request.UserTypeId ?? 1,
                AuthorityId = request.AuthorityId,
                RoleId = request.RoleId
            };

            var result = await _userService.AddAsync(createRequest, cancellationToken);
            if (result == null)
                return BadRequest(new AuthResultDto { Succeeded = false, Message = "فشل إنشاء الحساب." });

            var user = await _userManager.FindByIdAsync(result.Id.ToString());
            if (user == null)
                return StatusCode(500, new AuthResultDto { Succeeded = false, Message = "تم إنشاء الحساب ولكن فشل استرجاع البيانات" });

            var token = await _tokenService.CreateTokenAsync(user, cancellationToken);
            return Ok(new AuthResultDto { Succeeded = true, Message = "تم إنشاء الحساب بنجاح", Token = token });
        }
        catch (ServiceException ex)
        {
            return BadRequest(new AuthResultDto { Succeeded = false, Message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new AuthResultDto { Succeeded = false, Message = "حدث خطأ غير متوقع أثناء إنشاء الحساب" });
        }
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(new AuthResultDto { Succeeded = false, Message = "بيانات غير صالحة" });

        var result = await _tokenService.RefreshTokenAsync(request, cancellationToken);
        if (!result.Succeeded)
            return Unauthorized(result);

        return Ok(result);
    }

    [HttpPost("revoke-token")]
    [Authorize]
    public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenRequestDto request, CancellationToken cancellationToken = default)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int currentUserId))
            return Unauthorized(new AuthResultDto { Succeeded = false, Message = "غير مصرح" });

        var tokenToRevoke = string.IsNullOrEmpty(request.RefreshToken)
            ? (await _tokenService.GetActiveRefreshTokenByUserIdAsync(currentUserId, cancellationToken))?.Token
            : request.RefreshToken;

        if (string.IsNullOrEmpty(tokenToRevoke))
            return Ok(new AuthResultDto { Succeeded = true, Message = "لا يوجد رمز نشط للإلغاء" });

        var storedToken = await _tokenService.GetRefreshTokenAsync(tokenToRevoke, cancellationToken);
        if (storedToken == null || storedToken.UserId != currentUserId)
            return BadRequest(new AuthResultDto { Succeeded = false, Message = "Refresh Token غير صالح" });

        await _tokenService.RevokeRefreshTokenAsync(tokenToRevoke, cancellationToken);
        return Ok(new AuthResultDto { Succeeded = true, Message = "تم إلغاء الرمز بنجاح" });
    }

    [HttpPost("revoke-all-tokens")]
    [Authorize]
    public async Task<IActionResult> RevokeAllTokens(CancellationToken cancellationToken = default)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int currentUserId))
            return Unauthorized(new AuthResultDto { Succeeded = false, Message = "غير مصرح" });

        await _tokenService.RevokeAllUserRefreshTokensAsync(currentUserId, cancellationToken);
        return Ok(new AuthResultDto { Succeeded = true, Message = "تم إلغاء جميع الرموز بنجاح" });
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken = default)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int currentUserId))
            return Unauthorized(new AuthResultDto { Succeeded = false, Message = "غير مصرح" });

        await _tokenService.RevokeAllUserRefreshTokensAsync(currentUserId, cancellationToken);
        await _signInManager.SignOutAsync();
        return Ok(new AuthResultDto { Succeeded = true, Message = "تم تسجيل الخروج بنجاح" });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken = default)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            return Unauthorized(new { Succeeded = false, Message = "غير مصرح" });

        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user == null || user.isDeleted)
            return Unauthorized(new { Succeeded = false, Message = "المستخدم غير موجود" });

        var tokenData = await _tokenService.CreateTokenAsync(user, cancellationToken);
        return Ok(new { Succeeded = true, User = tokenData.User });
    }

    [HttpGet("roles")]
    [Authorize]
    public async Task<IActionResult> GetRoles()
    {
        var roles = await _roleService.GetAllRolesAsync();
        return Ok(new { Succeeded = true, Roles = roles });
    }

    [HttpGet("user-types")]
    [AllowAnonymous]
    public async Task<IActionResult> GetUserTypes()
    {
        var userTypes = await _userService.GetUserTypes();
        return Ok(new { Succeeded = true, UserTypes = userTypes });
    }
}
