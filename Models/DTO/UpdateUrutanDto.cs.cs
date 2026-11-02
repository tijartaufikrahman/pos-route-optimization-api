namespace SistemOptimasiWebAPI.Models.DTO
{
    public class UpdateUrutanDto
    {
        public int jadwalId { get; set; }
        public List<int> paketIds { get; set; }
    }
}