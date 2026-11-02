using SistemOptimasiWebAPI.DTOs;

namespace SistemOptimasiWebAPI.Interfaces
{
    public interface IAuth
    {
        
        Task<LoginResponse?> Login(string username, string password, int roleId);
        Task<AuthResult> ForgotPassword(string email);
        Task<AuthResult> VerifyOtp(string email, string otp);
        Task<AuthResult> ResetPassword(string email, string otp, string password);
    }
}
