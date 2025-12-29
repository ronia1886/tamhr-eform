using TAMHR.ESS.Domain;
using Agit.Domain.UnitOfWork;
using Agit.Domain;
using Agit.Domain.Repository;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;
using Agit.Common.Extensions;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    public class DashboardAreaActivityService : DomainServiceBase
    {
        #region Constructor
        /// <summary>
        /// Public constructor.
        /// </summary>

        #endregion

        #region Domain Repository
        protected IReadonlyRepository<DashboardChartAreaActivityViewModel> DashboardChartAreaActivityReadOnlyViewRepository => UnitOfWork.GetRepository<DashboardChartAreaActivityViewModel>();
        protected IReadonlyRepository<DashboardListAreaActivityViewModel> DashboardListAreaActivityReadOnlyViewRepository => UnitOfWork.GetRepository<DashboardListAreaActivityViewModel>();
        protected IReadonlyRepository<DashboardChartAreaActivityByFilterViewModel> DashboardListAreaActivityByFilterReadOnlyViewRepository => UnitOfWork.GetRepository<DashboardChartAreaActivityByFilterViewModel>();

        protected IRepository<SafetyIncidentViewModel> SafetyIncidentViewRepository => UnitOfWork.GetRepository<SafetyIncidentViewModel>();
        protected IReadonlyRepository<SafetyIncidentViewModel> SafetyIncidentReadOnlyViewRepository => UnitOfWork.GetRepository<SafetyIncidentViewModel>();
        // Repository untuk Role Akses berdasarkan NoReg
        protected IRepository<RoleAreaActivitytModel> RoleAreaActivityRepository => UnitOfWork.GetRepository<RoleAreaActivitytModel>();
        protected IReadonlyRepository<RoleAreaActivitytModel> RoleAreaActivityReadOnlyRepository => UnitOfWork.GetRepository<RoleAreaActivitytModel>();
        protected IReadonlyRepository<TotalWorkDayChartFRSRModel> TotalWorkDayChartFRSRReadOnlyRepository => UnitOfWork.GetRepository<TotalWorkDayChartFRSRModel>();
        #endregion

        #region Variables & Properties
        /// <summary>
        /// Field that hold properties that can be updated for bank entity.
        /// </summary>
        private readonly string[] Properties = new[] {
            //"Id",
            "TotalActual",
            "Remark",
            "DivisionCode",
            "DivisionName",
            "AreaId",
            "AreaName",
            "EquipmentId",
            "EquipmentName",
            "RowStatus",
            "DeletedOn"
        };

        #endregion
        public DashboardAreaActivityService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IQueryable<DashboardListAreaActivityViewModel> GetQuery()
        {
            return DashboardListAreaActivityReadOnlyViewRepository.Fetch()
                .AsNoTracking();
        }

        public IEnumerable<DashboardListAreaActivityViewModel> Gets(string NoReg = null, string periode = null, string divisionCode = null, string areaId = null)
        {
            var query = GetQuery();

            // **Filter berdasarkan hak akses NoReg**
            if (!string.IsNullOrEmpty(NoReg))
            {
                Console.WriteLine($"Filtering by NoReg: {NoReg}");

                // Ambil daftar role akses dari tabel RoleAreaActivity berdasarkan NoReg
                var userAccess = RoleAreaActivityReadOnlyRepository.Fetch()
                    .Where(a => a.NoReg == NoReg && a.IsActive == true && a.RowStatus == true)
                    .Select(a => new { a.DivisionCode, a.AreaId })
                    .ToList(); // Eksekusi query ke database

                var allowedAreaIds = userAccess
                    .Select(a => a.AreaId) // Konversi Guid ke string untuk perbandingan
                    .Distinct()
                    .ToList();

                // Terapkan filter jika ada akses yang ditemukan
                if (allowedAreaIds.Any())
                {
                    //query = query.Where(e =>
                    //    allowedAreaIds.Contains(e.AreaId)
                    //);
                    var allowedAreaQueryable = allowedAreaIds.AsQueryable();

                    query = query.Join(
                        allowedAreaQueryable,
                        a => a.AreaId,
                        id => id,
                        (a, id) => a
                    );
                }
            }
            return query.ToList();
        }

        public IQueryable<DashboardChartAreaActivityViewModel> GetQueryChart(string NoReg = null)
        {
            return DashboardChartAreaActivityReadOnlyViewRepository.Fetch()
                .AsNoTracking();
        }

        public IEnumerable<DashboardChartAreaActivityViewModel> GetsChartOld(string category, string noreg, string periode = null, string divisionCode = null, string areaId = null)
        {
            Console.WriteLine("categoryChart2>> " + category);
            var query = GetQueryChart()
                .Where(x => x.categoryChart == category)
                .Select(x => new DashboardChartAreaActivityViewModel
                {
                    category = x.category,
                    value = x.value,
                    periode= x.periode
                });

            return query.ToList();
        }
        public Object GetsChart(string categoryChart, string noreg, string periode = null, string divisionCode = null, string areaId = null)
        {
            Console.WriteLine("categoryChart2>> " + categoryChart);

            var query = DashboardListAreaActivityByFilterReadOnlyViewRepository.Fetch()?.AsNoTracking();

            if (query == null)
            {
                throw new Exception("Query is null. Ensure data exists in the repository.");
            }

            if (!string.IsNullOrEmpty(categoryChart))
                query = query.Where(x => x.categoryChart == categoryChart);

            var userAccess = RoleAreaActivityReadOnlyRepository.Fetch()
                .Where(a => a.NoReg == noreg && a.IsActive && a.RowStatus)
                .Select(a => new { a.DivisionCode, a.AreaId })
                .ToList();

            var allowedAreaIds = userAccess
                .Select(a => a.DivisionCode)
                .Distinct()
                .ToList();

            if (allowedAreaIds.Any())
            {
                //query = query.Where(e => allowedAreaIds.Contains(e.DivisionCode));
                var allowedAreaQueryable = allowedAreaIds.AsQueryable();

                query = query.Join(
                    allowedAreaQueryable,
                    a => a.DivisionCode,
                    id => id,
                    (a, id) => a
                );
            }

            var dataList = query.ToList();

            // Ambil waktu sekarang untuk digunakan jika categoryChart = ProjectActivity
            var currentPeriode = DateTime.Now.ToString("yyyy-MM");

            // Jika categoryChart = Project_Activity, ubah periode-nya di awal
            if (categoryChart == "Project_Activity")
            {
                dataList = dataList.Select(x => new DashboardChartAreaActivityByFilterViewModel
                {
                    category = x.category,
                    value = x.value,
                    periode = currentPeriode
                }).ToList();
            }

            var result = dataList
                .GroupBy(x => new { x.category, x.periode })
                .Select(g => new
                {
                    category = g.Key.category,
                    value = g.Sum(x => x.value),
                    periode = g.Key.periode
                })
                .OrderBy(x => x.periode)
                .ToList();

            Console.WriteLine("Tes hasil donut chart>> " + result);

            return result;
        }


        public Object GetsChartByFilter(string categoryChart, string noreg, string periode = null, string divisionCode = null, string areaId = null)
        {
            Console.WriteLine("categoryChart2>> " + categoryChart);

            var query = DashboardListAreaActivityByFilterReadOnlyViewRepository.Fetch()?.AsNoTracking();

            // Cek apakah query null
            if (query == null)
            {
                throw new Exception("Query is null. Ensure data exists in the repository.");
            }

            // Terapkan filter berdasarkan parameter
            if (!string.IsNullOrEmpty(categoryChart))
                query = query.Where(x => x.categoryChart == categoryChart);

            if (!string.IsNullOrEmpty(periode))
                query = query.Where(x => x.periode == periode);

            if (!string.IsNullOrEmpty(divisionCode))
            {
                query = query.Where(x => x.DivisionCode == divisionCode);
            }
            else
            {
                // Ambil daftar role akses dari tabel RoleAreaActivity berdasarkan NoReg
                var userAccess = RoleAreaActivityReadOnlyRepository.Fetch()
                    .Where(a => a.NoReg == noreg && a.IsActive == true && a.RowStatus == true)
                    .Select(a => new { a.DivisionCode, a.AreaId })
                    .ToList(); // Eksekusi query ke database

                var allowedAreaIds = userAccess
                    .Select(a => a.DivisionCode) // Konversi Guid ke string untuk perbandingan
                    .Distinct()
                    .ToList();

                // Terapkan filter jika ada akses yang ditemukan
                if (allowedAreaIds.Any())
                {
                    //query = query.Where(e =>
                    //    allowedAreaIds.Contains(e.DivisionCode)
                    //);
                    var allowedAreaQueryable = allowedAreaIds.AsQueryable();

                    query = query.Join(
                        allowedAreaQueryable,
                        a => a.DivisionCode,
                        id => id,
                        (a, id) => a
                    );
                }
            }
                
            if (!string.IsNullOrEmpty(areaId))
                query = query.Where(x => x.AreaId == areaId);

            Console.WriteLine("Tes query sebelum eksekusi>> " + query); // Debugging SQL Query

            // Eksekusi query sebelum pemrosesan lebih lanjut
            var dataList = query.ToList();

            var result = dataList
                .GroupBy(x => new { x.category, x.periode}) // Grup berdasarkan kategori dan periode
                .Select(g => new
                {
                    category = g.Key.category,
                    value = g.Sum(x => x.value), // Gunakan 0 jika null
                    periode = g.Key.periode
                })
                .OrderBy(x => x.periode) // Urutkan berdasarkan periode
                .ToList();

            Console.WriteLine("Tes hasil donut chart>> " + result);

            return result;
        }

        public Object GetsBarChartOld(string categoryChart, string noreg, string periode = null, string divisionCode = null, string areaId = null)
        {
            Console.WriteLine("categoryChart>> " + categoryChart);

            var rawData = GetQueryChart()
                .AsNoTracking()
                .Where(x => x.categoryChart == categoryChart)
                .GroupBy(x => x.periode) // Kelompokkan berdasarkan bulan/periode
                .Select(g => new
                {
                    Periode = g.Key,
                    DataPerCategory = g.ToDictionary(
                        x => x.category,
                        x => x.value
                    )
                })
                .OrderBy(x => x.Periode)
                .ToList();

            // Ambil daftar kategori unik yang ada di dataset
            var uniqueCategories = rawData
                .SelectMany(x => x.DataPerCategory.Keys)
                .Distinct()
                .ToList();

            // Buat format series data untuk Kendo Chart
            var seriesData = uniqueCategories.Select(category => new
            {
                name = category,
                data = rawData.Select(x => x.DataPerCategory.ContainsKey(category) ? x.DataPerCategory[category] : 0).ToArray(),
                axis = "Total"
            }).ToList();

            return new
            {
                series = seriesData,
                categoryAxis = new { categories = rawData.Select(x => x.Periode).ToArray() },
                valueAxis = new[]
                {
                    new { name = "Total", labels = new { format = "{0}%" } }
                }
            };
        }

        public Object GetsBarChart(string categoryChart, string noreg, string periode = null, string divisionCode = null, string areaId = null)
        {
            Console.WriteLine("categoryChart>> " + categoryChart);

            if (DashboardListAreaActivityByFilterReadOnlyViewRepository == null)
            {
                throw new Exception("Repository is not initialized.");
            }

            var query = DashboardListAreaActivityByFilterReadOnlyViewRepository.Fetch()?.AsNoTracking();
            if (query == null)
            {
                throw new Exception("Query is null. Ensure data exists in the repository.");
            }

            if (!string.IsNullOrEmpty(categoryChart))
                query = query.Where(x => x.categoryChart == categoryChart);

            var userAccess = RoleAreaActivityReadOnlyRepository.Fetch()
                .Where(a => a.NoReg == noreg && a.IsActive == true && a.RowStatus == true)
                .Select(a => new { a.DivisionCode, a.AreaId })
                .ToList();

            var allowedAreaIds = userAccess
                .Select(a => a.DivisionCode)
                .Distinct()
                .ToList();

            if (allowedAreaIds.Any())
            {
                //query = query.Where(e => allowedAreaIds.Contains(e.DivisionCode));
                var allowedAreaQueryable = allowedAreaIds.AsQueryable();

                query = query.Join(
                    allowedAreaQueryable,
                    a => a.DivisionCode,
                    id => id,
                    (a, id) => a
                );
            }

            Console.WriteLine("Tes query sebelum eksekusi>> " + query);

            var dataList = query.ToList();

            var rawData = dataList
                .Where(x => x.periode != null && x.category != null && x.value != null)
                .GroupBy(x => x.periode ?? "Unknown")
                .Select(g => new
                {
                    Periode = g.Key,
                    DataPerCategory = g.GroupBy(y => y.category ?? "Unknown")
                        .ToDictionary(
                            y => y.Key,
                            y => y.Sum(z => z.value)
                        )
                });

            // 🔹 Urutan kategori yang diinginkan
            List<string> categoryOrder = new List<string> { "Dh", "Dph", "Officer", "Operation/Staff", "Other" };

            if (categoryChart == "TrainingRecord")
            {
                rawData = rawData
                    .Select(g => new
                    {
                        Periode = g.Periode,
                        DataPerCategory = g.DataPerCategory
                            .OrderBy(y => categoryOrder.IndexOf(y.Key) == -1 ? int.MaxValue : categoryOrder.IndexOf(y.Key)) // Urutkan sesuai daftar di atas
                            .ToDictionary(y => y.Key, y => y.Value)
                    })
                    .OrderBy(g => g.Periode); // Urutkan Periode setelah kategori
            }
            else
            {
                // 🔹 Untuk categoryChart lain, hanya urutkan berdasarkan periode saja
                rawData = rawData.OrderBy(x => x.Periode);
            }

            var rawDataList = rawData.ToList();

            var uniqueCategories = rawDataList
               .SelectMany(x => x.DataPerCategory.Keys)
               .Distinct()
               .ToList();

            if (categoryChart == "TrainingRecord")
            {
                uniqueCategories = categoryOrder
                .Where(cat => rawDataList.Any(x => x.DataPerCategory.ContainsKey(cat)))
                .ToList();
            }

            Console.WriteLine("Tes uniqueCategories>> " + uniqueCategories.Count);

            var seriesData = uniqueCategories.Select(category => new
            {
                name = category,
                data = rawDataList.Select(x => x.DataPerCategory.ContainsKey(category) ? x.DataPerCategory[category] : 0).ToArray(),
                axis = "Total"
            }).ToList();

            return new
            {
                series = seriesData,
                categoryAxis = new { categories = rawDataList.Select(x => x.Periode).ToArray() },
                valueAxis = new[]
                {
                    new { name = "Total", labels = new { format = "{0}%" } }
                }
            };
        }

        public Object GetsMultipleChart(string categoryChart, string noreg, string periode = null, string divisionCode = null, string areaId = null)
        {
            Console.WriteLine("categoryChart>> " + categoryChart);

            if (SafetyIncidentReadOnlyViewRepository == null)
            {
                throw new Exception("Repository is not initialized.");
            }

            var query = SafetyIncidentReadOnlyViewRepository.Fetch()?.Where(x => x.RowStatus == true).AsNoTracking();
            if (query == null)
            {
                throw new Exception("Query is null. Ensure data exists in the repository.");
            }

            //🔹 Fetch total active workdays
            var getTotalWorkDay = TotalWorkDayChartFRSRReadOnlyRepository.Fetch()?.AsNoTracking();
            if (getTotalWorkDay == null || !getTotalWorkDay.Any())
            {
                throw new Exception("No data available in TotalWorkDayChartFRSRReadOnlyRepository.");
            }

            var totalWorkDay = getTotalWorkDay.FirstOrDefault()?.totalWorkDay ?? 0;
            if (totalWorkDay == 0)
            {
                throw new Exception("Total workday cannot be zero or null.");
            }

            // 🔹 Ambil data per bulan dan ubah format periode ke "MMM"
            //var monthlyData = query
            //    .Where(x => x.IncidentTypeCode == "working_accident" || x.IncidentTypeCode == "traffic_accident")
            //    .GroupBy(x => x.Periode != null ? DateTime.Parse(x.Periode).ToString("MMM") : "Unknown")
            //    .Select(g => new
            //    {
            //        Periode = g.Key,
            //        SeverityRate = Math.Round(((g.Sum(x => (decimal?)x.LossTime2) ?? 0) * 1000000m) / totalWorkDay, 2, MidpointRounding.AwayFromZero),
            //        FrequencyRate = Math.Round(((decimal)g.Count() * 1000000m) / totalWorkDay, 2, MidpointRounding.AwayFromZero)
            //    })
            //    .ToList();

            var monthlyData = query
                            .Where(x => x.IncidentTypeCode == "working_accident" || x.IncidentTypeCode == "traffic_accident").ToList()
                            .GroupBy(x => {
                                try
                                {
                                    return !string.IsNullOrEmpty(x.Periode)
                                        ? DateTime.Parse(x.Periode).ToString("MMM")
                                        : "Unknown";
                                }
                                catch
                                {
                                    return "Unknown";
                                }
                            })
                            .Select(g => new
                            {
                                Periode = g.Key,
                                SeverityRate = Math.Round(((g.Sum(x => (decimal?)x.LossTime2) ?? 0) * 1000000m) / totalWorkDay, 2, MidpointRounding.AwayFromZero),
                                FrequencyRate = Math.Round(((decimal)g.Count() * 1000000m) / totalWorkDay, 2, MidpointRounding.AwayFromZero)
                            })
                            .ToList();

            // 🔹 Ambil daftar bulan yang ada di monthlyData dan urutkan dari awal tahun hingga sekarang
            var months = monthlyData
                .Select(x => x.Periode)
                .Distinct()
                .OrderBy(m => DateTime.ParseExact(m, "MMM", System.Globalization.CultureInfo.InvariantCulture))
                .ToList();

            // 🔹 Isi data sesuai dengan bulan yang tersedia
            var severityRates = months.Select(m =>
                monthlyData.FirstOrDefault(x => x.Periode == m)?.SeverityRate ?? 0).ToArray();

            var frequencyRates = months.Select(m =>
                monthlyData.FirstOrDefault(x => x.Periode == m)?.FrequencyRate ?? 0).ToArray();

            // 🔹 Return data sesuai format yang diinginkan
            return new
            {
                labels = months,
                severityRate = severityRates,
                frequencyRate = frequencyRates
            };
        }

        public Object GetsMultipleChartByFilter(string categoryChart, string noreg, string periode = null, string divisionCode = null, string areaId = null)
        {
            Console.WriteLine("categoryChart>> " + categoryChart);

            // Cek apakah repository null
            if (SafetyIncidentReadOnlyViewRepository == null)
            {
                throw new Exception("Repository is not initialized.");
            }

            var query = SafetyIncidentReadOnlyViewRepository.Fetch()?.Where(x => x.RowStatus == true).AsNoTracking();

            // Cek apakah query null
            if (query == null)
            {
                throw new Exception("Query is null. Ensure data exists in the repository.");
            }

            // Terapkan filter berdasarkan parameter
            if (!string.IsNullOrEmpty(periode))
                query = query.Where(x => x.Periode == periode);

            if (!string.IsNullOrEmpty(divisionCode))
            {
                query = query.Where(x => x.DivisionCode == divisionCode);
            }
            else
            {
                // Ambil daftar role akses dari tabel RoleAreaActivity berdasarkan NoReg
                var userAccess = RoleAreaActivityReadOnlyRepository.Fetch()
                    .Where(a => a.NoReg == noreg && a.IsActive == true && a.RowStatus == true)
                    .Select(a => new { a.DivisionCode, a.AreaId })
                    .ToList(); // Eksekusi query ke database

                var allowedDivisionCodes = userAccess
                    .Select(a => a.DivisionCode)
                    .Distinct()
                    .ToList();

                if (allowedDivisionCodes.Any())
                {
                    //query = query.Where(e => allowedDivisionCodes.Contains(e.DivisionCode));
                    var allowedDivisionQueryable = allowedDivisionCodes.AsQueryable();

                    query = query.Join(
                        allowedDivisionQueryable,
                        a => a.DivisionCode,
                        id => id,
                        (a, id) => a
                    );
                }
            }

            if (!string.IsNullOrEmpty(areaId))
                query = query.Where(x => x.AreaId == areaId);

            Console.WriteLine("Tes query sebelum eksekusi>> " + query); // Debugging SQL Query

            // Eksekusi query sebelum pengolahan
            var dataList = query.ToList(); // Eksekusi query

            // Perhitungan SeverityRate dan FrequencyRate
            var getTotalWorkDay = TotalWorkDayChartFRSRReadOnlyRepository.Fetch()?.AsNoTracking();
            if (getTotalWorkDay == null || !getTotalWorkDay.Any())
            {
                throw new Exception("No data available in TotalWorkDayChartFRSRReadOnlyRepository.");
            }

            var totalWorkDay = getTotalWorkDay.FirstOrDefault()?.totalWorkDay ?? 0;
            if (totalWorkDay == 0)
            {
                throw new Exception("Total workday cannot be zero or null.");
            }

            var rawData = dataList
                .Where(x => x.Periode != null && (x.IncidentTypeCode == "working_accident" || x.IncidentTypeCode == "traffic_accident"))
                .GroupBy(x =>
                {
                    try
                    {
                        // Parsing periode menggunakan format "yyyy-MM"
                        return !string.IsNullOrEmpty(x.Periode)
                            ? DateTime.ParseExact(x.Periode, "yyyy-MM", System.Globalization.CultureInfo.InvariantCulture).ToString("MMM")
                            : "Unknown";
                    }
                    catch (FormatException)
                    {
                        return "Unknown"; // Gunakan fallback value jika parsing gagal
                    }
                })
                .Select(g => new
                {
                    Periode = g.Key,
                    SeverityRate = Math.Round(((g.Sum(x => (decimal?)x.LossTime2) ?? 0) * 1000000m) / totalWorkDay, 2, MidpointRounding.AwayFromZero),
                    FrequencyRate = Math.Round(((decimal)g.Count() * 1000000m) / totalWorkDay, 2, MidpointRounding.AwayFromZero)
                })
                .OrderBy(x => x.Periode)
                .ToList();

            Console.WriteLine("Tes rawData>> " + rawData.Count);

            // Ambil daftar bulan
            var months = rawData
                .Select(x => x.Periode)
                .Distinct()
                .OrderBy(m =>
                {
                    try
                    {
                        return DateTime.ParseExact(m, "MMM", System.Globalization.CultureInfo.InvariantCulture);
                    }
                    catch
                    {
                        return DateTime.MaxValue; // Untuk menghindari error pada data invalid
                    }
                })
                .ToList();

            // Isi data untuk setiap bulan
            var severityRates = months.Select(m => rawData.FirstOrDefault(x => x.Periode == m)?.SeverityRate ?? 0).ToArray();
            var frequencyRates = months.Select(m => rawData.FirstOrDefault(x => x.Periode == m)?.FrequencyRate ?? 0).ToArray();

            // Return hasil
            return new
            {
                labels = months,
                severityRate = severityRates,
                frequencyRate = frequencyRates
            };
        }

        public Object GetsBarChartByFilter(string categoryChart, string noreg, string periode = null, string divisionCode = null, string areaId = null)
        {
            Console.WriteLine("categoryChart>> " + categoryChart);

            // Cek apakah repository null
            if (DashboardListAreaActivityByFilterReadOnlyViewRepository == null)
            {
                throw new Exception("Repository is not initialized.");
            }

            var query = DashboardListAreaActivityByFilterReadOnlyViewRepository.Fetch()?.AsNoTracking();

            // Cek apakah query null
            if (query == null)
            {
                throw new Exception("Query is null. Ensure data exists in the repository.");
            }

            // Terapkan filter berdasarkan parameter
            if (!string.IsNullOrEmpty(categoryChart))
                query = query.Where(x => x.categoryChart == categoryChart);

            if (!string.IsNullOrEmpty(periode))
                query = query.Where(x => x.periode == periode);

            if (!string.IsNullOrEmpty(divisionCode)) {
                query = query.Where(x => x.DivisionCode == divisionCode);
            }
            else
            {
                // Ambil daftar role akses dari tabel RoleAreaActivity berdasarkan NoReg
                var userAccess = RoleAreaActivityReadOnlyRepository.Fetch()
                    .Where(a => a.NoReg == noreg && a.IsActive == true && a.RowStatus == true)
                    .Select(a => new { a.DivisionCode, a.AreaId })
                    .ToList(); // Eksekusi query ke database

                var allowedAreaIds = userAccess
                    .Select(a => a.DivisionCode) // Konversi Guid ke string untuk perbandingan
                    .Distinct()
                    .ToList();

                // Terapkan filter jika ada akses yang ditemukan
                if (allowedAreaIds.Any())
                {
                    var allowedAreaQueryable = allowedAreaIds.AsQueryable();

                    query = query.Join(
                        allowedAreaQueryable,
                        a => a.DivisionCode,
                        id => id,
                        (a, id) => a
                    );
                }
            }

            if (!string.IsNullOrEmpty(areaId))
                query = query.Where(x => x.AreaId == areaId);

            Console.WriteLine("Tes query sebelum eksekusi>> " + query); // Debugging SQL Query

            // Eksekusi query sebelum GroupBy untuk menghindari error EntityQueryable<T>
            var dataList = query.ToList(); // ✅ Eksekusi query di database sebelum pengolahan lebih lanjut

            var rawData = dataList
                .Where(x => x.periode != null && x.category != null && x.value != null) // Hindari null reference
                .GroupBy(x => x.periode ?? "Unknown") // Gunakan "Unknown" jika periode null
                .Select(g => new
                {
                    Periode = g.Key,
                    DataPerCategory = g.GroupBy(y => y.category ?? "Unknown") // Gunakan "Unknown" jika kategori null
                        .ToDictionary(
                            y => y.Key,
                            y => y.Sum(z => z.value) // Gunakan 0 jika value null
                        )
                })
                .OrderBy(x => x.Periode)
                .ToList();

            Console.WriteLine("Tes rawData>> " + rawData.Count);

            // Ambil daftar kategori unik
            var uniqueCategories = rawData
                .SelectMany(x => x.DataPerCategory.Keys)
                .Distinct()
                .ToList();

            Console.WriteLine("Tes uniqueCategories>> " + uniqueCategories.Count);

            // Buat format series data untuk Kendo Chart
            var seriesData = uniqueCategories.Select(category => new
            {
                name = category,
                data = rawData.Select(x => x.DataPerCategory.ContainsKey(category) ? x.DataPerCategory[category] : 0).ToArray(),
                axis = "Total"
            }).ToList();

            return new
            {
                series = seriesData,
                categoryAxis = new { categories = rawData.Select(x => x.Periode).ToArray() },
                valueAxis = new[]
                {
            new { name = "Total", labels = new { format = "{0}%" } }
        }
            };
        }


    }

}