using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Kantor")]
public class Kantor
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("nama")]
    [StringLength(200)]
    public string Nama { get; set; } = string.Empty;

    [Required]
    [Column("kode_pos")]
    [StringLength(10)]
    public string KodePos { get; set; } = string.Empty;

    [Column("alamat")]
    public string? Alamat { get; set; }

    [Required]
    [Column("latitude")]
    public double Latitude { get; set; }

    [Required]
    [Column("longitude")]
    public double Longitude { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [Column("created_by")]
    public int? CreatedBy { get; set; }

    [Column("updated_by")]
    public int? UpdatedBy { get; set; }

    
}