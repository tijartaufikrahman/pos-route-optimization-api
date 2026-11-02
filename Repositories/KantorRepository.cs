using System.Globalization;
using Microsoft.EntityFrameworkCore;
using SistemOptimasiWebAPI.Interfaces;

public class KantorRepository : IKantor
{
    private readonly AppDbContext _context;

    public KantorRepository(AppDbContext context)
    {
        _context = context;
    }

    // GET: Ambil data kantor pertama
    public async Task<Kantor?> GetKantor()
    {
        return await _context.Kantor.FirstOrDefaultAsync();
    }

    // POST: Simpan / perbarui data kantor (upsert)
    public async Task<(bool Success, string? Message, int Id)> UpsertKantor(LokasiKantorDto request)
    {
        try
        {
            // PROTEKSI EXTRA: Bersihkan karakter kutip (') atau (") jika terbawa dari input
            string cleanLat = (request.Latitude ?? "0").Replace("'", "").Replace("\"", "").Trim();
            string cleanLng = (request.Longitude ?? "0").Replace("'", "").Replace("\"", "").Trim();

            // PARSING menggunakan InvariantCulture (Titik = Desimal)
            double lat = double.Parse(cleanLat, CultureInfo.InvariantCulture);
            double lng = double.Parse(cleanLng, CultureInfo.InvariantCulture);

            var kantor = await _context.Kantor.FirstOrDefaultAsync();

            if (kantor == null)
            {
                kantor = new Kantor
                {
                    Nama = request.NamaKantor ?? "",
                    KodePos = request.KodePos ?? "",
                    Alamat = request.Alamat,
                    Latitude = lat,
                    Longitude = lng,
                    CreatedAt = DateTime.Now,
                    CreatedBy = request.UserId
                };
                _context.Kantor.Add(kantor);
            }
            else
            {
                kantor.Nama = request.NamaKantor ?? "";
                kantor.KodePos = request.KodePos ?? "";
                kantor.Alamat = request.Alamat;
                kantor.Latitude = lat;
                kantor.Longitude = lng;
                kantor.UpdatedAt = DateTime.Now;
                kantor.UpdatedBy = request.UserId;
                _context.Kantor.Update(kantor);
            }

            await _context.SaveChangesAsync();

            return (true, null, kantor.Id);
        }
        catch (Exception ex)
        {
            return (false, "Format koordinat salah atau gagal simpan: " + ex.Message, 0);
        }
    }

    // DELETE: Hapus data kantor
    public async Task<bool> DeleteKantor()
    {
        var kantor = await _context.Kantor.FirstOrDefaultAsync();

        if (kantor == null)
        {
            return false;
        }

        _context.Kantor.Remove(kantor);
        await _context.SaveChangesAsync();

        return true;
    }
}
