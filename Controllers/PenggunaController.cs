using Microsoft.AspNetCore.Mvc;
using SistemOptimasiWebAPI.Interfaces;

namespace SistemOptimasiWebAPI.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class PenggunaController : ControllerBase
    {
        private readonly IPengguna _pengguna;

        public PenggunaController(IPengguna pengguna)
        {
            _pengguna = pengguna;
        }

        // 1. GET: Ambil semua pengguna + Nama Role-nya
        [HttpGet]
        public async Task<IActionResult> GetPengguna()
        {
            try
            {
                var data = await _pengguna.GetPengguna();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Terjadi kesalahan saat mengambil data", error = ex.Message });
            }
        }

        [HttpGet("{id}")]   
        public async Task<IActionResult> GetPenggunaById(int id)
        {
            try
            {
                var user = await _pengguna.GetPenggunaById(id);
                if (user == null) return NotFound(new { message = "Data tidak ditemukan" });
                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Terjadi kesalahan saat mengambil data", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> TambahPengguna([FromBody] Pengguna userData)
        {
            try
            {
                if (userData == null) return BadRequest();

                if (string.IsNullOrWhiteSpace(userData.Password))
                    return BadRequest(new { message = "Password tidak boleh kosong" });

                var isExists = await _pengguna.IsUsernameExists(userData.Username);
                if (isExists) return BadRequest(new { message = "Username sudah digunakan" });

                await _pengguna.AddPengguna(userData);

                return Ok(new { message = "Pengguna berhasil ditambahkan" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Terjadi kesalahan saat menambahkan pengguna", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePengguna(int id, [FromBody] Pengguna userData)
        {
            try
            {
                var user = await _pengguna.UpdatePengguna(id, userData);
                if (user == null) return NotFound();

                return Ok(new { message = "Data berhasil diperbarui" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Terjadi kesalahan saat memperbarui data", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> HapusPengguna(int id)
        {
            try
            {
                var isDeleted = await _pengguna.DeletePengguna(id);
                if (!isDeleted) return NotFound();

                return Ok(new { message = "Data berhasil dihapus" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Terjadi kesalahan saat menghapus data", error = ex.Message });
            }
        }
    }
}
