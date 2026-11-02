using Microsoft.EntityFrameworkCore;
using SistemOptimasiWebAPI.Interfaces;

public class RolesRepository : IRoles
{
    private readonly AppDbContext _context;

    public RolesRepository(AppDbContext context)
    {
        _context = context;
    }

    // GET: Semua role + jumlah user
    public async Task<List<RoleListItemDto>> GetAllRoles()
    {
        return await _context.Role.Select(r => new RoleListItemDto
        {
            Id = r.Id,
            NamaRole = r.NamaRole,
            // Kita hitung jumlah user dari tabel Users yang punya role_id sama
            JumlahUser = _context.Pengguna.Count(u => u.RoleId == r.Id)
        }).ToListAsync();
    }

    // GET: Role by id
    public async Task<RoleDto?> GetRoleById(int id)
    {
        return await _context.Role
            .Select(r => new RoleDto { Id = r.Id, NamaRole = r.NamaRole })
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    // POST: Tambah role
    public async Task<(bool Success, string? Message, RoleDto? Data)> CreateRole(string? namaRole)
    {
        if (string.IsNullOrWhiteSpace(namaRole))
            return (false, "Nama Role tidak boleh kosong", null);

        // Cek duplikat
        var exists = await _context.Role
            .AnyAsync(r => r.NamaRole.ToLower() == namaRole.ToLower());

        if (exists) return (false, "Nama Role tersebut sudah ada", null);

        var newRole = new Role { NamaRole = namaRole };
        _context.Role.Add(newRole);
        await _context.SaveChangesAsync();

        return (true, "Role berhasil ditambahkan", new RoleDto
        {
            Id = newRole.Id,
            NamaRole = newRole.NamaRole
        });
    }

    // PUT: Update role
    public async Task<(bool Success, bool NotFound, string? Message)> UpdateRole(int id, string? namaRole)
    {
        var role = await _context.Role.FindAsync(id);
        if (role == null) return (false, true, "Role tidak ditemukan");

        if (string.IsNullOrWhiteSpace(namaRole))
            return (false, false, "Nama Role tidak boleh kosong");

        role.NamaRole = namaRole;
        await _context.SaveChangesAsync();

        return (true, false, "Role berhasil diperbarui");
    }

    // DELETE: Hapus role
    public async Task<(bool Success, bool NotFound, string? Message)> DeleteRole(int id)
    {
        var role = await _context.Role.FindAsync(id);
        if (role == null) return (false, true, "Role tidak ditemukan");

        // Cek apakah ada user yang pakai role ini sebelum hapus
        var isUsed = await _context.Pengguna.AnyAsync(u => u.RoleId == id);
        if (isUsed)
        {
            return (false, false, "Gagal hapus! Role ini masih digunakan oleh beberapa pengguna.");
        }

        _context.Role.Remove(role);
        await _context.SaveChangesAsync();

        return (true, false, "Role berhasil dihapus");
    }
}
