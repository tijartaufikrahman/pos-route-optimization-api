namespace SistemOptimasiWebAPI.Interfaces
{
    public interface IProofOfDelivery
    {
        Task<List<BuktiPengantaran>> GetAllPod();
        Task<List<UrutanPodDto>> GetUrutanPod(DateOnly tanggal);
        Task<BuktiPengantaran?> GetPodByKirimanIdAsync(int kirimanId);
        Task<List<BuktiPengantaran>> AddRangePod(List<BuktiPengantaran> data);
        Task<List<PodExcelRow>> ParseExcelPod(IFormFile file);
        Task<List<BuktiPengantaran>> BulkInsertPod(List<PodBulkInsertRequest> request);
        Task<int> DeleteAllPod();
        Task<string> UploadFileAsync(IFormFile file, string? nama);
        Task<(byte[] Bytes, string ContentType)?> GetFileAsync(string fileName);
        Task DeleteFileAsync(string? fileName);
    }
}
