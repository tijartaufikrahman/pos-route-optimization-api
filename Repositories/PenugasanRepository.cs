using Microsoft.EntityFrameworkCore;
using SistemOptimasiWebAPI.Interfaces;

public class PenugasanRepository : IPenugasan
{
    private readonly AppDbContext _context;
    private readonly IProofOfDelivery _buktiPengantaran;

    private readonly TimeZoneInfo _timeZone;

    // GLOBAL NOW WIB
    private DateTime WIB => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _timeZone);

    public PenugasanRepository(AppDbContext context, IProofOfDelivery buktiPengantaran)
    {
        _context = context;
        _buktiPengantaran = buktiPengantaran;
        _timeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
    }

    // GET: Daftar penugasan per rentang tanggal (opsional filter user & status)
    public async Task<List<PenugasanListItemDto>> GetPenugasan(DateOnly tglAwal, DateOnly tglAkhir, int? userId, string? status)
    {
        DateTime startDateTime = tglAwal.ToDateTime(TimeOnly.MinValue);
        DateTime endDateTime = tglAkhir.ToDateTime(TimeOnly.MaxValue);

        var query = from j in _context.Penugasan
                    join u in _context.Pengguna
                        on j.PenggunaId equals u.Id
                    join up in _context.DetailPenugasan
                        on j.Id equals up.PenugasanId into upGroup
                    where j.TanggalPengantaran >= startDateTime
                       && j.TanggalPengantaran <= endDateTime
                       // ✅ JIKA USER_ID ADA
                       && (!userId.HasValue || j.PenggunaId == userId.Value)
                       && (string.IsNullOrEmpty(status)
                           || j.Status == status
                           || (status == "Sedang Berjalan" && j.Status == "menunggu persetujuan"))
                    orderby j.TanggalPengantaran descending
                    select new PenugasanListItemDto
                    {
                        id = j.Id,
                        tanggal = j.TanggalPengantaran,
                        namaKurir = u.Nama,
                        status = j.Status == "menunggu persetujuan" ? "Sedang Berjalan" : j.Status,
                        kodePenugasan = j.KodePenugasan,
                        jumlahPaket = upGroup.Count(),
                        total_berat_kiriman = Math.Round(upGroup.Sum(x => (double?)x.Kiriman.BeratKiriman) ?? 0, 3),
                        isOptimized = j.SudahDioptimasi,
                    };

        return await query.ToListAsync();
    }

    // GET: Info dashboard kurir
    public async Task<DashboardInfoDto> GetDashboardInfo(int userId)
    {
        // WIB
        var today = TimeZoneInfo
            .ConvertTimeFromUtc(DateTime.UtcNow, _timeZone)
            .Date;

        // Ambil semua jadwal hari ini
        var jadwalIds = await _context.Penugasan
            .Where(j => j.PenggunaId == userId && j.TanggalPengantaran == today)
            .Select(j => j.Id)
            .ToListAsync();

        if (!jadwalIds.Any())
        {
            return new DashboardInfoDto
            {
                total = 0,
                proses = 0,
                delivered = 0, // Ditambahkan biar aman di UI
                returned = 0,
                selesai = 0,
                total_penugasan = 0
            };
        }

        // Ambil semua kiriman
        var data = await _context.DetailPenugasan
            .Where(u => jadwalIds.Contains(u.PenugasanId))
            .Select(u => new
            {
                status = u.StatusPengantaran
            })
            .ToListAsync();

        // Hitung berdasarkan status riil di DB
        var total = data.Count;

        var proses = data.Count(x => x.status == "on_delivery");
        var delivered = data.Count(x => x.status == "delivered");
        var returned = data.Count(x => x.status == "returned");

        // LOGIKA BARU: Selesai adalah gabungan delivered dan returned
        var selesai = delivered + returned;

        var totalPenugasan = jadwalIds.Count;

        return new DashboardInfoDto
        {
            total = total,
            proses = proses,
            delivered = delivered,  // Nilai murni paket yang sukses dikirim
            returned = returned,   // Nilai murni paket yang retur
            selesai = selesai,    // Total akumulasi yang sudah beres (sukses + gagal)
            total_penugasan = totalPenugasan
        };
    }

    // DELETE: Hapus penugasan + kembalikan status paket
    public async Task<(bool Success, string? Message)> HapusPenugasan(int jadwalId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Cari jadwal
            var jadwal = await _context.Penugasan
                .FirstOrDefaultAsync(j => j.Id == jadwalId);

            if (jadwal == null)
            {
                return (false, "Jadwal pengantaran tidak ditemukan.");
            }

            // Ambil semua urutan pengantaran
            var urutanPengantaranList = await _context.DetailPenugasan
                .Include(u => u.Kiriman)
                .Where(u => u.PenugasanId == jadwalId)
                .ToListAsync();

            /// ==========================================
            /// 🔥 KEMBALIKAN STATUS PAKET
            /// ==========================================
            foreach (var item in urutanPengantaranList)
            {
                if (item.Kiriman != null)
                {
                    item.Kiriman.Status = "manifested";
                }
            }

            /// ==========================================
            /// 🔥 HAPUS URUTAN
            /// ==========================================
            if (urutanPengantaranList.Any())
            {
                _context.DetailPenugasan.RemoveRange(urutanPengantaranList);
            }

            /// HAPUS JADWAL
            _context.Penugasan.Remove(jadwal);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return (true, "Penugasan Berhasil dihapus");
        }
        catch
        {
            await transaction.RollbackAsync();

            throw;
        }
    }

    // POST: Tugaskan kiriman ke pengantaran
    public async Task<(bool Success, bool NotFound, string? Message, AssignToDeliveryResponseDto? Data)> TugaskanKiriman(AssignToDeliveryRequest request)
    {
        var userId = request.UserId;

        // ✅ WIB
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);
        var today = now.Date;

        // 1. Validasi user (Gunakan AsNoTracking karena hanya dibaca)
        var user = await _context.Pengguna.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null)
        {
            return (false, false, "User / Kurir tidak ditemukan", null);
        }

        // 2. Ambil data kantor (Gunakan AsNoTracking)
        var kantor = await _context.Kantor.AsNoTracking().FirstOrDefaultAsync();
        if (kantor == null)
        {
            return (false, false, "Data kantor tidak ditemukan", null);
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

        var paketList = await _context.Kiriman
            .Where(p => p.Status != null && p.Status.ToLower() == "manifested")
            .ToListAsync();
        if (!paketList.Any())
        {
            return (false, false, "Tidak ada kiriman berstatus manifested yang dapat ditugaskan.", null);
        }

        // 3. Ambil kode penugasan terakhir hari ini
        var lastKode = await _context.Penugasan
            .Where(x => x.PenggunaId == userId && x.TanggalPengantaran == today && x.KodePenugasan != null)
            .OrderByDescending(x => x.Id)
            .Select(x => x.KodePenugasan)
            .FirstOrDefaultAsync();

        int nomorPenugasan = 1;
        if (!string.IsNullOrEmpty(lastKode))
        {
            var split = lastKode.Split('-');
            var nomorTerakhir = split.Last();
            nomorPenugasan = int.Parse(nomorTerakhir) + 1;
        }

        // 4. Generate kode penugasan
        //var kodePenugasan = $"KCP{kantor.KodePos}-{now:yyyyMMdd}-{userId}-{nomorPenugasan:D2}";
        var kodePenugasan = $"KCP{kantor.KodePos}{now:yyyyMMdd}{nomorPenugasan:D2}";

        // 5. Buat jadwal baru
        var jadwal = new Penugasan
        {
            KantorId = kantor.Id,
            PenggunaId = userId,
            TanggalPengantaran = today,
            Status = "Sedang Berjalan",
            SudahDioptimasi = false,
            KodePenugasan = kodePenugasan,
            CreatedBy = request.CreatedBy,
            CreatedAt = WIB
        };

        _context.Penugasan.Add(jadwal);

        foreach (var paket in paketList)
        {
            _context.DetailPenugasan.Add(new DetailPenugasan
            {
                Penugasan = jadwal,
                KirimanId = paket.Id,
                NomorUrutan = null,
                StatusPengantaran = "on_delivery",
                CreatedAt = WIB,
                CreatedBy = userId
            });

            paket.Status = "on_delivery";
            paket.UpdatedAt = WIB;
            paket.UpdatedBy = userId;
        }

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return (true, false, "Seluruh kiriman manifested berhasil dimasukkan ke penugasan.", new AssignToDeliveryResponseDto
        {
            message = "Seluruh kiriman manifested berhasil dimasukkan ke penugasan.",
            jadwal_id = jadwal.Id,
            kode_penugasan = jadwal.KodePenugasan,
            total_request = paketList.Count,
            inserted = paketList.Select(p => p.Id).ToList(),
            skipped = new List<int>()
        });
    }

    // GET: Detail kiriman per penugasan
    public async Task<ByPenugasanResponseDto?> GetKirimanByPenugasan(int penugasanId)
    {
        // 1. Ambil penugasan
        var jadwal = await _context.Penugasan
            .FirstOrDefaultAsync(j => j.Id == penugasanId);

        if (jadwal == null)
        {
            return null;
        }

        var user = await _context.Pengguna
            .FirstOrDefaultAsync(u => u.Id == jadwal.PenggunaId);

        // 2. Ambil detail paket
        var data = await (
            from u in _context.DetailPenugasan
            where u.PenugasanId == jadwal.Id

            orderby u.NomorUrutan == null, u.NomorUrutan

            select new DetailPenugasanDto
            {
                urutan_id = u.Id,
                nomor_urutan = u.NomorUrutan,
                status = u.StatusPengantaran,

                paket = new DetailPenugasanPaketDto
                {
                    id = u.Kiriman.Id,
                    no_kiriman = u.Kiriman.NomorKiriman,
                    recipient_name = u.Kiriman.NamaPenerima,
                    telepon = u.Kiriman.TeleponPenerima,
                    address = u.Kiriman.AlamatPenerima,

                    // Info kiriman
                    jenis_kiriman = u.Kiriman.JenisKiriman,
                    jenis_layanan = u.Kiriman.JenisLayanan,
                    berat_kiriman = u.Kiriman.BeratKiriman,

                    // Status asli paket
                    paket_status = u.Kiriman.Status,

                    // Koordinat tujuan
                    latitude = u.Kiriman.Latitude,
                    longitude = u.Kiriman.Longitude,

                    // Status optimasi
                    isOptimized = jadwal.SudahDioptimasi,

                    // POD
                    // Jika belum ada POD maka hasilnya null
                    pod = _context.BuktiPengantaran
                        .Where(p => p.KirimanId == u.Kiriman.Id)
                        .OrderByDescending(p => p.CreatedAt)
                        .Select(p => new DetailPenugasanPodDto
                        {
                            id = p.Id,
                            waktu_pengantaran = p.WaktuPengantaran,
                            koordinat = p.Koordinat,
                            latitude = p.Latitude,
                            longitude = p.Longitude,
                            foto_bukti = p.FotoBukti
                        })
                        .FirstOrDefault()
                }
            }
        ).ToListAsync();

        // 3. Return response
        return new ByPenugasanResponseDto
        {
            jadwal_id = jadwal.Id,
            penugasan_id = jadwal.Id,
            kode_penugasan = jadwal.KodePenugasan,

            jadwal_status = jadwal.Status == "menunggu persetujuan" ? "Sedang Berjalan" : jadwal.Status,
            isOptimized = jadwal.SudahDioptimasi,

            user_id = jadwal.PenggunaId,
            user_nama = user?.Nama,

            tanggal = jadwal.TanggalPengantaran,
            total = data.Count,

            data = data
        };
    }

    // PUT: Selesaikan penugasan
    public async Task<(bool Success, bool NotFound, string? Message, int JadwalId, string? Status)> UpdateStatusPenugasan(int jadwalId)
    {
        // WIB
        var now = WIB;

        // 1. Ambil jadwal
        var jadwal = await _context.Penugasan.FirstOrDefaultAsync(x => x.Id == jadwalId);

        if (jadwal == null)
        {
            return (false, true, "Jadwal tidak ditemukan", 0, null);
        }

        // validasi jika sudah selesai
        if (jadwal.Status == "selesai")
        {
            return (false, false, "Penugasan sudah selesai", 0, null);
        }

        // 2. Ambil semua urutan pengantaran
        var urutanList = await _context.DetailPenugasan
            .Include(x => x.Kiriman)
            .Where(x => x.PenugasanId == jadwal.Id)
            .ToListAsync();

        // 3. Cari paket yang masih on_delivery
        foreach (var item in urutanList)
        {
            // history jangan diubah
            // hanya paket realtime

            if (item.StatusPengantaran == "on_delivery")
            {
                item.Kiriman.Status = "manifested";

                item.Kiriman.UpdatedAt = now;
                item.Kiriman.UpdatedBy = jadwal.PenggunaId;
            }
        }

        // 4. Update status jadwal
        jadwal.Status = "selesai";

        // optional audit
        // jadwal.UpdatedAt = now;
        // jadwal.UpdatedBy = jadwal.UserId;

        await _context.SaveChangesAsync();

        return (true, false, "Penugasan berhasil diselesaikan", jadwal.Id, jadwal.Status);
    }

    // POST: Update status pengiriman + simpan bukti
    public async Task<(bool Success, bool NotFound, string? Message, UpdateStatusPengirimanResponseDto? Data)> UpdateStatusKiriman(UpdateStatusPengirimanDto request)
    {
        var now = WIB;

        var allowedStatus = new[]
        {
            "delivered",
            "returned"
        };

        if (!allowedStatus.Contains(request.Status))
        {
            return (false, false, "Status tidak valid", null);
        }

        var urutan = await _context.DetailPenugasan
            .Include(x => x.Kiriman)
            .FirstOrDefaultAsync(x => x.Id == request.UrutanId);

        if (urutan == null)
        {
            return (false, true, "Data pengantaran tidak ditemukan", null);
        }

        // ==========================
        // UPDATE STATUS PENGANTARAN
        // ==========================

        urutan.StatusPengantaran = request.Status;
        urutan.Kiriman.Status = request.Status;
        urutan.Kiriman.UpdatedAt = now;

        // ==========================
        // UPLOAD FOTO POD
        // ==========================

        string? namaFoto = null;

        if (request.PodPhoto != null)
        {
            namaFoto = await _buktiPengantaran.UploadFileAsync(request.PodPhoto, $"POD_{urutan.KirimanId}");
        }

        // ==========================
        // SIMPAN BUKTI PENGANTARAN
        // ==========================

        var buktiPengantaran = new BuktiPengantaran
        {
            KirimanId = urutan.KirimanId,
            WaktuPengantaran = request.PodDateTime ?? now,
            NamaPenerima = request.NamaPenerima,
            Latitude = request.PodLatitude,
            Longitude = request.PodLongitude,
            Koordinat = request.PodLatitude != null && request.PodLongitude != null ? $"{request.PodLatitude},{request.PodLongitude}" : null,
            FotoBukti = namaFoto,
            NomorKiriman = urutan.Kiriman.NomorKiriman,
            Telepon = urutan.Kiriman.TeleponPenerima,
            AlamatPenerima = urutan.Kiriman.AlamatPenerima,
            CreatedAt = now,
            UpdatedAt = now
        };

        _context.BuktiPengantaran.Add(buktiPengantaran);

        await _context.SaveChangesAsync();

        return (true, false, "Status pengiriman berhasil diperbarui", new UpdateStatusPengirimanResponseDto
        {
            message = "Status pengiriman berhasil diperbarui",
            urutan_id = urutan.Id,
            kiriman_id = urutan.KirimanId,
            status_pengantaran = urutan.StatusPengantaran,
            paket_status = urutan.Kiriman.Status,
            pod = new PodInfoDto
            {
                foto = namaFoto,
                latitude = request.PodLatitude,
                longitude = request.PodLongitude,
                datetime = request.PodDateTime ?? now
            }
        });
    }
}
