namespace SistemOptimasiWebAPI.Interfaces
{
    public interface IKantor
    {
        
        Task<Kantor?> GetKantor();
        Task<(bool Success, string? Message, int Id)> UpsertKantor(LokasiKantorDto request);
        Task<bool> DeleteKantor();
    }
}
