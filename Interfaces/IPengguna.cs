namespace SistemOptimasiWebAPI.Interfaces
{
    public interface IPengguna
    {
        // GET: Ambil semua pengguna + Nama Role-nya
        Task<List<PenggunaDto>> GetPengguna();

        // GET: Ambil satu pengguna berdasarkan id
        Task<Pengguna?> GetPenggunaById(int id);

        // CEK: Apakah username sudah dipakai
        Task<bool> IsUsernameExists(string username);

        // POST: Tambah pengguna baru
        Task<Pengguna> AddPengguna(Pengguna userData);

        // PUT: Update data pengguna (return null jika id tidak ditemukan)
        Task<Pengguna?> UpdatePengguna(int id, Pengguna userData);

        // DELETE: Hapus pengguna (return true jika berhasil dihapus)
        Task<bool> DeletePengguna(int id);
    }
}
