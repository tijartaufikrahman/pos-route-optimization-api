public class KirimanListItemDto
{
    public int id { get; set; }

    public string? no_kiriman { get; set; }

    public string? recipient_name { get; set; }

    public string? telepon_penerima { get; set; }

    public string? address { get; set; }

    public string? kriteria { get; set; }

    public int kriteriaId { get; set; }

    public string? service { get; set; }

    public int serviceLayanan { get; set; }

    public decimal? berat_kiriman { get; set; }

    public double? latitude { get; set; }

    public double? longitude { get; set; }

    public string? koordinat { get; set; }

    public string? status { get; set; }
}

public class KirimanListDto
{
    public int id { get; set; }

    public string? no_kiriman { get; set; }

    public string? recipient_name { get; set; }

    public string? telepon_penerima { get; set; }

    public string? address { get; set; }

    public string? kriteria { get; set; }

    public int kriteriaId { get; set; }

    public string? service { get; set; }

    public int serviceLayanan { get; set; }

    public decimal? berat_kiriman { get; set; }

    public DateTime? created_at { get; set; }

    public double? latitude { get; set; }

    public double? longitude { get; set; }

    public string? koordinat { get; set; }

    public int isAction { get; set; }

    public string? status { get; set; }
}
