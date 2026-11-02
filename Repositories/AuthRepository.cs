using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SistemOptimasiWebAPI.DTOs;
using SistemOptimasiWebAPI.Interfaces;

public class AuthRepository : IAuth
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthRepository(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }
    
    public async Task<LoginResponse?> Login(string username, string password, int roleId)
    {
        // Query relasi user ke Role: hanya user yang role-nya sesuai dengan roleId
        // yang dikirim dari request yang dianggap cocok.
        var userWithRole = await (from u in _context.Pengguna
                                  join r in _context.Role on u.RoleId equals r.Id
                                  where u.Username == username && r.Id == roleId
                                  select new
                                  {
                                      u.Id,
                                      u.Nama,
                                      u.Username,
                                      u.RoleId,
                                      u.Password,
                                      RoleName = r.NamaRole
                                  }).FirstOrDefaultAsync();

        if (userWithRole == null || string.IsNullOrEmpty(password)) return null;

        // Verifikasi password (di database tersimpan sebagai hash BCrypt).
        // Jika hash di database bukan hash BCrypt yang valid (misalnya masih
        // plaintext dari user lama sebelum migrasi), login ditolak dan user
        // harus reset password melalui fitur lupa password.
        bool isValidPassword;
        try
        {
            isValidPassword = BCrypt.Net.BCrypt.Verify(password, userWithRole.Password);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            isValidPassword = false;
        }

        if (!isValidPassword)
        {
            return null;
        }

        string roleName = userWithRole.RoleName;
        string Nama = userWithRole.Nama ?? "Guest";

        // Generate Token 
        var dummyUser = new Pengguna { Id = userWithRole.Id, Username = userWithRole.Username };
        var token = GenerateJwtToken(dummyUser, roleName);

        return new LoginResponse
        {
            Id = userWithRole.Id,
            Token = token,
            Nama = Nama,
            Username = userWithRole.Username,
            RoleId = userWithRole.RoleId,
            Role = roleName,
            Message = "Login Berhasil"
        };
    }

    private string GenerateJwtToken(Pengguna user, string roleName)
    {
        var jwtKey = _configuration["Jwt:Key"] ?? "KunciRahasiaDefaultYangSangatPanjang123";
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, roleName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(24),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // FORGOT PASSWORD: buat OTP, simpan ke user, kirim email
    public async Task<AuthResult> ForgotPassword(string email)
    {
        var user = await _context.Pengguna.FirstOrDefaultAsync(x => x.Email == email);
        if (user == null)
        {
            return new AuthResult { Success = false, Message = "Email tidak ditemukan" };
        }

        var otp = new Random().Next(100000, 999999).ToString();

        user.ResetOtp = otp;
        user.ResetOtpExpiredAt = DateTime.Now.AddMinutes(5);

        await _context.SaveChangesAsync();

        try
        {
            await SendOtpEmail(user.Email!, otp);

            return new AuthResult { Success = true, Message = "OTP berhasil dikirim ke email" };
        }
        catch (Exception ex)
        {
            return new AuthResult { Success = false, Message = "Gagal mengirim email", Error = ex.Message };
        }
    }

    private async Task SendOtpEmail(string recipientEmail, string otp)
    {
        var host = _configuration["EmailSettings:Host"];
        var port = int.Parse(_configuration["EmailSettings:Port"]!);
        var senderEmail = _configuration["EmailSettings:Email"];
        var senderPassword = _configuration["EmailSettings:Password"];

        using var smtpClient = new SmtpClient(host)
        {
            Port = port,
            Credentials = new NetworkCredential(
                senderEmail,
                senderPassword
            ),
            EnableSsl = true
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(senderEmail!),
            Subject = "Reset Password Sistem Optimasi Rute",
            Body = $@"
                    Halo,

                    Kode OTP untuk reset password Anda adalah:

                    {otp}

                    OTP berlaku selama 5 menit.

                    Terima kasih.",
            IsBodyHtml = false
        };

        mailMessage.To.Add(recipientEmail);

        await smtpClient.SendMailAsync(mailMessage);
    }

    // VERIFY OTP: cek email, OTP, dan masa berlaku
    public async Task<AuthResult> VerifyOtp(string email, string otp)
    {
        var user = await _context.Pengguna.FirstOrDefaultAsync(x => x.Email == email);

        if (user == null)
        {
            return new AuthResult { Success = false, Message = "Email tidak ditemukan" };
        }

        if (string.IsNullOrEmpty(user.ResetOtp))
        {
            return new AuthResult { Success = false, Message = "OTP belum dibuat" };
        }

        if (user.ResetOtp != otp)
        {
            return new AuthResult { Success = false, Message = "OTP tidak valid" };
        }

        if (user.ResetOtpExpiredAt == null)
        {
            return new AuthResult { Success = false, Message = "OTP tidak valid" };
        }

        if (user.ResetOtpExpiredAt < DateTime.Now)
        {
            return new AuthResult { Success = false, Message = "OTP sudah kadaluarsa" };
        }

        return new AuthResult { Success = true, Message = "OTP valid" };
    }

    // RESET PASSWORD: validasi OTP lalu ubah password
    public async Task<AuthResult> ResetPassword(string email, string otp, string password)
    {
        var user = await _context.Pengguna
            .FirstOrDefaultAsync(x => x.Email == email);

        if (user == null)
        {
            return new AuthResult { Success = false, Message = "Email tidak ditemukan" };
        }

        if (string.IsNullOrEmpty(user.ResetOtp))
        {
            return new AuthResult { Success = false, Message = "OTP tidak ditemukan" };
        }

        if (user.ResetOtp != otp)
        {
            return new AuthResult { Success = false, Message = "OTP tidak valid" };
        }

        if (user.ResetOtpExpiredAt == null ||
            user.ResetOtpExpiredAt < DateTime.Now)
        {
            return new AuthResult { Success = false, Message = "OTP sudah kadaluarsa" };
        }

        // Simpan password baru dalam bentuk hash BCrypt
        user.Password = BCrypt.Net.BCrypt.HashPassword(password);

        user.ResetOtp = null;
        user.ResetOtpExpiredAt = null;

        await _context.SaveChangesAsync();

        return new AuthResult { Success = true, Message = "Password berhasil diubah" };
    }
}
