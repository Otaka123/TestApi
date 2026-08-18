using testAPI.api.domain.DTOs.Role;
using testAPI.web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace testAPI.web.Controllers;

[Authorize]
public class RolesController : BaseController
{
    private readonly IRolesApiService _rolesApi;
    private readonly IRoleClaimsApiService _claimsApi;
    private readonly IAuthApiService _authApi;

    public RolesController(IRolesApiService rolesApi, IRoleClaimsApiService claimsApi, IAuthApiService authApi)
    {
        _rolesApi = rolesApi;
        _claimsApi = claimsApi;
        _authApi = authApi;
    }

    private string? GetToken() => User.FindFirst("jwt_token")?.Value;

    public async Task<IActionResult> Index()
    {
        var token = GetToken();
        if (token == null) return RedirectToAction("Login", "Account");

        var result = await _rolesApi.GetAllAsync(token);
        
        // معالجة حالة عدم التصريح
        if (!result.Succeeded && IsUnauthorized(result.Message))
        {
            return HandleUnauthorizedApiResponse(result.Message);
        }
        
        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = result.Message;
            return View(new List<RoleDTO>());
        }
        return View(result.Data);
    }

    [HttpGet]
    public IActionResult Create() => View(new CreateRoleDTO());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateRoleDTO model)
    {
        if (!ModelState.IsValid) return View(model);

        var token = GetToken();
        if (token == null) return RedirectToAction("Login", "Account");

        var result = await _rolesApi.CreateAsync(model, token);
        if (!result.Succeeded)
        {
            ModelState.AddModelError("", result.Message);
            return View(model);
        }

        TempData["SuccessMessage"] = "تم إنشاء الدور بنجاح";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var token = GetToken();
        if (token == null) return RedirectToAction("Login", "Account");

        var result = await _rolesApi.GetByIdAsync(id, token);
        if (!result.Succeeded || result.Data == null)
        {
            TempData["ErrorMessage"] = result.Message ?? "الدور غير موجود";
            return RedirectToAction(nameof(Index));
        }

        return View(new UpdateRoleDTO { Id = result.Data.Id, Name = result.Data.Name });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateRoleDTO model)
    {
        if (!ModelState.IsValid) return View(model);

        var token = GetToken();
        if (token == null) return RedirectToAction("Login", "Account");

        var result = await _rolesApi.UpdateAsync(id, model, token);
        if (!result.Succeeded)
        {
            ModelState.AddModelError("", result.Message);
            return View(model);
        }

        TempData["SuccessMessage"] = "تم تحديث الدور بنجاح";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var token = GetToken();
        if (token == null) return RedirectToAction("Login", "Account");

        var result = await _rolesApi.DeleteAsync(id, token);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> ManageClaims(int id)
    {
        var token = GetToken();
        if (token == null) return RedirectToAction("Login", "Account");

        var result = await _claimsApi.GetClaimsForRoleAsync(id, token);
        
        // معالجة حالة عدم التصريح
        if (!result.Succeeded && IsUnauthorized(result.Message))
        {
            return HandleUnauthorizedApiResponse(result.Message);
        }
        
        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        ViewBag.RoleId = result.RoleId;
        ViewBag.RoleName = result.RoleName;
        ViewBag.RoleClaims = result.RoleClaims;
        ViewBag.AllClaimCategories = result.AllClaimCategories;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> UpdateClaims(int id, [FromBody] JsonElement model)
    {
        var token = GetToken();
        if (token == null) 
            return Json(new { succeeded = false, message = "غير مصرح. الرجاء تسجيل الدخول مرة أخرى" });

        try
        {
            var result = await _claimsApi.UpdateClaimsAsync(id, model, token);
            
            if (result.Succeeded)
            {
                var meResponse = await _authApi.GetCurrentUserAsync(token);
                if (meResponse.Succeeded && meResponse.Token != null && !string.IsNullOrEmpty(meResponse.Token.AccessToken))
                {
                    await SignInUserWithJwtAsync(meResponse.Token.AccessToken, User.Identity?.Name ?? string.Empty, isPersistent: true);
                }
            }
            
            return Json(new { succeeded = result.Succeeded, message = result.Message });
        }
        catch (Exception ex)
        {
            return Json(new { succeeded = false, message = "حدث خطأ أثناء الحفظ: " + ex.Message });
        }
    }
}
