using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using testAPI.api.domain.DTOs.Auth;
using testAPI.web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace testAPI.web.Controllers;

public class AccountController : BaseController
{
    private readonly IAuthApiService _authApiService;

    public AccountController(IAuthApiService authApiService)
    {
        _authApiService = authApiService;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToLocal(returnUrl);
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginRequestDto());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginRequestDto model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _authApiService.LoginAsync(model);

        if (!result.Succeeded || result.Token == null)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "فشل تسجيل الدخول");
            if (result.Errors != null)
            {
                foreach (var err in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, err);
                }
            }
            return View(model);
        }

        // Sign in user with Cookie Auth using claims extracted from JWT token
        await SignInUserWithJwtAsync(result.Token.AccessToken, model.Username, model.RememberMe);

        TempData["SuccessMessage"] = result.Message ?? "تم تسجيل الدخول بنجاح";
        return RedirectToLocal(returnUrl);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(new RegisterRequestDto { RoleId = 2 }); // Default role
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterRequestDto model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _authApiService.RegisterAsync(model);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "فشل إنشاء الحساب");
            if (result.Errors != null)
            {
                foreach (var err in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, err);
                }
            }
            return View(model);
        }

        // If registration returns a token, automatically sign in
        if (result.Token != null && !string.IsNullOrEmpty(result.Token.AccessToken))
        {
            await SignInUserWithJwtAsync(result.Token.AccessToken, model.Username, false);
            TempData["SuccessMessage"] = "تم إنشاء الحساب وتسجيل الدخول بنجاح";
            return RedirectToAction("Index", "Home");
        }

        TempData["SuccessMessage"] = result.Message ?? "تم إنشاء الحساب بنجاح، يرجى تسجيل الدخول";
        return RedirectToAction(nameof(Login));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var tokenClaim = User.FindFirst("jwt_token")?.Value;
        
        await _authApiService.LogoutAsync(tokenClaim);
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        TempData["SuccessMessage"] = "تم تسجيل الخروج بنجاح";
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public async Task<IActionResult> LogoutGet()
    {
        return await Logout();
    }



    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        return RedirectToAction("Index", "Home");
    }
}
