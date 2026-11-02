using Microsoft.AspNetCore.Mvc;
using SistemOptimasiWebAPI.Interfaces;
using SistemOptimasiWebAPI.Models.DTO;

namespace SistemOptimasiWebAPI.Controllers
{
    [ApiController]
    [Route("api/optimization")]
    public class OptimizationController : ControllerBase
    {
        private readonly IOptimization _optimization;

        public OptimizationController(IOptimization optimization)
        {
            _optimization = optimization;
        }

        [HttpPost("run")]
        public async Task<IActionResult> HitungJarakMatrixRunOptimasi([FromBody] OptimizationRequestDto request)
        {
            try
            {
                if (request == null || request.pakets == null || request.pakets.Count == 0)
                    return BadRequest("pakets kosong");

                var result = await _optimization.HitungJarakMatrixRunOptimasi(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("route")]
        public async Task<IActionResult> RoutePolyline([FromBody] RouteRequestDto request)
        {
            try
            {
                if (request.locations == null || request.locations.Count < 2)
                    return BadRequest("Minimal 2 titik");

                var result = await _optimization.RoutePolyline(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("route-static")]
        public IActionResult GetStaticRoute()
        {
            try
            {
                var result = _optimization.GetStaticRoute();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("calculate")]
        public async Task<IActionResult> GetHasilOptimasi([FromBody] CalculateRequestDto request)
        {
            try
            {
                if (request.locations == null || request.locations.Count < 1)
                    return BadRequest("Lokasi minimal 1");

                var result = await _optimization.GetHasilOptimasi(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("save-urutan")]
        public async Task<IActionResult> SimpanOptimasi([FromBody] UpdateUrutanDto request)
        {
            try
            {
                var result = await _optimization.SimpanOptimasi(request);

                if (!result.Success)
                    return BadRequest(result.Message);

                return Ok(new
                {
                    message = result.Message,
                    total = result.Total
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Terjadi kesalahan",
                    error = ex.Message
                });
            }
        }
    }
}
