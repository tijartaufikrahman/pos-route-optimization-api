using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Kiriman")]
public class Kiriman
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("jenis_kiriman")]
    [StringLength(100)]
    public string? JenisKiriman { get; set; }

    [Column("jenis_layanan")]
    [StringLength(100)]
    public string? JenisLayanan { get; set; }

    [Required]
    [Column("nomor_kiriman")]
    [StringLength(50)]
    public string NomorKiriman { get; set; } = string.Empty;

    [Required]
    [Column("nama_penerima")]
    [StringLength(100)]
    public string NamaPenerima { get; set; } = string.Empty;

    [Required]
    [Column("alamat_penerima")]
    public string AlamatPenerima { get; set; } = string.Empty;

    [Column("latitude")]
    public double? Latitude { get; set; }

    [Column("longitude")]
    public double? Longitude { get; set; }

    [Column("telepon_penerima")]
    [StringLength(20)]
    public string? TeleponPenerima { get; set; }

    [Required]
    [Column("status")]
    [StringLength(20)]
    public string Status { get; set; } = string.Empty;

    [Column("berat_kiriman")]
    public decimal? BeratKiriman { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [Column("created_by")]
    public int? CreatedBy { get; set; }

    [Column("updated_by")]
    public int? UpdatedBy { get; set; }

    /* Navigation Property */
    public DetailPenugasan DetailPenugasan { get; set; } = null!;
    public BuktiPengantaran? POD { get; set; }
}