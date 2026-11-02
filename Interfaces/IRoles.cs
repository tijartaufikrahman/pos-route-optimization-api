namespace SistemOptimasiWebAPI.Interfaces
{
    public interface IRoles
    {
        // GET: Semua role + jumlah user
        Task<List<RoleListItemDto>> GetAllRoles();

        // GET: Role by id (return null jika tidak ditemukan)
        Task<RoleDto?> GetRoleById(int id);

        // POST: Tambah role (return success, message, data)
        Task<(bool Success, string? Message, RoleDto? Data)> CreateRole(string? namaRole);

        // PUT: Update role (return success, notfound, message)
        Task<(bool Success, bool NotFound, string? Message)> UpdateRole(int id, string? namaRole);

        // DELETE: Hapus role (return success, notfound, message)
        Task<(bool Success, bool NotFound, string? Message)> DeleteRole(int id);
    }
}
