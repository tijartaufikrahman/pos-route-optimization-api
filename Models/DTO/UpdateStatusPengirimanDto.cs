public class UpdateStatusPengirimanDto
{
    public int UrutanId { get; set; }

    public string Status { get; set; } = string.Empty;

    public string? NamaPenerima { get; set; }

    public string? StatusPenerima { get; set; }

    public IFormFile? PodPhoto { get; set; }

    public double? PodLatitude { get; set; }

    public double? PodLongitude { get; set; }

    public DateTime? PodDateTime { get; set; }
}