using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace testAPI.web.Controllers;

public class BaseController : Controller
{
    /// <summary>
    /// معالجة استجابة API عندما تكون غير مصرح (403/401)
    /// </summary>
    protected IActionResult HandleUnauthorizedApiResponse(string message = "ليس لديك الصلاحيات اللازمة")
    {
        TempData["ErrorMessage"] = message;
        return RedirectToAction("Unauthorized", "Home");
    }

    /// <summary>
    /// فحص استجابة API والتعامل مع حالات غير المصرح
    /// </summary>
    protected bool IsUnauthorized(string message)
    {
        return message != null && (
            message.Contains("غير مصرح") || 
            message.Contains("Unauthorized") ||
            message.Contains("Forbidden") ||
            message.Contains("صلاحيات")
        );
    }

    /// <summary>
    /// Sign in the user locally using cookie authentication after parsing the given JWT Token
    /// </summary>
    protected async Task SignInUserWithJwtAsync(string token, string username, bool isPersistent = false)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, username),
            new Claim("jwt_token", token)
        };

        try
        {
            var handler = new JwtSecurityTokenHandler();
            if (handler.CanReadToken(token))
            {
                var jwtToken = handler.ReadJwtToken(token);
                foreach (var jwtClaim in jwtToken.Claims)
                {
                    if (!claims.Any(c => c.Type == jwtClaim.Type && c.Value == jwtClaim.Value))
                    {
                        claims.Add(new Claim(jwtClaim.Type, jwtClaim.Value));
                    }
                }
            }
        }
        catch
        {
            // If token parsing fails, fallback to basic claims
        }

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = isPersistent,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);
    }
}
