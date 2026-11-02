public class PenugasanListItemDto
{
    public int id { get; set; }

    public DateTime tanggal { get; set; }

    public string? namaKurir { get; set; }

    public string? status { get; set; }

    public string? kodePenugasan { get; set; }

    public int jumlahPaket { get; set; }

    public double total_berat_kiriman { get; set; }

    public bool isOptimized { get; set; }
}

public class DashboardInfoDto
{
    public int total { get; set; }

    public int proses { get; set; }

    public int delivered { get; set; }

    public int returned { get; set; }

    public int selesai { get; set; }

    public int total_penugasan { get; set; }
}
