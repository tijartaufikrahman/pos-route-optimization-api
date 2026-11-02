using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Role> Role { get; set; }
    public DbSet<Pengguna> Pengguna { get; set; }
    public DbSet<Kantor> Kantor { get; set; }
    public DbSet<Kiriman> Kiriman { get; set; }
    public DbSet<Penugasan> Penugasan { get; set; }
    public DbSet<DetailPenugasan> DetailPenugasan { get; set; }
    public DbSet<BuktiPengantaran> BuktiPengantaran { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        // Urutan -> Jadwal
        modelBuilder.Entity<DetailPenugasan>()
            .HasOne(u => u.Penugasan)
            .WithMany(j => j.DetailPenugasan)
            .HasForeignKey(u => u.PenugasanId);

        // Urutan -> Paket (1-1 karena paket_id unique)
        

        // POD -> Paket (1-1)
        modelBuilder.Entity<BuktiPengantaran>()
            .HasOne(p => p.Kiriman)
            .WithOne(p => p.POD)
            .HasForeignKey<BuktiPengantaran>(p => p.KirimanId);

        // Unique constraint (jadwal_id + nomor_urutan)
        modelBuilder.Entity<DetailPenugasan>()
            .HasIndex(u => new { u.PenugasanId, u.NomorUrutan })
            .IsUnique();


    }
}