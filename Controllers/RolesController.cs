using Microsoft.AspNetCore.Mvc;
using SistemOptimasiWebAPI.Interfaces;

namespace SistemOptimasiWebAPI.Controllers
{
    [ApiController]
    [Route("api/roles")]
    public class RolesController : ControllerBase
    {
        private readonly IRoles _roles;

        public RolesController(IRoles roles)
        {
            _roles = roles;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRoles()
        {
            try
            {
                var roles = await _roles.GetAllRoles();
                return Ok(roles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoleById(int id)
        {
            try
            {
                var role = await _roles.GetRoleById(id);

                if (role == null) return NotFound(new { message = "Role tidak ditemukan" });

                return Ok(role);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] Role roleData)
        {
            try
            {
                var result = await _roles.CreateRole(roleData?.NamaRole);

                if (!result.Success)
                    return BadRequest(new { message = result.Message });

                return Ok(new
                {
                    message = result.Message,
                    data = result.Data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] Role roleData)
        {
            try
            {
                var result = await _roles.UpdateRole(id, roleData?.NamaRole);

                if (result.NotFound)
                    return NotFound(new { message = result.Message });

                if (!result.Success)
                    return BadRequest(new { message = result.Message });

                return Ok(new { message = result.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            try
            {
                var result = await _roles.DeleteRole(id);

                if (result.NotFound)
                    return NotFound(new { message = result.Message });

                if (!result.Success)
                    return BadRequest(new { message = result.Message });

                return Ok(new { message = result.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
