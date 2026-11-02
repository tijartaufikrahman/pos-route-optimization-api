using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SistemOptimasiWebAPI.Interfaces;
using SistemOptimasiWebAPI.Models.DTO;

public class OptimizationRepository : IOptimization
{
    private readonly HttpClient _http;
    private readonly AppDbContext _context;

    //private string urlGeo = "http://localhost:8002";
    //private string urlGeo = "http://98.90.139.52:8002";
    //private string urlGeo = "http://62.171.169.192:8003";
    private string urlGeo = "http://localhost:8002";

    public OptimizationRepository(HttpClient http, AppDbContext context)
    {
        _http = http;
        _context = context;
    }

    // POST: Hitung matrix jarak + jalankan optimasi (ACO/GA + 2-opt)
    public async Task<OptimizationResultDto> HitungJarakMatrixRunOptimasi(OptimizationRequestDto request)
    {
        var locations = new List<object>();

        locations.Add(new
        {
            lat = request.depotLat,
            lon = request.depotLon
        });

        locations.AddRange(request.pakets.Select(p => new
        {
            lat = p.lat,
            lon = p.lon
        }));

        /* Request Matrix Valhalla */
        var valhallaRequest = new
        {
            sources = locations,
            targets = locations,
            costing = "motorcycle",
            //costing = "auto",
        };

        var json = JsonSerializer.Serialize(valhallaRequest);

        var url = $"{urlGeo}/sources_to_targets?json={json}";

        //var response = await _http.GetStringAsync(url);

        //
        var httpResponse = await _http.GetAsync(url);
        var response = await httpResponse.Content.ReadAsStringAsync();

        if (!httpResponse.IsSuccessStatusCode)
        {
            throw new Exception($"Valhalla Error: {response}");
        }
        //


        var result = JsonSerializer.Deserialize<JsonElement>(response);

        var matrix = JsonSerializer.Deserialize<MatrixCell[][]>(
            result.GetProperty("sources_to_targets").ToString()
        );

        /* Jalankan Algoritma (ACO / GA / NN) */
        var ant = request.ant ?? 65;

        AcoResult aco;

        switch (request.algoritma?.ToLower())
        {
            case "ge":
                aco = GeneticAlgorithm(matrix: matrix, popSize: 100, nIter: 1000, crossoverRate: 0.9, mutationRate: 0.1, tournamentSize: 5, elitism: true);
                break;

            case "nn":
                aco = NearestNeighbor(matrix);
                break;

            case "ant":
            default:
                aco = AntColony(matrix, ant, 99999, 1, 3, 0.5, 100);
                break;
        }

        bool use2Opt = request.is_2opt ?? true;

        /* DEFAULT */
        AcoResult finalResult = aco;
        List<IterationHistory> histories = aco.histories;
        List<IterationHistory> histories2Opt = new List<IterationHistory>();

        /* JALANKAN 2OPT JIKA DIAKTIFKAN */

        if(request.algoritma != "nn")
        {
            if (use2Opt)
            {
                var twoOpt = TwoOptFirstImprovement(matrix, aco);
                finalResult = twoOpt;
                histories2Opt = twoOpt.histories;
            }
        }

        

        /* ROUTE FINAL */
        var route = finalResult.solution;

        /* Hitung distance + segments */
        double totalDistance = 0;
        double totalTime = 0;

        var segments = new List<SegmentDto>();

        for (int i = 0; i < route.Count - 1; i++)
        {
            var from = route[i];
            var to = route[i + 1];

            var dist = matrix[from][to].distance;
            var time = matrix[from][to].time;

            totalDistance += dist;
            totalTime += time;

            segments.Add(new SegmentDto
            {
                from = from == 0 ? "depot" : request.pakets[from - 1].id.ToString(),
                to = to == 0 ? "depot" : request.pakets[to - 1].id.ToString(),
                distance = dist,
                duration = time
            });
        }

        /* Mapping ke paket */
        var orderedPakets = route
            .Skip(1)
            .Take(route.Count - 2)
            .Select((index, i) => new OrderedPaketDto
            {
                order = i + 1,
                paketId = request.pakets[index - 1].id,
                lat = request.pakets[index - 1].lat,
                lon = request.pakets[index - 1].lon
            })
            .ToList();

        /* Return FINAL */
        return new OptimizationResultDto
        {
            orderedPakets = orderedPakets,
            totalDistance = totalDistance,
            totalDuration = totalTime,
            segments = segments,

            histories = histories,
            histories2Opt = histories2Opt
        };
    }

    // POST: Route polyline per pasangan titik
    public async Task<RoutePolylineResponseDto> RoutePolyline(RouteRequestDto request)
    {
        var resultRoutes = new List<RoutePolylineDto>();

        for (int i = 0; i < request.locations.Count - 1; i++)
        {
            var start = request.locations[i];
            var end = request.locations[i + 1];

            var body = new
            {
                locations = new[]
                {
                    start,
                    end
                },
                //costing = "auto"
                costing = "motorcycle"
            };

            var json = JsonSerializer.Serialize(body);

            var response = await _http.PostAsync(
                $"{urlGeo}/route",
                new StringContent(json, Encoding.UTF8, "application/json")
            );

            var result = await response.Content.ReadAsStringAsync();

            var doc = JsonDocument.Parse(result);

            var shape = doc.RootElement
                .GetProperty("trip")
                .GetProperty("legs")[0]
                .GetProperty("shape")
                .GetString();

            resultRoutes.Add(new RoutePolylineDto
            {
                start = start,
                end = end,
                polyline = shape
            });
        }

        return new RoutePolylineResponseDto
        {
            routes = resultRoutes
        };
    }

    // GET: Route statis (dummy)
    public RoutePolylineResponseDto GetStaticRoute()
    {
        var routes = new List<RoutePolylineDto>
        {
            new()
            {
                start = new LocationDto { lat = -6.9105, lon = 107.6065 },
                end = new LocationDto { lat = -6.9120, lon = 107.6150 },
                polyline = "bcxdL{cxflEeJeBwDs@N}@x@iFrDwYZgCv@kG^_JRcHCiDa@sDo@kH_A}KOcBoAiMK_B[kFYyFJeFHaGIkDgKm]iMqh@g@uBoBiHMeCYqGGwAG_E?g@CyKEs\\\\A_Gd@eWK{CKaBQeGVaDjAmGjHcTdEwK|P{a@t@{B|IiEhO_ExAa@jw@wS`XiHsGqW_BmG"
            },
            new()
            {
                start = new LocationDto { lat = -6.9120, lon = 107.6150 },
                end = new LocationDto { lat = -6.9185, lon = 107.6205 },
                polyline = "r}zdL_phglE~AlGrGpWdI~Y~CfLv@rC`GrSvBrHpApA~A`AhBl@pBRrBAtDa@rKuDtCeAdFgBjEwCvFkDv@e@hEwBxLeGfCqAxFwCnJcFdBe@xBa@VCvBKbBEjEFjBNhBLxIt@~j@jF|Gt@lAJbBNvYhCjEb@np@~G`LjAvElAtAt@xEpCbI~IdBfEx@dDnBjMNb@zP{^xPy^dPc`@`g@euAlWuv@hT{k@nNk`@sj@qByHIqHYer@gCdAiYTaHNyCj@kM^cD|DwSzBqOh@oD~AiKxC{PyCm@aB[wIoAwB`@mBfB_J_B"
            },
            new()
            {
                start = new LocationDto { lat = -6.9185, lon = 107.6205 },
                end = new LocationDto { lat = -6.9220, lon = 107.6250 },
                polyline = "vogeL{srglE~I~AlBgBvBa@vInA`BZxCl@vH{e@tE}X\\\\iBzDwKrAeCnFwEn`@ePqDgIkCaGUm@qH_QeKqWtYuNnRgJiB}DrN_GhDmAt@k@l@}@lCuHd@eAtCkE`@]\\\\SnFm@vScBpJw@~LgAd@a@Vs@oCm^G{E`Ci@zBUjCC`CElC?rEZL_CFmAB{Ak@aEAg@"
            },
            new()
            {
                start = new LocationDto { lat = -6.9220, lon = 107.6250 },
                end = new LocationDto { lat = -6.9255, lon = 107.6120 },
                polyline = "btneLwa|glE@f@j@`ECzAGlAM~BsE[mC?aCDkCB{BTaCh@mB@iCdCkD~@yz@tHcn@rFc]zByRnAaBVPnDfEvJrG`OxAhDjBlEvRld@l@xAdKpWpH~PTl@jC`GpDfIhExJhBjEnG~N`EhJhOr]xOr^pFbMbA~BP^jKhVvKdUhKzVzVho@vE`MpAdDvAfErAdEx@rJQpCU`ESnCW`EOrBwBl[g@fHG~@q@xJe@bHKlAeFds@W|DqBdZe@`I]tGaArOc@hH{C|g@QjCq@hLYtEEt@m@tKUpD_AbQUnExDVzObA|QhAbG^b^lB`ZpA~On@`]|Aj\\\\~@~AJlFHvBIrC[zCy@nDaBbFiCjGoDvDmC"
            }
        };

        return new RoutePolylineResponseDto { routes = routes };
    }

    // POST: Hasil optimasi (matrix + polyline)
    public async Task<CalculateResultDto> GetHasilOptimasi(CalculateRequestDto request)
    {
        var depot = (from d in _context.Kantor
                     select new LocationDto
                     {
                         lat = d.Latitude,
                         lon = d.Longitude
                     }).FirstOrDefault();

        var fullLocations = new List<LocationDto>();

        fullLocations.Add(depot);                 // depot awal
        fullLocations.AddRange(request.locations); // kiriman
        fullLocations.Add(depot);                 // depot akhir

        var matrixReq = new
        {
            sources = fullLocations,
            targets = fullLocations,
            //costing = "auto"
            costing = "motorcycle"
        };

        var matrixJson = JsonSerializer.Serialize(matrixReq);
        var matrixUrl = $"{urlGeo}/sources_to_targets?json={matrixJson}";

        var matrixRes = await _http.GetStringAsync(matrixUrl);

        var matrixResult = JsonSerializer.Deserialize<JsonElement>(matrixRes);

        var matrix = JsonSerializer.Deserialize<MatrixCell[][]>(
            matrixResult.GetProperty("sources_to_targets").ToString()
        );

        double totalDistance = 0;
        double totalDuration = 0;

        var segments = new List<CalculateSegmentDto>();

        for (int i = 0; i < fullLocations.Count - 1; i++)
        {
            var dist = matrix[i][i + 1].distance;
            var time = matrix[i][i + 1].time;

            totalDistance += dist;
            totalDuration += time;

            segments.Add(new CalculateSegmentDto
            {
                fromIndex = i,
                toIndex = i + 1,
                distance = dist,
                duration = time
            });
        }

        /// ROUTE (POLYLINE)
        var routeReq = new
        {
            locations = fullLocations,
            //costing = "auto"
            costing = "motorcycle"
        };

        var routeJson = JsonSerializer.Serialize(routeReq);
        var routeUrl = $"{urlGeo}/route?json={routeJson}";

        var routeRes = await _http.GetStringAsync(routeUrl);

        var routeResult = JsonSerializer.Deserialize<JsonElement>(routeRes);

        var legs = routeResult
            .GetProperty("trip")
            .GetProperty("legs");

        var polylines = new List<string>();

        foreach (var leg in legs.EnumerateArray())
        {
            if (leg.TryGetProperty("shape", out var shape))
            {
                polylines.Add(shape.GetString());
            }
        }

        /// RETURN
        return new CalculateResultDto
        {
            totalDistance = totalDistance,
            totalDuration = totalDuration,
            segments = segments,
            polylines = polylines,
            totalPoints = fullLocations.Count
        };
    }

    // POST: Simpan urutan optimasi (dengan transaksi)
    public async Task<(bool Success, string? Message, int Total)> SimpanOptimasi(UpdateUrutanDto request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // VALIDASI
            if (request.jadwalId <= 0)
                return (false, "Jadwal tidak valid", 0);

            if (request.paketIds == null || !request.paketIds.Any())
                return (false, "Paket kosong", 0);

            // AMBIL DATA URUTAN
            var existing = await _context.DetailPenugasan
                .Where(x => x.PenugasanId == request.jadwalId && request.paketIds.Contains(x.KirimanId))
                .ToListAsync();

            // SET TEMPORARY NOMOR
            // BIAR TIDAK TABRAKAN UNIQUE
            int temp = -1;

            foreach (var item in existing)
            {
                item.NomorUrutan = temp;
                temp--;
            }

            await _context.SaveChangesAsync();

            // SET NOMOR FINAL
            int nomor = 1;

            foreach (var paketId in request.paketIds)
            {
                var data = existing.FirstOrDefault(x => x.KirimanId == paketId);

                if (data == null)
                    return (false, $"Paket ID {paketId} tidak ditemukan", 0);

                data.NomorUrutan = nomor;
                nomor++;
            }

            await _context.SaveChangesAsync();

            // UPDATE STATUS OPTIMASI
            var jadwal = await _context.Penugasan.FirstOrDefaultAsync(x => x.Id == request.jadwalId);

            if (jadwal != null)
            {
                // WIB
                var wibZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

                var wibNow = TimeZoneInfo.ConvertTimeFromUtc(
                    DateTime.UtcNow,
                    wibZone
                );

                jadwal.SudahDioptimasi = true;
                jadwal.WaktuOptimasi = wibNow;
            }

            await _context.SaveChangesAsync();

            // COMMIT
            await transaction.CommitAsync();

            return (true, "Urutan berhasil diupdate", request.paketIds.Count);
        }
        catch
        {
            await transaction.RollbackAsync();

            throw;
        }
    }

    // ============ ALGORITMA ============

    // Nearest Neighbor (greedy): selalu pilih titik terdekat yang belum dikunjungi
    AcoResult NearestNeighbor(MatrixCell[][] matrix)
    {
        int n = matrix.Length;
        int start = 0;

        bool[] visited = new bool[n];
        List<int> route = new List<int>();

        int current = start;
        route.Add(current);
        visited[current] = true;

        /* SELALU PILIH TITIK TERDEKAT YANG BELUM DIKUNJUNGI */
        while (route.Count < n)
        {
            int next = -1;
            double bestDistance = double.MaxValue;

            for (int i = 0; i < n; i++)
            {
                if (!visited[i] && matrix[current][i] != null)
                {
                    double distance = matrix[current][i].distance;

                    if (distance > 0 && distance < bestDistance)
                    {
                        bestDistance = distance;
                        next = i;
                    }
                }
            }

            /* TIDAK ADA KANDIDAT */
            if (next == -1)
            {
                break;
            }

            route.Add(next);
            visited[next] = true;
            current = next;
        }

        /* Jika tidak semua titik berhasil dikunjungi */
        if (route.Count != n)
        {
            return new AcoResult
            {
                solution = new List<int>(),
                distance = 0,
                duration = 0,
                histories = new List<IterationHistory>()
            };
        }

        /* KEMBALI KE DEPOT */
        route.Add(start);

        /* HITUNG TOTAL JARAK DAN DURASI */
        double totalDistance = 0;
        double totalDuration = 0;
        bool validRoute = true;

        for (int i = 0; i < route.Count - 1; i++)
        {
            int from = route[i];
            int to = route[i + 1];

            if (matrix[from][to] == null)
            {
                validRoute = false;
                break;
            }

            totalDistance += matrix[from][to].distance;
            totalDuration += matrix[from][to].time;
        }

        if (!validRoute || totalDistance <= 0)
        {
            return new AcoResult
            {
                solution = new List<int>(),
                distance = 0,
                duration = 0,
                histories = new List<IterationHistory>()
            };
        }

        /* SIMPAN HISTORY (1 ITERASI) */
        var histories = new List<IterationHistory>
        {
            new IterationHistory
            {
                Iteration = 1,
                BestDistance = totalDistance,
                BestDuration = totalDuration,
                BestRoute = new List<int>(route)
            }
        };

        return new AcoResult
        {
            solution = route,
            distance = totalDistance,
            duration = totalDuration,
            histories = histories
        };
    }

    AcoResult AntColony(MatrixCell[][] matrix, int nAnts, int nIter, double alpha, double beta, double evaporation, double Q)
    {
        int n = matrix.Length;
        int start = 0;

        int maxStagnation = 100;

        int stagnationCount = 0;

        double previousIterationBestDistance = -1;

        /* PHEROMONE AWAL */
        double[][] pheromone = new double[n][];

        for (int i = 0; i < n; i++)
        {
            pheromone[i] = new double[n];

            for (int j = 0; j < n; j++)
            {
                pheromone[i][j] = 0.01;
            }
        }

        List<int> bestRoute = null;
        double bestDistance = double.MaxValue;
        double bestDuration = double.MaxValue;

        Random rand = new Random();

        /* HISTORY ITERASI */
        var histories = new List<IterationHistory>();

        /* ITERASI ACO */
        for (int iter = 0; iter < nIter; iter++)
        {
            var allRoutes = new List<(List<int> route, double dist, double time)>();

            /* SETIAP SEMUT */
            for (int ant = 0; ant < nAnts; ant++)
            {
                bool[] visited = new bool[n];
                List<int> route = new List<int>();

                int current = start;

                route.Add(current);
                visited[current] = true;

                /* CARI RUTE */
                while (route.Count < n)
                {
                    var candidates = new List<(int node, double value)>();
                    double total = 0;

                    /* HITUNG NILAI PROBABILITAS */
                    for (int i = 0; i < n; i++)
                    {
                        if (!visited[i] && matrix[current][i] != null)
                        {
                            double distance = matrix[current][i].distance;

                            if (distance <= 0)
                            {
                                continue;
                            }

                            double tau = Math.Pow(pheromone[current][i], alpha);
                            double eta = Math.Pow(1.0 / distance, beta);
                            double value = tau * eta;

                            candidates.Add((i, value));
                            total += value;
                        }
                    }

                    /* TIDAK ADA KANDIDAT */
                    if (candidates.Count == 0 || total <= 0)
                    {
                        break;
                    }

                    /* RANDOM ROULETTE WHEEL */
                    double randomValue = rand.NextDouble();
                    double cumulativeProbability = 0;
                    int next = candidates[0].node;

                    foreach (var candidate in candidates)
                    {
                        double probability = candidate.value / total;
                        cumulativeProbability += probability;

                        if (randomValue <= cumulativeProbability)
                        {
                            next = candidate.node;
                            break;
                        }
                    }

                    /* PINDAH KE NODE BERIKUTNYA */
                    route.Add(next);
                    visited[next] = true;
                    current = next;
                }

                /* Jika tidak semua titik berhasil dikunjungi */
                if (route.Count != n)
                {
                    continue;
                }

                /* KEMBALI KE DEPOT */
                route.Add(start);

                /* HITUNG TOTAL JARAK DAN DURASI */
                double totalDistance = 0;
                double totalDuration = 0;
                bool validRoute = true;

                for (int i = 0; i < route.Count - 1; i++)
                {
                    int from = route[i];
                    int to = route[i + 1];

                    if (matrix[from][to] == null)
                    {
                        validRoute = false;
                        break;
                    }

                    totalDistance += matrix[from][to].distance;
                    totalDuration += matrix[from][to].time;
                }

                /* Jika ada jalur tidak valid */
                if (!validRoute || totalDistance <= 0)
                {
                    continue;
                }

                /* SIMPAN RUTE SEMUT */
                allRoutes.Add((
                    route,
                    totalDistance,
                    totalDuration
                ));
            }

            /* Jika tidak ada rute valid */
            if (allRoutes.Count == 0)
            {
                break;
            }

            /* EVAPORATION */
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    pheromone[i][j] *= (1 - evaporation);
                }
            }

            /* UPDATE PHEROMONE */
            foreach (var r in allRoutes)
            {
                if (r.dist <= 0)
                {
                    continue;
                }

                double delta = Q / r.dist;

                for (int i = 0; i < r.route.Count - 1; i++)
                {
                    int from = r.route[i];
                    int to = r.route[i + 1];

                    pheromone[from][to] += delta;
                    pheromone[to][from] += delta;
                }
            }

            /* RUTE TERBAIK PADA ITERASI SAAT INI */
            var iterationBest = allRoutes
                .OrderBy(x => x.dist)
                .First();

            /* UPDATE SOLUSI TERBAIK GLOBAL */
            if (iterationBest.dist < bestDistance)
            {
                bestDistance = iterationBest.dist;
                bestDuration = iterationBest.time;
                bestRoute = new List<int>(iterationBest.route);
            }

            /* CEK STAGNASI */
            if (previousIterationBestDistance >= 0 && Math.Abs(iterationBest.dist - previousIterationBestDistance) < 0.000001)
            {
                stagnationCount++;
            }
            else
            {
                stagnationCount = 0;
            }

            /* Simpan nilai iterasi sekarang untuk pembanding iterasi berikutnya */
            previousIterationBestDistance = iterationBest.dist;

            /* SIMPAN HISTORY ITERASI */
            histories.Add(new IterationHistory
            {
                Iteration = iter + 1,
                BestDistance = iterationBest.dist,
                BestDuration = iterationBest.time,
                BestRoute = new List<int>(iterationBest.route)
            });

            /*
             * BERHENTI JIKA NILAI TERBAIK SAMA
             * SELAMA 100 ITERASI BERTURUT-TURUT */
            if (stagnationCount >= maxStagnation)
            {
                break;
            }
        }

        /* RETURN */
        return new AcoResult
        {
            solution = bestRoute,
            distance = bestDistance,
            duration = bestDuration,
            histories = histories
        };
    }

    AcoResult TwoOptFirstImprovement(MatrixCell[][] matrix, AcoResult acoResult)
    {
        /* AMBIL ROUTE ACO */
        var bestRoute = new List<int>(acoResult.solution);

        double bestDistance = CalculateDistance(matrix, bestRoute);
        double bestDuration = 0;

        for (int i = 0; i < bestRoute.Count - 1; i++)
        {
            int from = bestRoute[i];

            int to = bestRoute[i + 1];

            bestDuration += matrix[from][to].time;
        }

        /* HISTORY 2OPT */
        var histories = new List<IterationHistory>();

        /* FLAG IMPROVEMENT */
        bool improved = true;

        int iteration = 0;

        /* LOOP TERUS SAMPAI TIDAK ADA PERBAIKAN */
        while (improved)
        {
            improved = false;

            iteration++;

            /* CEK SEMUA KOMBINASI */
            for (int i = 1; i < bestRoute.Count - 2; i++)
            {
                for (int k = i + 1; k < bestRoute.Count - 1; k++)
                {
                    /* SWAP ROUTE */
                    var newRoute = TwoOptSwap(bestRoute, i, k);

                    /* HITUNG DISTANCE BARU */
                    double newDistance = CalculateDistance(matrix, newRoute);

                    /* JIKA LEBIH BAIK */
                    if (newDistance < bestDistance)
                    {
                        var oldRoute = new List<int>(bestRoute);
                        int swapA = bestRoute[i];
                        int swapB = bestRoute[k];
                        double oldDistance = bestDistance;

                        bestRoute = newRoute;
                        bestDistance = newDistance;

                        /* HITUNG DURATION BARU */
                        bestDuration = 0;

                        for (int x = 0; x < bestRoute.Count - 1; x++)
                        {
                            int from = bestRoute[x];
                            int to = bestRoute[x + 1];

                            bestDuration += matrix[from][to].time;
                        }

                        improved = true;

                        /* SIMPAN HISTORY */
                        histories.Add(new IterationHistory
                        {
                            Iteration = iteration,
                            BestDistance = bestDistance,
                            BestDuration = bestDuration,
                            BestRoute = new List<int>(bestRoute),

                            SwapFromIndex = i,
                            SwapToIndex = k,
                            SwapValue = $"{swapA} <-> {swapB}",
                            RouteBeforeSwap = new List<int>(oldRoute),
                            RouteAfterSwap = new List<int>(bestRoute),
                            DistanceBeforeSwap = oldDistance,
                            DistanceAfterSwap = bestDistance
                        });

                        /* FIRST IMPROVEMENT LANGSUNG ULANG */
                        goto NEXT_ITERATION;
                    }
                }
            }

        NEXT_ITERATION:

            ;
        }

        /* RETURN */
        return new AcoResult
        {
            solution = bestRoute,
            distance = bestDistance,
            duration = bestDuration,
            histories = histories
        };
    }

    // Helper 2opt
    List<int> TwoOptSwap(List<int> route, int i, int k)
    {
        var result = new List<int>();

        /* sebelum i */
        for (int c = 0; c < i; c++)
        {
            result.Add(route[c]);
        }

        /* reverse i sampai k */
        for (int c = k; c >= i; c--)
        {
            result.Add(route[c]);
        }

        /* setelah k */
        for (int c = k + 1; c < route.Count; c++)
        {
            result.Add(route[c]);
        }
        return result;
    }

    // Helper 2opt
    double CalculateDistance(MatrixCell[][] matrix, List<int> route)
    {
        double total = 0;
        for (int i = 0; i < route.Count - 1; i++)
        {
            total += matrix[route[i]]
                      [route[i + 1]]
                      .distance;
        }
        return total;
    }

    private AcoResult GeneticAlgorithm(MatrixCell[][] matrix, int popSize, int nIter, double crossoverRate, double mutationRate, int tournamentSize, bool elitism)
    {
        int n = matrix.Length;
        int start = 0;
        Random rand = new Random();

        /* INISIALISASI POPULASI */
        List<List<int>> population = new();

        List<int> baseRoute = Enumerable.Range(1, n - 1).ToList();

        for (int i = 0; i < popSize; i++)
        {
            List<int> chromosome = new(baseRoute);

            // Fisher-Yates Shuffle
            for (int j = chromosome.Count - 1; j > 0; j--)
            {
                int k = rand.Next(j + 1);
                (chromosome[j], chromosome[k]) = (chromosome[k], chromosome[j]);
            }

            population.Add(chromosome);
        }

        List<int> bestRoute = null;
        double bestDistance = double.MaxValue;
        double bestDuration = double.MaxValue;

        var histories = new List<IterationHistory>();

        /* GENERASI */
        for (int iter = 0; iter < nIter; iter++)
        {
            // Evaluasi populasi
            List<(List<int> chromosome, double fitness)> evaluated = new();

            foreach (var chromosome in population)
            {
                List<int> fullRoute = new() { start };
                fullRoute.AddRange(chromosome);
                fullRoute.Add(start);

                double distance = CalculateDistance(matrix, fullRoute);

                evaluated.Add((chromosome, distance));

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestRoute = new List<int>(fullRoute);

                    bestDuration = 0;

                    for (int i = 0; i < fullRoute.Count - 1; i++)
                    {
                        bestDuration += matrix[
                            fullRoute[i]
                        ][
                            fullRoute[i + 1]
                        ].time;
                    }
                }
            }

            List<List<int>> newPopulation = new();

            /* ELITISM */
            if (elitism && bestRoute != null)
            {
                newPopulation.Add(
                    bestRoute
                        .Skip(1)
                        .Take(bestRoute.Count - 2)
                        .ToList()
                );
            }

            /* SELEKSI + CROSSOVER + MUTASI */
            while (newPopulation.Count < popSize)
            {
                var parent1 = TournamentSelection(
                    population,
                    matrix,
                    tournamentSize,
                    rand
                );

                var parent2 = TournamentSelection(
                    population,
                    matrix,
                    tournamentSize,
                    rand
                );

                List<int> child;

                // Order Crossover (OX)
                if (rand.NextDouble() < crossoverRate)
                {
                    child = OrderCrossover(parent1, parent2, rand);
                }
                else
                {
                    child = new List<int>(parent1);
                }

                // Swap Mutation
                if (rand.NextDouble() < mutationRate)
                {
                    int idx1 = rand.Next(child.Count);
                    int idx2 = rand.Next(child.Count);

                    (child[idx1], child[idx2]) =
                        (child[idx2], child[idx1]);
                }

                newPopulation.Add(child);
            }

            population = newPopulation;

            histories.Add(new IterationHistory
            {
                Iteration = iter + 1,
                BestDistance = bestDistance,
                BestDuration = bestDuration,
                BestRoute = new List<int>(bestRoute)
            });
        }

        return new AcoResult
        {
            solution = bestRoute,
            distance = bestDistance,
            duration = bestDuration,
            histories = histories
        };
    }

    private List<int> TournamentSelection(List<List<int>> population, MatrixCell[][] matrix, int tournamentSize, Random rand)
    {
        List<int> winner = null;
        double bestFitness = double.MaxValue;

        for (int i = 0; i < tournamentSize; i++)
        {
            var candidate = population[rand.Next(population.Count)];

            List<int> route = new() { 0 };
            route.AddRange(candidate);
            route.Add(0);

            double fitness = CalculateDistance(matrix, route);

            if (fitness < bestFitness)
            {
                bestFitness = fitness;
                winner = candidate;
            }
        }

        return new List<int>(winner);
    }

    private List<int> OrderCrossover(List<int> parent1, List<int> parent2, Random rand)
    {
        int size = parent1.Count;

        List<int> child =
            Enumerable.Repeat(-1, size).ToList();

        int start = rand.Next(size);
        int end = rand.Next(size);

        if (start > end)
            (start, end) = (end, start);

        // Salin segmen parent1
        for (int i = start; i <= end; i++)
        {
            child[i] = parent1[i];
        }

        int currentIndex = (end + 1) % size;

        for (int i = 0; i < size; i++)
        {
            int city = parent2[(end + 1 + i) % size];

            if (!child.Contains(city))
            {
                child[currentIndex] = city;
                currentIndex = (currentIndex + 1) % size;
            }
        }

        return child;
    }
}
