using testAPI.api.application.Config;
using testAPI.api.application.ServiceInterfaces;
using testAPI.api.infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace testAPI.api.Controllers;

[ApiController]
[Route("api/roles/{roleId:int}/[controller]")]
[Authorize]
[Produces("application/json")]
public class RoleClaimsController : ControllerBase
{
    private readonly IRoleClaimsService _roleClaimsService;
    private readonly IRoleService _roleService;
    private readonly ILogger<RoleClaimsController> _logger;

    public RoleClaimsController(IRoleClaimsService roleClaimsService, IRoleService roleService, ILogger<RoleClaimsController> logger)
    {
        _roleClaimsService = roleClaimsService;
        _roleService = roleService;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.ManageRoleClaimsPolicy)]
    public async Task<IActionResult> GetClaimsForRole(int roleId, CancellationToken cancellationToken = default)
    {
        try
        {
            var role = await _roleService.GetRoleByIdAsync(roleId);
            if (role == null)
                return NotFound(new { Succeeded = false, Message = "الدور غير موجود" });

            var roleClaims = await _roleClaimsService.GetClaimsForRoleAsync(roleId) ?? new ClaimsModel { RoleId = roleId };

            var allClaimCategories = new
            {
                HomeClaims = ClaimStore.HomeClaimsList.Select(c => new { c.Type, c.Value }).ToList(),
                RolesClaims = ClaimStore.RolesClaimsList.Select(c => new { c.Type, c.Value }).ToList(),
                UsersClaims = ClaimStore.UsersClaimsList.Select(c => new { c.Type, c.Value }).ToList()
            };

            return Ok(new
            {
                Succeeded = true,
                RoleId = roleId,
                RoleName = role.Name,
                RoleClaims = roleClaims,
                AllClaimCategories = allClaimCategories,
                AllClaimTypes = AuthorizationPolicies.AllClaimTypes
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving claims for role {RoleId}", roleId);
            return StatusCode(500, new { Succeeded = false, Message = "حدث خطأ أثناء استرجاع صلاحيات الدور" });
        }
    }

    [HttpPut]
    [Authorize(Policy = AuthorizationPolicies.ManageRoleClaimsPolicy)]
    public async Task<IActionResult> UpdateClaimsForRole(int roleId, [FromBody] ClaimsModel model, CancellationToken cancellationToken = default)
    {
        if (roleId != model.RoleId)
            return BadRequest(new { Succeeded = false, Message = "المعرف غير متطابق" });

        if (!ModelState.IsValid)
            return BadRequest(new { Succeeded = false, Message = "بيانات غير صالحة" });

        try
        {
            var role = await _roleService.GetRoleByIdAsync(roleId);
            if (role == null)
                return NotFound(new { Succeeded = false, Message = "الدور غير موجود" });

            var result = await _roleClaimsService.UpdateRoleClaimsAsync(roleId, model);
            if (!result)
                return BadRequest(new { Succeeded = false, Message = "فشل تحديث الصلاحيات" });

            var updatedClaims = await _roleClaimsService.GetClaimsForRoleAsync(roleId);
            return Ok(new { Succeeded = true, Message = "تم تحديث الصلاحيات بنجاح", Data = updatedClaims });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating claims for role {RoleId}", roleId);
            return StatusCode(500, new { Succeeded = false, Message = "حدث خطأ أثناء تحديث صلاحيات الدور" });
        }
    }

    [HttpGet("available")]
    [Authorize(Policy = AuthorizationPolicies.ManageRoleClaimsPolicy)]
    public IActionResult GetAvailableClaims()
    {
        try
        {
            var allCategories = new
            {
                HomeClaims = ClaimStore.HomeClaimsList.Select(c => new { c.Type, c.Value }).ToList(),
                RolesClaims = ClaimStore.RolesClaimsList.Select(c => new { c.Type, c.Value }).ToList(),
                UsersClaims = ClaimStore.UsersClaimsList.Select(c => new { c.Type, c.Value }).ToList()
            };

            return Ok(new
            {
                Succeeded = true,
                Categories = allCategories,
                AllClaimTypes = AuthorizationPolicies.AllClaimTypes
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving available claims");
            return StatusCode(500, new { Succeeded = false, Message = "حدث خطأ أثناء استرجاع الصلاحيات المتاحة" });
        }
    }
}
