public class UrutanPodDto
{
    public int id { get; set; }
    public string? no_kiriman { get; set; }
    public string? recipient_name { get; set; }
    public string? telepon_penerima { get; set; }
    public string? address { get; set; }
    public DateTime waktu_pod { get; set; }
    public double latitude { get; set; }
    public double longitude { get; set; }
}
