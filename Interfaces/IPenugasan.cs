namespace SistemOptimasiWebAPI.Interfaces
{
    public interface IPenugasan
    {
        // GET: Daftar penugasan per rentang tanggal (opsional filter user & status)
        Task<List<PenugasanListItemDto>> GetPenugasan(DateOnly tglAwal, DateOnly tglAkhir, int? userId, string? status);

        // GET: Info dashboard kurir (total, proses, delivered, returned, selesai)
        Task<DashboardInfoDto> GetDashboardInfo(int userId);

        // DELETE: Hapus penugasan + kembalikan status paket (return success, message)
        Task<(bool Success, string? Message)> HapusPenugasan(int jadwalId);

        // POST: Tugaskan seluruh kiriman manifested ke pengantaran.
        Task<(bool Success, bool NotFound, string? Message, AssignToDeliveryResponseDto? Data)> TugaskanKiriman(AssignToDeliveryRequest request);

        // GET: Detail kiriman per penugasan (return null jika penugasan tidak ditemukan)
        Task<ByPenugasanResponseDto?> GetKirimanByPenugasan(int penugasanId);

        // PUT: Selesaikan penugasan (return success, notfound, message, jadwalId, status)
        Task<(bool Success, bool NotFound, string? Message, int JadwalId, string? Status)> UpdateStatusPenugasan(int jadwalId);

        // POST: Update status pengiriman + simpan bukti (return success, notfound, message, data)
        Task<(bool Success, bool NotFound, string? Message, UpdateStatusPengirimanResponseDto? Data)> UpdateStatusKiriman(UpdateStatusPengirimanDto request);
    }
}
