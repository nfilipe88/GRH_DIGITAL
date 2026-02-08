using HRManager.WebAPI.Domain.Interfaces;
using HRManager.WebAPI.DTOs;
using HRManager.WebAPI.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace HRManager.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [PermissionAuthorize("ROLES_MANAGE")]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RolesController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<RoleDto>>> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var roles = await _roleService.GetRolesAsync(page, pageSize);
            return Ok(roles);
        }

        [HttpPost]
        public async Task<ActionResult<RoleDetailDto>> Create(CreateRoleRequest request)
        {
            var role = await _roleService.CreateRoleAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = role.Id }, role);
        }

        [HttpPost("{roleId}/permissions")]
        public async Task<ActionResult> AssignPermissions(Guid roleId, [FromBody] List<Guid> permissionIds)
        {
            await _roleService.AssignPermissionsToRoleAsync(roleId, permissionIds);
            return Ok();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RoleDetailDto>> GetById(Guid id)
        {
            var role = await _roleService.GetRoleByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }
            return Ok(role);
        }
    }
}
