using Microsoft.AspNetCore.Mvc;
using SistemOptimasiWebAPI.Interfaces;

namespace SistemOptimasiWebAPI.Controllers
{
    [ApiController]
    [Route("api/pod")]
    public class ProofOfDeliveryController : ControllerBase
    {
        private readonly IProofOfDelivery _repository;

        public ProofOfDeliveryController(IProofOfDelivery repository)
        {
            _repository = repository;
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var data = await _repository.GetAllPod();

                return Ok(new
                {
                    success = true,
                    total = data.Count,
                    data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpGet("urutan-pengantaran")]
        public async Task<IActionResult> GetUrutanPod([FromQuery] DateOnly tanggal)
        {
            var data = await _repository.GetUrutanPod(tanggal);
            return Ok(new { data });
        }

        [HttpPost("cek-by-kiriman")]
        public async Task<IActionResult> CekPodByKiriman([FromBody] CekPodRequest request)
        {
            try
            {
                if (request == null || request.KirimanId <= 0)
                {
                    return BadRequest(new
                    {
                        status = false,
                        message = "Id kiriman tidak valid."
                    });
                }

                var pod = await _repository.GetPodByKirimanIdAsync(request.KirimanId);

                if (pod == null)
                {
                    return Ok(new
                    {
                        status = false,
                        data = false,
                        message = "Data POD tidak ditemukan untuk kiriman ini."
                    });
                }

                return Ok(new
                {
                    status = true,
                    data = pod
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = false,
                    message = ex.Message
                });
            }
        }

        [HttpPost("tambahdatapod")]
        public async Task<IActionResult> TambahDataPOD([FromBody] List<BuktiPengantaran> data)
        {
            try
            {
                if (data == null || !data.Any())
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Data tidak boleh kosong."
                    });
                }

                var podList = await _repository.AddRangePod(data);

                return Ok(new
                {
                    success = true,
                    total = podList.Count,
                    message = "Data POD berhasil disimpan."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message,
                    innerException = ex.InnerException?.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file, string? nama)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "File tidak ditemukan"
                    });
                }
                var fileName = await _repository.UploadFileAsync(file, nama);

                return Ok(new
                {
                    success = true,
                    fileName
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpGet("view/{fileName}")]
        public async Task<IActionResult> LihatFile(string fileName)
        {
            try
            {
                var result = await _repository.GetFileAsync(fileName);

                if (result == null)
                {
                    return NotFound();
                }

                return File(result.Value.Bytes, result.Value.ContentType);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpDelete("{fileName}")]
        public async Task<IActionResult> HapusFile(string fileName)
        {
            try
            {
                await _repository.DeleteFileAsync(fileName);

                return Ok(new
                {
                    success = true,
                    message = "Proses hapus selesai"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        // EXTERNAL: Upload & parse Excel POD
        [HttpPost("upload-excel-pod")]
        public async Task<IActionResult> UploadExcel(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("File kosong");

                var kirimanList = await _repository.ParseExcelPod(file);

                return Ok(new
                {
                    total = kirimanList.Count,
                    data = kirimanList
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpPost("bulk-insert-pod")]
        public async Task<IActionResult> BulkInsertPod([FromBody] List<PodBulkInsertRequest> request)
        {
            try
            {
                if (request == null || !request.Any())
                {
                    return BadRequest(new { message = "Data tidak boleh kosong" });
                }

                var podList = await _repository.BulkInsertPod(request);

                if (!podList.Any())
                {
                    return BadRequest(new { message = "Tidak ada data valid untuk disimpan" });
                }

                return Ok(new
                {
                    message = "Data POD berhasil disimpan ke database",
                    total_request = request.Count,
                    total_inserted = podList.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpDelete("hapus-all")]
        public async Task<IActionResult> HapusSemuaPOD()
        {
            try
            {
                var total = await _repository.DeleteAllPod();

                if (total == 0)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "Tidak ada data Proof Of Delivery."
                    });
                }

                return Ok(new
                {
                    success = true,
                    total,
                    message = "Semua data Proof Of Delivery berhasil dihapus."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
    }
}
