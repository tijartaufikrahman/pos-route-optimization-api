using SistemOptimasiWebAPI.Models.DTO;

namespace SistemOptimasiWebAPI.Interfaces
{
    public interface IOptimization
    {
        // POST: Hitung matrix jarak + jalankan optimasi (ACO/GA + 2-opt)
        Task<OptimizationResultDto> HitungJarakMatrixRunOptimasi(OptimizationRequestDto request);

        // POST: Route polyline per pasangan titik
        Task<RoutePolylineResponseDto> RoutePolyline(RouteRequestDto request);

        // GET: Route statis (dummy)
        RoutePolylineResponseDto GetStaticRoute();

        // POST: Hasil optimasi (matrix + polyline)
        Task<CalculateResultDto> GetHasilOptimasi(CalculateRequestDto request);

        // POST: Simpan urutan optimasi (return success, message, total)
        Task<(bool Success, string? Message, int Total)> SimpanOptimasi(UpdateUrutanDto request);
    }
}
