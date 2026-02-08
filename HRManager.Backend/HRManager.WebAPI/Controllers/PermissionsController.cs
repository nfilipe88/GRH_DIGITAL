using HRManager.WebAPI.Domain.Interfaces;
using HRManager.WebAPI.DTOs;
using Microsoft.AspNetCore.Mvc;
using HRManager.WebAPI.Infrastructure;

namespace HRManager.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [PermissionAuthorize("PERMISSIONS_MANAGE")] // Exige permissão para gerir permissões
    public class PermissionsController : ControllerBase
    {
        private readonly IPermissionService _permissionService;

        public PermissionsController(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        [HttpGet]
        public async Task<ActionResult<List<PermissionDto>>> GetAll()
        {
            var permissions = await _permissionService.GetAllPermissionsAsync();
            return Ok(permissions);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PermissionDto>> GetById(Guid id)
        {
            var permission = await _permissionService.GetPermissionAsync(id);
            return Ok(permission);
        }

        [HttpPost]
        public async Task<ActionResult<PermissionDto>> Create(CreatePermissionRequest request)
        {
            var permission = await _permissionService.CreatePermissionAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = permission.Id }, permission);
        }
    }
}
