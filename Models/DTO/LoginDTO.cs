namespace SistemOptimasiWebAPI.DTOs
{
    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }

        // RoleId dikirim dari frontend, dipakai untuk validasi relasi ke tabel Role
        public int RoleId { get; set; }
    }

    public class LoginResponse
    {
        public int Id { get; set; }
        public string Token { get; set; }
        public string Nama { get; set; }
        public string Username { get; set; }
        public int RoleId { get; set; }
        
        public string Role { get; set; } 
        public string Message { get; set; }
    }
}