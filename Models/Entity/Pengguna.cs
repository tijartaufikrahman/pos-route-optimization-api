using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Pengguna")]
public class Pengguna
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    //[Required]
    [Column("nama")]
    [StringLength(100)]
    public string Nama { get; set; } = string.Empty;

    [Column("email")]
    [StringLength(100)]
    public string? Email { get; set; }

    //[Required]
    [Column("role_id")]
    public int RoleId { get; set; }

    //[Required]
    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    //[Required]
    [Column("username")]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;

    //[Required]
    [Column("password")]
    [StringLength(100)]
    public string Password { get; set; } = string.Empty;

    [Column("reset_otp")]
    [StringLength(10)]
    public string? ResetOtp { get; set; }

    [Column("reset_otp_expired_at")]
    public DateTime? ResetOtpExpiredAt { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [Column("created_by")]
    public int? CreatedBy { get; set; }

    [Column("updated_by")]
    public int? UpdatedBy { get; set; }

    /* Navigation Property */

    //[ForeignKey(nameof(RoleId))]
    //public virtual Role Role { get; set; }

    //public virtual ICollection<Penugasan> Penugasan { get; set; }
    //    = new List<Penugasan>();
}