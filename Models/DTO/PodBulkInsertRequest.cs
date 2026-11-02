public class PodBulkInsertRequest
{
    public string? nomor_kiriman { get; set; }
    public string? nama_penerima { get; set; }
    public string? alamat_penerima { get; set; }
    public string? telepon_penerima { get; set; }
    public string? delivery { get; set; } // Dari Excel masih berupa teks (string)
    public string? koordinat { get; set; }
    public double? latitude { get; set; }
    public double? longitude { get; set; }
}