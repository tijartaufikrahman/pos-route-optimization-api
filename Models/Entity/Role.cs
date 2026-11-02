using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Role")]
public class Role
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("nama_role")]
    [StringLength(50)]
    public string NamaRole { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [Column("created_by")]
    public int? CreatedBy { get; set; }

    [Column("updated_by")]
    public int? UpdatedBy { get; set; }

    /* Navigation Property */

    //public virtual ICollection<Pengguna> Pengguna { get; set; }
    //    = new List<Pengguna>();
}