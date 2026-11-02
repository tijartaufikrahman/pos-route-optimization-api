using Microsoft.EntityFrameworkCore;
using SistemOptimasiWebAPI.Interfaces;

public class PenggunaRepository : IPengguna
{
    private readonly AppDbContext _context;

    public PenggunaRepository(AppDbContext context)
    {
        _context = context;
    }

    // GET: Ambil semua pengguna + Nama Role-nya
    public async Task<List<PenggunaDto>> GetPengguna()
    {
        return await (from u in _context.Pengguna
                      join r in _context.Role on u.RoleId equals r.Id
                      select new PenggunaDto
                      {
                          Id = u.Id,
                          Nama = u.Nama,
                          Username = u.Username,
                          RoleId = u.RoleId,
                          IsActive = u.IsActive, // Menampilkan status aktif
                          RoleName = r.NamaRole
                      }).ToListAsync();
    }

    // GET: Ambil satu pengguna berdasarkan id
    public async Task<Pengguna?> GetPenggunaById(int id)
    {
        return await _context.Pengguna.FindAsync(id);
    }

    // CEK: Apakah username sudah dipakai
    public async Task<bool> IsUsernameExists(string username)
    {
        return await _context.Pengguna.AnyAsync(u => u.Username == username);
    }

    // POST: Tambah pengguna baru
    public async Task<Pengguna> AddPengguna(Pengguna userData)
    {
        // Simpan password dalam bentuk hash BCrypt
        userData.Password = BCrypt.Net.BCrypt.HashPassword(userData.Password);

        _context.Pengguna.Add(userData);
        await _context.SaveChangesAsync();

        return userData;
    }

    // PUT: Update data pengguna (return null jika id tidak ditemukan)
    public async Task<Pengguna?> UpdatePengguna(int id, Pengguna userData)
    {
        var user = await _context.Pengguna.FindAsync(id);
        if (user == null) return null;

        user.Nama = userData.Nama;
        user.Username = userData.Username;
        user.RoleId = userData.RoleId;

        user.IsActive = userData.IsActive;

        if (!string.IsNullOrWhiteSpace(userData.Password))
        {
            // Simpan password baru dalam bentuk hash BCrypt
            user.Password = BCrypt.Net.BCrypt.HashPassword(userData.Password);
        }

        await _context.SaveChangesAsync();

        return user;
    }

    // DELETE: Hapus pengguna (return true jika berhasil dihapus)
    public async Task<bool> DeletePengguna(int id)
    {
        var user = await _context.Pengguna.FindAsync(id);
        if (user == null) return false;

        _context.Pengguna.Remove(user);
        await _context.SaveChangesAsync();

        return true;
    }
}
