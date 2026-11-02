using System.Globalization;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using SistemOptimasiWebAPI.Interfaces;

public class KirimanRepository : IKiriman
{
    private readonly AppDbContext _context;

    public KirimanRepository(AppDbContext context)
    {
        _context = context;
    }

    // GET: Daftar kiriman status Manifested (page tambah penugasan)
    public async Task<List<KirimanListItemDto>> GetKiriman()
    {
        return await (from p in _context.Kiriman
                      where p.Status.ToLower() == "manifested"
                      orderby p.CreatedAt descending
                      select new KirimanListItemDto
                      {
                          id = p.Id,
                          no_kiriman = p.NomorKiriman,
                          recipient_name = p.NamaPenerima,
                          telepon_penerima = p.TeleponPenerima,
                          address = p.AlamatPenerima,
                          kriteria = p.JenisKiriman,
                          kriteriaId = 0,
                          service = p.JenisLayanan,
                          serviceLayanan = 0,
                          berat_kiriman = p.BeratKiriman,
                          latitude = p.Latitude,
                          longitude = p.Longitude,
                          koordinat = p.Latitude + "," + p.Longitude,
                          status = p.Status
                      }).ToListAsync();
    }

    // GET: Daftar kiriman berdasarkan rentang tanggal dan status (page datapaket)
    public async Task<List<KirimanListDto>> GetListKiriman(DateOnly? tanggalAwal, DateOnly? tanggalAkhir, string? status)
    {
        var normalizedStatus = string.IsNullOrWhiteSpace(status) ? null : status.Trim().ToLower();
        var query = _context.Kiriman.AsQueryable();

        if (tanggalAwal.HasValue)
        {
            var start = tanggalAwal.Value.ToDateTime(TimeOnly.MinValue);
            query = query.Where(p => p.CreatedAt >= start);
        }

        if (tanggalAkhir.HasValue)
        {
            var end = tanggalAkhir.Value.ToDateTime(TimeOnly.MaxValue);
            query = query.Where(p => p.CreatedAt <= end);
        }

        if (normalizedStatus != null)
        {
            query = query.Where(p => p.Status != null && p.Status.ToLower() == normalizedStatus);
        }

        return await (from p in query
                      orderby p.CreatedAt descending
                      select new KirimanListDto
                      {
                          id = p.Id,
                          no_kiriman = p.NomorKiriman,
                          recipient_name = p.NamaPenerima,
                          telepon_penerima = p.TeleponPenerima,
                          address = p.AlamatPenerima,
                          kriteria = p.JenisKiriman,
                          kriteriaId = 0,
                          service = p.JenisLayanan,
                          serviceLayanan = 0,
                          berat_kiriman = p.BeratKiriman,
                          created_at = p.CreatedAt,
                          latitude = p.Latitude,
                          longitude = p.Longitude,
                          koordinat = p.Latitude + "," + p.Longitude,
                          isAction = _context.DetailPenugasan.Any(u => u.KirimanId == p.Id) ? 0 : 1,
                          status = p.Status
                      }).ToListAsync();
    }

    
    public async Task<ByKurirResponseDto?> GetPaketByKurir(int userId)
    {
        // WIB
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        var today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone).Date;

        // Ambil jadwal hari ini
        var jadwal = await _context.Penugasan.FirstOrDefaultAsync(j => j.PenggunaId == userId && j.TanggalPengantaran == today);

        if (jadwal == null)
        {
            return null;
        }

        // Ambil data paket + urutan + Join POD dengan 3 kriteria
        var data = await (from u in _context.DetailPenugasan
                          where u.PenugasanId == jadwal.Id
                          let pod = _context.BuktiPengantaran.FirstOrDefault(p =>
                              p.NomorKiriman == u.Kiriman.NomorKiriman &&
                              p.Telepon == u.Kiriman.TeleponPenerima &&
                              EF.Functions.Like(u.Kiriman.AlamatPenerima, p.AlamatPenerima))

                          orderby u.NomorUrutan == null, u.NomorUrutan
                          select new DetailPenugasanKurirDto
                          {
                              urutan_id = u.Id,
                              nomor_urutan = u.NomorUrutan,
                              status = u.StatusPengantaran,

                              paket = new PaketKurirDto
                              {
                                  id = u.Kiriman.Id,
                                  no_kiriman = u.Kiriman.NomorKiriman,
                                  recipient_name = u.Kiriman.NamaPenerima,
                                  telepon = u.Kiriman.TeleponPenerima,
                                  address = u.Kiriman.AlamatPenerima,
                                  latitude = u.Kiriman.Latitude != null ? u.Kiriman.Latitude : (pod != null ? pod.Latitude : null),
                                  longitude = u.Kiriman.Longitude != null ? u.Kiriman.Longitude : (pod != null ? pod.Longitude : null),

                                  is_delivered = pod != null
                              }
                          })
                          .ToListAsync();

        return new ByKurirResponseDto
        {
            jadwal_id = jadwal.Id,
            user_id = userId,
            tanggal = today,
            total = data.Count,
            data = data
        };
    }

    // POST: Preview data dari file Excel
    public async Task<List<PreviewExcelRow>> PreviewDataExcel(IFormFile file)
    {
        var kirimanList = new List<PreviewExcelRow>();

        using (var stream = new MemoryStream())
        {
            await file.CopyToAsync(stream);

            using (var package = new ExcelPackage(stream))
            {
                var sheet = package.Workbook.Worksheets[0];

                // HEADER
                var headers = new Dictionary<string, int>();

                for (int col = 1; col <= sheet.Dimension.Columns; col++)
                {
                    var header = sheet.Cells[1, col].Text.Trim();

                    if (!headers.ContainsKey(header))
                        headers.Add(header, col);
                }

                // LOOP DATA
                for (int row = 2; row <= sheet.Dimension.Rows; row++)
                {
                    try
                    {
                        // Ambil langsung nilai string dari Excel
                        var jenisLayanan = GetValue(sheet, row, headers, "Jenis Layanan")?.Trim();
                        var jenisKiriman = GetValue(sheet, row, headers, "Kriteria Kiriman")?.Trim();

                        var kiriman = new PaketBulkInsertRequest
                        {
                            nomor_kiriman = GetValue(sheet, row, headers, "Nomor Kiriman"),

                            jenis_layanan = jenisLayanan,
                            kriteria_kiriman = jenisKiriman,

                            berat_kiriman = ParseBeratAktual(GetValue(sheet, row, headers, "Berat Kiriman")),
                            nama_penerima = GetValue(sheet, row, headers, "Nama Penerima"),

                            telepon_penerima = GetValue(sheet, row, headers, "Telp Penerima"),
                            alamat_penerima = GetValue(sheet, row, headers, "Alamat Penerima"),
                            koordinat = GetValue(sheet, row, headers, "Koordinat")
                        };

                        kirimanList.Add(new PreviewExcelRow
                        {
                            request_data = kiriman,
                            nama_layanan_preview = jenisLayanan,
                            nama_kriteria_preview = jenisKiriman
                        });
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
        }

        return kirimanList;
    }

    /// 🔥 AMBIL BERAT AKTUAL SAJA
    private decimal ParseBeratAktual(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return 0;

        try
        {
            var splitAktual = text.Split("Aktual :");

            if (splitAktual.Length < 2)
                return 0;

            var afterAktual = splitAktual[1];

            var splitKg = afterAktual.Split("Kg");

            if (splitKg.Length < 1)
                return 0;

            var numberText = splitKg[0]
                .Trim()
                .Replace(",", ".");

            return decimal.TryParse(
                numberText,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out decimal result
            )
                ? result : 0;
        }
        catch
        {
            return 0;
        }
    }

    // Helper [HttpPost("upload-excel")]
    private string? GetValue(ExcelWorksheet sheet, int row, Dictionary<string, int> headers, string columnName)
    {
        if (!headers.ContainsKey(columnName))
            return null;

        return sheet.Cells[row, headers[columnName]].Text.Trim();
    }

    // POST: Bulk insert kiriman
    public async Task<List<Kiriman>> BulkInsertKiriman(List<PaketBulkInsertRequest> request)
    {
        // WIB
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);

        var paketBaruList = new List<Kiriman>();

        foreach (var x in request.Where(x => !string.IsNullOrWhiteSpace(x.nomor_kiriman)))
        {
            double? lat = null;
            double? lng = null;

            // PRIORITAS 1 : Gunakan koordinat dari file import
            if (!string.IsNullOrWhiteSpace(x.koordinat))
            {
                var split = x.koordinat.Split(',');

                if (split.Length == 2)
                {
                    if (double.TryParse(split[0].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedLat))
                    {
                        lat = parsedLat;
                    }

                    if (double.TryParse(split[1].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedLng))
                    {
                        lng = parsedLng;
                    }
                }
            }
            else
            {
                // PRIORITAS 2 : Cari koordinat dari POD
                var pod = await _context.BuktiPengantaran
                    .Where(p =>
                        p.NamaPenerima == x.nama_penerima &&
                        p.Telepon == x.telepon_penerima &&
                        p.AlamatPenerima == x.alamat_penerima &&
                        p.Latitude != null &&
                        p.Longitude != null)
                    .OrderByDescending(p => p.CreatedAt)
                    .FirstOrDefaultAsync();

                if (pod != null)
                {
                    lat = pod.Latitude;
                    lng = pod.Longitude;
                }
            }

            paketBaruList.Add(new Kiriman
            {
                NomorKiriman = x.nomor_kiriman,
                JenisLayanan = x.jenis_layanan,
                NamaPenerima = x.nama_penerima,
                TeleponPenerima = x.telepon_penerima,
                AlamatPenerima = x.alamat_penerima,
                BeratKiriman = x.berat_kiriman,
                JenisKiriman = x.kriteria_kiriman,

                Latitude = lat,
                Longitude = lng,

                Status = "Manifested",

                CreatedAt = now,
                CreatedBy = null
            });
        }

        if (!paketBaruList.Any())
        {
            return paketBaruList;
        }

        await _context.Kiriman.AddRangeAsync(paketBaruList);
        await _context.SaveChangesAsync();

        return paketBaruList;
    }

    // PUT: Update lokasi kiriman
    public async Task<bool> UpdateLokasiAsync(int idKiriman, double latitude, double longitude)
    {
        var kiriman = await _context.Kiriman.FindAsync(idKiriman);

        if (kiriman == null)
            return false;

        kiriman.Latitude = latitude;
        kiriman.Longitude = longitude;
        kiriman.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    // DELETE: Hapus satu kiriman
    public async Task<bool> DeleteKiriman(int id)
    {
        var paket = await _context.Kiriman.FindAsync(id);

        if (paket == null)
        {
            return false;
        }

        _context.Kiriman.Remove(paket);
        await _context.SaveChangesAsync();

        return true;
    }

    // DELETE: Hapus semua kiriman
    public async Task<(bool Success, string? Message, int Total, string? Error)> DeleteAllKiriman()
    {
        try
        {
            var data = await _context.Kiriman.ToListAsync();

            if (!data.Any())
            {
                return (true, "Tidak ada data kiriman yang dihapus.", 0, null);
            }

            _context.Kiriman.RemoveRange(data);
            await _context.SaveChangesAsync();

            return (true, "Semua data kiriman berhasil dihapus.", data.Count, null);
        }
        catch (DbUpdateException)
        {
            // Batalkan state Delete yang masih tersimpan di DbContext
            _context.ChangeTracker.Clear();

            return (false, "Data kiriman gagal dihapus karena masih digunakan pada penugasan.", 0, null);
        }
        catch (Exception ex)
        {
            _context.ChangeTracker.Clear();

            return (false, "Data kiriman gagal dihapus.", 0, ex.Message);
        }
    }
}
