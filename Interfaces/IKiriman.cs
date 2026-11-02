namespace SistemOptimasiWebAPI.Interfaces
{
    public interface IKiriman
    {
        
        Task<List<KirimanListItemDto>> GetKiriman();       
        Task<List<KirimanListDto>> GetListKiriman(DateOnly? tanggalAwal, DateOnly? tanggalAkhir, string? status);
        Task<ByKurirResponseDto?> GetPaketByKurir(int userId);
        Task<List<PreviewExcelRow>> PreviewDataExcel(IFormFile file);
        Task<List<Kiriman>> BulkInsertKiriman(List<PaketBulkInsertRequest> request);
        Task<bool> UpdateLokasiAsync(int idKiriman, double latitude, double longitude);
        Task<bool> DeleteKiriman(int id);
        Task<(bool Success, string? Message, int Total, string? Error)> DeleteAllKiriman();
    }
}
