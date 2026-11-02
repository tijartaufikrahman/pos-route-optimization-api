using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

//[Table("BuktiPengantaran")]
[Table("ProofOfDelivery")]
public class BuktiPengantaran
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    
    [Column("kiriman_id")]
    public int? KirimanId { get; set; }

    [Column("nama_penerima")]
    public string? NamaPenerima { get; set; }

    [Column("waktu_pengantaran")]
    public DateTime? WaktuPengantaran { get; set; }

    [Column("koordinat")]
    [StringLength(100)]
    public string? Koordinat { get; set; }

    [Column("latitude")]
    public double? Latitude { get; set; }

    [Column("longitude")]
    public double? Longitude { get; set; }

    [Column("foto_bukti")]
    [StringLength(255)]
    public string? FotoBukti { get; set; }
    
    [Column("nomor_kiriman")]    
    public string? NomorKiriman { get; set; }
    
    [Column("telepon_penerima")]    
    public string? Telepon { get; set; }

    [Column("alamat_penerima")]
    public string? AlamatPenerima { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [Column("created_by")]
    public int? CreatedBy { get; set; }

    [Column("updated_by")]
    public int? UpdatedBy { get; set; }

    /* Navigation Property */

    [ForeignKey(nameof(KirimanId))]
    public virtual Kiriman? Kiriman { get; set; }

    // lama 
    //public Paket? Paket { get; set; }
}