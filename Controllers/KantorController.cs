using Microsoft.AspNetCore.Mvc;
using SistemOptimasiWebAPI.Interfaces;

namespace SistemOptimasiWebAPI.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("api/lokasi-kantor")]
    public class KantorController : ControllerBase
    {
        private readonly IKantor _repository;

        public KantorController(IKantor repository)
        {
            _repository = repository;
        }

        [HttpGet("getKantor")]
        public async Task<IActionResult> getKantor()
        {
            try
            {
                var query = await _repository.GetKantor();
                return Ok(query);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("setting")]
        public async Task<IActionResult> GetDataLokasiKantor()
        {
            try
            {
                var kantor = await _repository.GetKantor();
                if (kantor == null) return NotFound(new { message = "Data belum ada." });
                return Ok(kantor);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("setting")]
        public async Task<IActionResult> UpdateDataKantor([FromBody] LokasiKantorDto request)
        {
            if (request == null) return BadRequest(new { message = "Request tidak valid" });

            try
            {
                var result = await _repository.UpsertKantor(request);

                if (result.Success) return Ok(new { message = "Simpan Berhasil!", id = result.Id });

                return BadRequest(new { message = result.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Format koordinat salah atau gagal simpan: " + ex.Message });
            }
        }

        [HttpDelete("setting")]
        public async Task<IActionResult> HapusDataKantor()
        {
            try
            {
                var deleted = await _repository.DeleteKantor();

                if (!deleted)
                {
                    return NotFound(new { message = "Data kantor tidak ditemukan." });
                }

                return Ok(new { message = "Data kantor berhasil dihapus." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Gagal menghapus data: " + ex.Message });
            }
        }
    }
}
