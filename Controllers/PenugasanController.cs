using Microsoft.AspNetCore.Mvc;
using SistemOptimasiWebAPI.Interfaces;

namespace SistemOptimasiWebAPI.Controllers
{
    [ApiController]
    [Route("api/manajemen-antaran")]
    public class PenugasanController : ControllerBase
    {
        private readonly IPenugasan _penugasan;

        public PenugasanController(IPenugasan penugasan)
        {
            _penugasan = penugasan;
        }

        [HttpGet("penugasan")]
        public async Task<IActionResult> GetPenugasan([FromQuery] DateOnly tglAwal, [FromQuery] DateOnly tglAkhir, [FromQuery] int? user_id = null, [FromQuery] string? status = null)
        {
            try
            {
                var data = await _penugasan.GetPenugasan(tglAwal, tglAkhir, user_id, status);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("dashboard/{userId}")]
        public async Task<IActionResult> GetInfo(int userId)
        {
            try
            {
                var result = await _penugasan.GetDashboardInfo(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("penugasan")]
        public async Task<IActionResult> HapusPenugasan([FromQuery] int jadwalId)
        {
            try
            {
                var result = await _penugasan.HapusPenugasan(jadwalId);

                if (!result.Success)
                {
                    return NotFound(new { message = result.Message });
                }

                return Ok(new { message = result.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Terjadi kesalahan pada server saat menghapus data.",
                    error = ex.Message
                });
            }
        }

        // tambah
        [HttpPost("assign-to-delivery")]
        public async Task<IActionResult> TugaskanKiriman([FromBody] AssignToDeliveryRequest request)
        {
            try
            {
                var result = await _penugasan.TugaskanKiriman(request);

                if (!result.Success)
                {
                    if (result.NotFound)
                        return NotFound(new { message = result.Message });

                    return BadRequest(new { message = result.Message });
                }

                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // Hal Mobile
        [HttpGet("by-penugasan-id/{penugasanId}")]
        public async Task<IActionResult> GetKirimantByPenugasan(int penugasanId)
        {
            try
            {
                var result = await _penugasan.GetKirimanByPenugasan(penugasanId);

                if (result == null)
                {
                    return NotFound(new { message = "Penugasan tidak ditemukan" });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    ex.Message,
                    ex.StackTrace
                });
            }
        }

        [HttpPut("selesaikan-penugasan/{jadwalId}")]
        public async Task<IActionResult> UpdateStatusPenugasan(int jadwalId)
        {
            try
            {
                var result = await _penugasan.UpdateStatusPenugasan(jadwalId);

                if (result.NotFound)
                    return NotFound(new { message = result.Message });

                if (!result.Success)
                    return BadRequest(new { message = result.Message });

                return Ok(new
                {
                    message = result.Message,
                    jadwal_id = result.JadwalId,
                    status = result.Status
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("update-status-pengiriman")]
        public async Task<IActionResult> UpdateStatusKiriman([FromForm] UpdateStatusPengirimanDto request)
        {
            try
            {
                var result = await _penugasan.UpdateStatusKiriman(request);

                if (result.NotFound)
                    return NotFound(new { message = result.Message });

                if (!result.Success)
                    return BadRequest(new { message = result.Message });

                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
