//public class DetailPenugasanPaketDto
//{
//    public int id { get; set; }

//    public string? no_kiriman { get; set; }

//    public string? recipient_name { get; set; }

//    public string? telepon { get; set; }

//    public string? address { get; set; }

//    public string? paket_status { get; set; }

//    public double? latitude { get; set; }

//    public double? longitude { get; set; }

//    public bool isOptimized { get; set; }
//}

//public class DetailPenugasanDto
//{
//    public int urutan_id { get; set; }

//    public int? nomor_urutan { get; set; }

//    public string? status { get; set; }

//    public DetailPenugasanPaketDto? paket { get; set; }
//}

//public class ByPenugasanResponseDto
//{
//    public int jadwal_id { get; set; }

//    public int penugasan_id { get; set; }

//    public string? kode_penugasan { get; set; }

//    public string? jadwal_status { get; set; }

//    public bool isOptimized { get; set; }

//    public int user_id { get; set; }

//    public string? user_nama { get; set; }

//    public DateTime tanggal { get; set; }

//    public int total { get; set; }

//    public List<DetailPenugasanDto> data { get; set; } = new();
//}



public class DetailPenugasanPodDto
{
    public int id { get; set; }

    public DateTime? waktu_pengantaran { get; set; }

    public string? koordinat { get; set; }

    public double? latitude { get; set; }

    public double? longitude { get; set; }

    public string? foto_bukti { get; set; }
}

public class DetailPenugasanPaketDto
{
    public int id { get; set; }

    public string? no_kiriman { get; set; }

    public string? recipient_name { get; set; }

    public string? telepon { get; set; }

    public string? address { get; set; }

    public string? jenis_kiriman { get; set; }

    public string? jenis_layanan { get; set; }

    public decimal? berat_kiriman { get; set; }

    public string? paket_status { get; set; }

    public double? latitude { get; set; }

    public double? longitude { get; set; }

    public bool isOptimized { get; set; }

    public DetailPenugasanPodDto? pod { get; set; }
}

public class DetailPenugasanDto
{
    public int urutan_id { get; set; }

    public int? nomor_urutan { get; set; }

    public string? status { get; set; }

    public DetailPenugasanPaketDto? paket { get; set; }
}

public class ByPenugasanResponseDto
{
    public int jadwal_id { get; set; }

    public int penugasan_id { get; set; }

    public string? kode_penugasan { get; set; }

    public string? jadwal_status { get; set; }

    public bool isOptimized { get; set; }

    public int user_id { get; set; }

    public string? user_nama { get; set; }

    public DateTime tanggal { get; set; }

    public int total { get; set; }

    public List<DetailPenugasanDto> data { get; set; } = new();
}