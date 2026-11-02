using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("DetailPenugasan")]
public class DetailPenugasan
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("penugasan_id")]
    public int PenugasanId { get; set; }

    [Required]
    [Column("kiriman_id")]
    public int KirimanId { get; set; }

    
    [Column("nomor_urutan")]
    public int? NomorUrutan { get; set; }

    [Column("status_pengantaran")]
    [StringLength(50)]
    public string? StatusPengantaran { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [Column("created_by")]
    public int? CreatedBy { get; set; }

    [Column("updated_by")]
    public int? UpdatedBy { get; set; }

    /* Navigation Property */

    //[ForeignKey(nameof(PenugasanId))]
    //public virtual Penugasan Penugasan { get; set; }

    //[ForeignKey(nameof(KirimanId))]
    //public virtual Kiriman Kiriman { get; set; }

    // RELASI LAMA
    //public JadwalPengantaran JadwalPengantaran { get; set; }
    //public Paket Paket { get; set; }

    public Penugasan Penugasan { get; set; }
    public Kiriman Kiriman { get; set; }
}