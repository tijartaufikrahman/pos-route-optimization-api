public class AssignToDeliveryResponseDto
{
    public string message { get; set; } = string.Empty;

    public int jadwal_id { get; set; }

    public string? kode_penugasan { get; set; }

    public int total_request { get; set; }

    public List<int> inserted { get; set; } = new();

    public List<int> skipped { get; set; } = new();
}
