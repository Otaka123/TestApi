using testAPI.api.domain.DTOs.User;
using testAPI.web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace testAPI.web.Controllers;

[Authorize]
public class UsersController : BaseController
{
    private readonly IUsersApiService _usersApi;

    public UsersController(IUsersApiService usersApi)
    {
        _usersApi = usersApi;
    }

    private string? GetToken() => User.FindFirst("jwt_token")?.Value;

    public async Task<IActionResult> Index()
    {
        var token = GetToken();
        if (token == null) return RedirectToAction("Login", "Account");

        var result = await _usersApi.GetAllAsync(token);
        
        // معالجة حالة عدم التصريح
        if (!result.Succeeded && IsUnauthorized(result.Message))
        {
            return HandleUnauthorizedApiResponse(result.Message);
        }
        
        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = result.Message;
            return View(new List<UserResponseDto>());
        }
        return View(result.Data);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var token = GetToken();
        if (token == null) return RedirectToAction("Login", "Account");

        await LoadRolesDropdown(token);
        return View(new CreateUserRequest { UserTypeId = 1 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserRequest model)
    {
        var token = GetToken();
        if (token == null) return RedirectToAction("Login", "Account");

        if (!ModelState.IsValid)
        {
            await LoadRolesDropdown(token);
            return View(model);
        }

        var result = await _usersApi.CreateAsync(model, token);
        if (!result.Succeeded)
        {
            ModelState.AddModelError("", result.Message);
            await LoadRolesDropdown(token);
            return View(model);
        }

        TempData["SuccessMessage"] = "تم إنشاء المستخدم بنجاح";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int? id)
    {
        var token = GetToken();
        if (token == null) return RedirectToAction("Login", "Account");

        if (id == null || id <= 0) return RedirectToAction(nameof(Index));

        var result = await _usersApi.GetByIdAsync(id.Value, token);
        if (!result.Succeeded || result.Data == null)
        {
            TempData["ErrorMessage"] = result.Message ?? "المستخدم غير موجود";
            return RedirectToAction(nameof(Index));
        }

        var user = result.Data;
        await LoadRolesDropdown(token);
        return View(new EditUserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            UserName = user.UserName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            UserTypeId = user.UserTypeId ?? 1,
            RoleId = user.RoleId ?? 0,
            AuthorityId = user.AuthorityId,
            DeveloperCode = user.DeveloperCode
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditUserDto model)
    {
        var token = GetToken();
        if (token == null) return RedirectToAction("Login", "Account");

        if (!ModelState.IsValid)
        {
            await LoadRolesDropdown(token);
            return View(model);
        }

        var result = await _usersApi.UpdateAsync(id, model, token);
        if (!result.Succeeded)
        {
            ModelState.AddModelError("", result.Message);
            await LoadRolesDropdown(token);
            return View(model);
        }

        TempData["SuccessMessage"] = "تم تحديث المستخدم بنجاح";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var token = GetToken();
        if (token == null) return RedirectToAction("Login", "Account");

        var result = await _usersApi.DeleteAsync(id, token);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleLock(int id)
    {
        var token = GetToken();
        if (token == null) return RedirectToAction("Login", "Account");

        var result = await _usersApi.ToggleLockAsync(id, token);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult ResetPassword(int id)
    {
        ViewBag.UserId = id;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(int id, string newPassword)
    {
        var token = GetToken();
        if (token == null) return RedirectToAction("Login", "Account");

        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 8)
        {
            ModelState.AddModelError("", "كلمة المرور يجب أن تكون 8 أحرف على الأقل");
            ViewBag.UserId = id;
            return View();
        }

        var result = await _usersApi.ResetPasswordAsync(id, newPassword, token);
        if (!result.Succeeded)
        {
            ModelState.AddModelError("", result.Message);
            ViewBag.UserId = id;
            return View();
        }

        TempData["SuccessMessage"] = "تم إعادة تعيين كلمة المرور بنجاح";
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadRolesDropdown(string token)
    {
        var roles = await _usersApi.GetRolesAsync(token);
        ViewBag.Roles = roles.Data?.Select(r => new SelectListItem { Value = r.Id.ToString(), Text = r.Name }).ToList()
                        ?? new List<SelectListItem>();
    }
}
