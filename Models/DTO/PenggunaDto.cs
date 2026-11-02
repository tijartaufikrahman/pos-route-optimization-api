public class PenggunaDto
{
    public int Id { get; set; }

    public string Nama { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public int RoleId { get; set; }

    public bool IsActive { get; set; }

    public string? RoleName { get; set; }
}
