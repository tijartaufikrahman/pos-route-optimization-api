using System.Globalization;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using SistemOptimasiWebAPI.Interfaces;

namespace SistemOptimasiWebAPI.Repositories
{
    public class ProofOfDeliveryRepository : IProofOfDelivery
    {
        private readonly AppDbContext _context;

        private readonly string _podFolder = @"C:\Storage\POD";

        public ProofOfDeliveryRepository(AppDbContext context)
        {
            _context = context;

            if (!Directory.Exists(_podFolder))
            {
                Directory.CreateDirectory(_podFolder);
            }
        }

        // GET: Ambil semua data POD
        public async Task<List<BuktiPengantaran>> GetAllPod()
        {
            return await _context.Set<BuktiPengantaran>()
                .AsNoTracking()
                .OrderByDescending(x => x.WaktuPengantaran)
                .ToListAsync();
        }

        public async Task<List<UrutanPodDto>> GetUrutanPod(DateOnly tanggal)
        {
            var awalHari = tanggal.ToDateTime(TimeOnly.MinValue);
            var akhirHari = tanggal.ToDateTime(TimeOnly.MaxValue);

            return await _context.BuktiPengantaran
                .AsNoTracking()
                .Where(p => p.WaktuPengantaran >= awalHari
                    && p.WaktuPengantaran <= akhirHari
                    && p.Latitude.HasValue
                    && p.Longitude.HasValue)
                .OrderBy(p => p.WaktuPengantaran)
                .Select(p => new UrutanPodDto
                {
                    id = p.Id,
                    no_kiriman = p.NomorKiriman,
                    recipient_name = p.NamaPenerima,
                    telepon_penerima = p.Telepon,
                    address = p.AlamatPenerima,
                    waktu_pod = p.WaktuPengantaran!.Value,
                    latitude = p.Latitude!.Value,
                    longitude = p.Longitude!.Value
                })
                .ToListAsync();
        }

        // GET: Cek data POD berdasarkan id kiriman
        public async Task<BuktiPengantaran?> GetPodByKirimanIdAsync(int kirimanId)
        {
            return await _context.Set<BuktiPengantaran>()
                .AsNoTracking()
                .Include(x => x.Kiriman)
                .FirstOrDefaultAsync(x => x.KirimanId == kirimanId);
        }

        // POST: Simpan data POD (list entity)
        public async Task<List<BuktiPengantaran>> AddRangePod(List<BuktiPengantaran> data)
        {
            var podList = data.Select(x => new BuktiPengantaran
            {
                KirimanId = x.KirimanId,
                NomorKiriman = x.NomorKiriman,
                NamaPenerima = x.NamaPenerima,
                Telepon = x.Telepon,
                AlamatPenerima = x.AlamatPenerima,
                WaktuPengantaran = x.WaktuPengantaran,
                Koordinat = x.Koordinat,
                Latitude = x.Latitude,
                Longitude = x.Longitude,
                FotoBukti = x.FotoBukti
            }).ToList();

            await _context.BuktiPengantaran.AddRangeAsync(podList);
            await _context.SaveChangesAsync();

            return podList;
        }

        // POST: Parse file Excel POD menjadi list data
        public async Task<List<PodExcelRow>> ParseExcelPod(IFormFile file)
        {
            var kirimanList = new List<PodExcelRow>();

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);

                using (var package = new ExcelPackage(stream))
                {
                    var sheet = package.Workbook.Worksheets[0];
                    var headers = new Dictionary<string, int>();

                    for (int col = 1; col <= sheet.Dimension.Columns; col++)
                    {
                        var header = sheet.Cells[1, col].Value?.ToString()?.Trim();

                        if (!string.IsNullOrEmpty(header) && !headers.ContainsKey(header))
                            headers.Add(header, col);
                    }

                    for (int row = 2; row <= sheet.Dimension.Rows; row++)
                    {
                        try
                        {
                            var nomorKiriman = GetValue(sheet, row, headers, "Nomor Kiriman");

                            if (string.IsNullOrWhiteSpace(nomorKiriman))
                                continue;

                            var koordinatStr = GetValue(sheet, row, headers, "Koordinat")
                                ?.Replace("\"", "")
                                .Replace("'", "")
                                .Trim();

                            double? lat = null;
                            double? lng = null;

                            // Jika kosong atau "-"
                            if (string.IsNullOrWhiteSpace(koordinatStr) ||
                                koordinatStr == "-" ||
                                koordinatStr.Equals("NULL", StringComparison.OrdinalIgnoreCase))
                            {
                                koordinatStr = "-";
                            }
                            else if (koordinatStr.Contains(","))
                            {
                                var parts = koordinatStr.Split(',');

                                if (parts.Length >= 2)
                                {
                                    if (double.TryParse(parts[0].Trim(),
                                        NumberStyles.Any,
                                        CultureInfo.InvariantCulture,
                                        out var parsedLat))
                                    {
                                        lat = parsedLat;
                                    }

                                    if (double.TryParse(parts[1].Trim(),
                                        NumberStyles.Any,
                                        CultureInfo.InvariantCulture,
                                        out var parsedLng))
                                    {
                                        lng = parsedLng;
                                    }
                                }
                            }

                            var kiriman = new PodExcelRow
                            {
                                no = GetValue(sheet, row, headers, "no"),
                                nomor_kiriman = nomorKiriman,
                                nama_penerima = GetValue(sheet, row, headers, "Nama Penerima"),
                                telepon_penerima = GetValue(sheet, row, headers, "Telp Penerima"),
                                alamat_penerima = GetValue(sheet, row, headers, "Alamat Penerima"),
                                delivery = GetValue(sheet, row, headers, "Delivery"),

                                koordinat = koordinatStr,

                                latitude = lat,
                                longitude = lng,

                                latitude_str = lat?.ToString(CultureInfo.InvariantCulture),
                                longitude_str = lng?.ToString(CultureInfo.InvariantCulture)
                            };

                            kirimanList.Add(kiriman);
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

        private string? GetValue(ExcelWorksheet sheet, int row, Dictionary<string, int> headers, string columnName)
        {
            if (!headers.ContainsKey(columnName))
                return null;

            return sheet.Cells[row, headers[columnName]].Value?.ToString()?.Trim();
        }

        // POST: Bulk insert POD dari request
        public async Task<List<BuktiPengantaran>> BulkInsertPod(List<PodBulkInsertRequest> request)
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);

            var podList = new List<BuktiPengantaran>();

            foreach (var item in request)
            {
                if (string.IsNullOrWhiteSpace(item.nomor_kiriman))
                    continue;

                DateTime? waktuDelivery = null;
                if (!string.IsNullOrWhiteSpace(item.delivery) && DateTime.TryParse(item.delivery, out var parsedDate))
                {
                    waktuDelivery = parsedDate;
                }

                var podBaru = new BuktiPengantaran
                {
                    NomorKiriman = item.nomor_kiriman,
                    NamaPenerima = item.nama_penerima,
                    AlamatPenerima = item.alamat_penerima, // Sesuaikan ke nama kolom di model
                    Telepon = item.telepon_penerima,
                    Koordinat = item.koordinat,
                    Latitude = item.latitude,
                    Longitude = item.longitude,
                    WaktuPengantaran = waktuDelivery, // Menggunakan tanggal yang sudah di-parse

                    CreatedAt = now,
                    CreatedBy = null // Sesuaikan jika ada id user login
                };

                podList.Add(podBaru);
            }

            if (!podList.Any())
            {
                return podList;
            }

            await _context.Set<BuktiPengantaran>().AddRangeAsync(podList);
            await _context.SaveChangesAsync();

            return podList;
        }

        // DELETE: Hapus semua data POD (return jumlah yang dihapus)
        public async Task<int> DeleteAllPod()
        {
            var data = await _context.BuktiPengantaran.ToListAsync();

            if (!data.Any())
            {
                return 0;
            }

            _context.BuktiPengantaran.RemoveRange(data);
            await _context.SaveChangesAsync();

            return data.Count;
        }

        // FILE: Upload file bukti
        public async Task<string> UploadFileAsync(IFormFile file, string? nama)
        {
            string extension = Path.GetExtension(file.FileName);

            string fileName;

            if (string.IsNullOrWhiteSpace(nama))
            {
                fileName = $"POD_{DateTime.Now:yyyyMMddHHmmssfff}{extension}";
            }
            else
            {
                nama = string.Join("_", nama.Split(Path.GetInvalidFileNameChars()));

                fileName = $"{nama}{extension}";
            }

            string filePath = Path.Combine(_podFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return fileName;
        }

        // FILE: Ambil file bukti
        public async Task<(byte[] Bytes, string ContentType)?> GetFileAsync(string fileName)
        {
            string filePath = Path.Combine(_podFolder, fileName);

            if (!File.Exists(filePath))
            {
                return null;
            }

            var provider = new FileExtensionContentTypeProvider();

            if (!provider.TryGetContentType(filePath, out string? contentType))
            {
                contentType = "application/octet-stream";
            }
            var bytes = await File.ReadAllBytesAsync(filePath);

            return (bytes, contentType);
        }

        // FILE: Hapus file bukti
        public Task DeleteFileAsync(string? fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return Task.CompletedTask;
            }
            string filePath = Path.Combine(_podFolder, fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            return Task.CompletedTask;
        }
    }
}
