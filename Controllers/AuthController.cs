using Microsoft.AspNetCore.Mvc;
using SistemOptimasiWebAPI.DTOs;
using SistemOptimasiWebAPI.Interfaces;

namespace SistemOptimasiWebAPI.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuth _auth;

        public AuthController(IAuth auth)
        {
            _auth = auth;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (request.RoleId <= 0)
                    return BadRequest(new { Message = "RoleId wajib dikirim pada request login" });

                var result = await _auth.Login(request.Username, request.Password, request.RoleId);
                if (result == null) return Unauthorized(new { Message = "Login gagal!" });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Terjadi kesalahan saat login", Error = ex.Message });
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            try
            {
                var result = await _auth.ForgotPassword(request.Email);

                if (result.Success) return Ok(new { Message = result.Message });
                if (!string.IsNullOrEmpty(result.Error))
                    return BadRequest(new { Message = result.Message, Error = result.Error });

                return NotFound(new { Message = result.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Terjadi kesalahan saat mengirim OTP", Error = ex.Message });
            }
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromForm] string email, [FromForm] string otp)
        {
            try
            {
                var result = await _auth.VerifyOtp(email, otp);

                if (result.Success) return Ok(new { Success = true, Message = result.Message });

                return BadRequest(new { Success = false, Message = result.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromForm] string email, [FromForm] string otp, [FromForm] string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(password))
                    return BadRequest(new { Success = false, Message = "Password baru tidak boleh kosong" });

                var result = await _auth.ResetPassword(email, otp, password);

                if (result.Success) return Ok(new { Success = true, Message = result.Message });

                return BadRequest(new { Success = false, Message = result.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }
    }
}
