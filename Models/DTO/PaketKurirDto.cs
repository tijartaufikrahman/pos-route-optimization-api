public class PaketKurirDto
{
    public int id { get; set; }

    public string? no_kiriman { get; set; }

    public string? recipient_name { get; set; }

    public string? telepon { get; set; }

    public string? address { get; set; }

    public double? latitude { get; set; }

    public double? longitude { get; set; }

    public bool is_delivered { get; set; }
}

public class DetailPenugasanKurirDto
{
    public int urutan_id { get; set; }

    public int? nomor_urutan { get; set; }

    public string? status { get; set; }

    public PaketKurirDto? paket { get; set; }
}

public class ByKurirResponseDto
{
    public int jadwal_id { get; set; }

    public int user_id { get; set; }

    public DateTime tanggal { get; set; }

    public int total { get; set; }

    public List<DetailPenugasanKurirDto> data { get; set; } = new();
}
