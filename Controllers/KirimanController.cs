using Microsoft.AspNetCore.Mvc;
using SistemOptimasiWebAPI.Interfaces;

namespace SistemOptimasiWebAPI.Controllers
{
    [ApiController]
    [Route("api/paket")]
    public class KirimanController : ControllerBase
    {
        private readonly IKiriman _kirimanRepository;

        public KirimanController(IKiriman kirimanRepository)
        {
            _kirimanRepository = kirimanRepository;
        }

        // Req di page tambah penugasan
        [HttpGet]
        public async Task<IActionResult> GetKiriman()
        {
            try
            {
                var data = await _kirimanRepository.GetKiriman();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // Req di page datapaket
        [HttpGet("list-kiriman")]
        public async Task<IActionResult> GetListKiriman(
            [FromQuery] DateOnly? tanggalAwal,
            [FromQuery] DateOnly? tanggalAkhir,
            [FromQuery] string? status)
        {
            try
            {
                if (tanggalAwal.HasValue && tanggalAkhir.HasValue && tanggalAwal.Value > tanggalAkhir.Value)
                {
                    return BadRequest(new { message = "Tanggal awal tidak boleh melebihi tanggal akhir." });
                }

                var data = await _kirimanRepository.GetListKiriman(tanggalAwal, tanggalAkhir, status);
                return Ok(new { data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> HapusKiriman(int id)
        {
            try
            {
                var deleted = await _kirimanRepository.DeleteKiriman(id);

                if (!deleted)
                {
                    return NotFound(new { message = "Paket tidak ditemukan" });
                }

                return Ok(new
                {
                    message = "Paket berhasil dihapus",
                    id = id
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        //rencana dihapus
        [HttpGet("by-kurir/{userId}")]
        public async Task<IActionResult> GetPaketByKurir(int userId)
        {
            try
            {
                var result = await _kirimanRepository.GetPaketByKurir(userId);

                if (result == null)
                {
                    return NotFound(new { message = "Belum ada jadwal pengantaran hari ini" });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("upload-excel")]
        public async Task<IActionResult> PreviewDataExcel(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("File kosong");

                var kirimanList = await _kirimanRepository.PreviewDataExcel(file);

                return Ok(new
                {
                    total = kirimanList.Count,
                    data = kirimanList
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("bulk-insert")]
        public async Task<IActionResult> TambahDataKiriman([FromBody] List<PaketBulkInsertRequest> request)
        {
            try
            {
                if (request == null || !request.Any())
                {
                    return BadRequest(new
                    {
                        message = "Data tidak boleh kosong"
                    });
                }

                var paketBaruList = await _kirimanRepository.BulkInsertKiriman(request);

                if (!paketBaruList.Any())
                {
                    return BadRequest(new
                    {
                        message = "Tidak ada data valid untuk disimpan"
                    });
                }

                return Ok(new
                {
                    message = "Data berhasil disimpan",
                    total_request = request.Count,
                    total_inserted = paketBaruList.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }



        [HttpPut("update-lokasi")]
        public async Task<IActionResult> UpdateLokasi([FromBody] UpdateLokasiDto request)
        {
            try
            {
                if (request == null)
                    return BadRequest(new { message = "Request tidak boleh kosong" });

                var result = await _kirimanRepository.UpdateLokasiAsync(
                    request.IdKiriman,
                    (double)request.Latitude,
                    (double)request.Longitude
                );

                if (!result)
                    return NotFound(new { message = "Kiriman tidak ditemukan" });

                return Ok(new { message = "Lokasi berhasil diperbarui" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("delete-all")]
        public async Task<IActionResult> HapusSemuaKiriman()
        {
            var result = await _kirimanRepository.DeleteAllKiriman();

            if (result.Success && result.Total == 0)
                return Ok(new { success = true, message = "Tidak ada data kiriman yang dihapus." });

            if (result.Success)
                return Ok(new { success = true, message = "Semua data kiriman berhasil dihapus.", total = result.Total });

            if (!string.IsNullOrEmpty(result.Error))
                return Ok(new { success = false, message = result.Message, error = result.Error });

            return Ok(new { success = false, message = result.Message });
        }
    }
}
