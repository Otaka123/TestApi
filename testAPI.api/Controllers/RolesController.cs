using testAPI.api.application.Config;
using testAPI.api.application.ServiceInterfaces;
using testAPI.api.domain.DTOs.Role;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace testAPI.api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;
    private readonly ILogger<RolesController> _logger;

    public RolesController(IRoleService roleService, ILogger<RolesController> logger)
    {
        _roleService = roleService;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.ViewRolesPolicy)]
    public async Task<IActionResult> GetAll([FromQuery] bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        try
        {
            var roles = includeDeleted
                ? await _roleService.GetAllRolesWithDeletedAsync()
                : await _roleService.GetAllRolesAsync();

            return Ok(new { Succeeded = true, Count = roles.Count, Data = roles });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving roles");
            return StatusCode(500, new { Succeeded = false, Message = "حدث خطأ أثناء استرجاع الأدوار" });
        }
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = AuthorizationPolicies.ViewRolesPolicy)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var role = await _roleService.GetRoleByIdAsync(id) ?? await _roleService.GetDeletedRoleByIdAsync(id);
            if (role == null)
                return NotFound(new { Succeeded = false, Message = "الدور غير موجود" });

            return Ok(new { Succeeded = true, Data = role });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving role {RoleId}", id);
            return StatusCode(500, new { Succeeded = false, Message = "حدث خطأ أثناء استرجاع بيانات الدور" });
        }
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.CreateRolePolicy)]
    public async Task<IActionResult> Create([FromBody] CreateRoleDTO request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { Succeeded = false, Message = "بيانات غير صالحة" });

        try
        {
            if (await _roleService.RoleNameExistsAsync(request.Name))
                return BadRequest(new { Succeeded = false, Message = "اسم الدور مستخدم بالفعل" });

            var result = await _roleService.CreateRoleAsync(request);
            if (!result)
                return BadRequest(new { Succeeded = false, Message = "فشل إنشاء الدور" });

            var allRoles = await _roleService.GetAllRolesAsync();
            var createdRole = allRoles.FirstOrDefault(r => r.Name == request.Name);

            return CreatedAtAction(nameof(GetById), new { id = createdRole?.Id ?? 0 }, new
            {
                Succeeded = true,
                Message = "تم إنشاء الدور بنجاح",
                Data = createdRole
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating role");
            return StatusCode(500, new { Succeeded = false, Message = "حدث خطأ أثناء إنشاء الدور" });
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = AuthorizationPolicies.EditRolePolicy)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRoleDTO request, CancellationToken cancellationToken = default)
    {
        if (id != request.Id)
            return BadRequest(new { Succeeded = false, Message = "المعرف غير متطابق" });

        if (!ModelState.IsValid)
            return BadRequest(new { Succeeded = false, Message = "بيانات غير صالحة" });

        try
        {
            var existingRole = await _roleService.GetRoleByIdAsync(id);
            if (existingRole == null)
                return NotFound(new { Succeeded = false, Message = "الدور غير موجود" });

            if (await _roleService.RoleNameExistsAsync(request.Name, id))
                return BadRequest(new { Succeeded = false, Message = "اسم الدور مستخدم بالفعل" });

            var result = await _roleService.UpdateRoleAsync(request);
            if (!result)
                return BadRequest(new { Succeeded = false, Message = "فشل تحديث الدور" });

            var updatedRole = await _roleService.GetRoleByIdAsync(id);
            return Ok(new { Succeeded = true, Message = "تم تحديث الدور بنجاح", Data = updatedRole });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role {RoleId}", id);
            return StatusCode(500, new { Succeeded = false, Message = "حدث خطأ أثناء تحديث الدور" });
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = AuthorizationPolicies.DeleteRolePolicy)]
    public async Task<IActionResult> Delete(int id, [FromQuery] bool permanent = false, CancellationToken cancellationToken = default)
    {
        try
        {
            var existingRole = await _roleService.GetRoleByIdAsync(id) ?? await _roleService.GetDeletedRoleByIdAsync(id);
            if (existingRole == null)
                return NotFound(new { Succeeded = false, Message = "الدور غير موجود" });

            bool result = permanent
                ? await _roleService.HardDeleteRoleAsync(id)
                : await _roleService.SoftDeleteRoleAsync(id);

            if (!result)
                return BadRequest(new { Succeeded = false, Message = "فشل حذف الدور" });

            return Ok(new { Succeeded = true, Message = permanent ? "تم حذف الدور نهائياً" : "تم حذف الدور بنجاح" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting role {RoleId}", id);
            return StatusCode(500, new { Succeeded = false, Message = "حدث خطأ أثناء حذف الدور" });
        }
    }

    [HttpPost("{id:int}/restore")]
    [Authorize(Policy = AuthorizationPolicies.DeleteRolePolicy)]
    public async Task<IActionResult> Restore(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var deletedRole = await _roleService.GetDeletedRoleByIdAsync(id);
            if (deletedRole == null)
                return NotFound(new { Succeeded = false, Message = "الدور غير موجود في سلة المحذوفات" });

            var result = await _roleService.RestoreRoleAsync(id);
            if (!result)
                return BadRequest(new { Succeeded = false, Message = "فشل استعادة الدور" });

            var restoredRole = await _roleService.GetRoleByIdAsync(id);
            return Ok(new { Succeeded = true, Message = "تم استعادة الدور بنجاح", Data = restoredRole });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restoring role {RoleId}", id);
            return StatusCode(500, new { Succeeded = false, Message = "حدث خطأ أثناء استعادة الدور" });
        }
    }

    [HttpGet("export/excel")]
    [Authorize(Policy = AuthorizationPolicies.ViewRolesPolicy)]
    public async Task<IActionResult> ExportToExcel()
    {
        try
        {
            var excelBytes = await _roleService.ExportRolesToExcelAsync();
            var fileName = $"Roles_Export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting roles");
            return StatusCode(500, new { Succeeded = false, Message = "حدث خطأ أثناء تصدير الأدوار" });
        }
    }
}
