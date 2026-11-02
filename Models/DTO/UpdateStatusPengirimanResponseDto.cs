public class PodInfoDto
{
    public string? foto { get; set; }

    public double? latitude { get; set; }

    public double? longitude { get; set; }

    public DateTime datetime { get; set; }
}

public class UpdateStatusPengirimanResponseDto
{
    public string message { get; set; } = string.Empty;

    public int urutan_id { get; set; }

    public int kiriman_id { get; set; }

    public string? status_pengantaran { get; set; }

    public string? paket_status { get; set; }

    public PodInfoDto pod { get; set; } = new();
}
