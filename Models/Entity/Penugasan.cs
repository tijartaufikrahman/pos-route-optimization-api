using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Penugasan")]
public class Penugasan
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("kode_penugasan")]
    public string KodePenugasan { get; set; } = string.Empty;

    [Column("kantor_id")]
    public int KantorId { get; set; }

    [Column("pengguna_id")]
    public int PenggunaId { get; set; }

    [Column("tanggal_pengantaran")]
    public DateTime TanggalPengantaran { get; set; }

    [Column("status")]
    public string Status { get; set; } = string.Empty;

    [Column("sudah_dioptimasi")]
    public bool SudahDioptimasi { get; set; }

    [Column("waktu_optimasi")]
    public DateTime? WaktuOptimasi { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [Column("created_by")]
    public int? CreatedBy { get; set; }

    [Column("updated_by")]
    public int? UpdatedBy { get; set; }

    /* Navigation Property */

    //[ForeignKey(nameof(PenggunaId))]
    //public virtual Pengguna Pengguna { get; set; }

    //[ForeignKey(nameof(KantorId))]
    //public virtual Kantor Kantor { get; set; }

    //public virtual ICollection<DetailPenugasan> DetailPenugasan { get; set; }
    //    = new List<DetailPenugasan>();


    // Lama
    //public User User { get; set; }

    //public ICollection<UrutanPengantaran> UrutanPengantaran { get; set; }

    public Pengguna Pengguna { get; set; }
    public ICollection<DetailPenugasan> DetailPenugasan { get; set; }
}