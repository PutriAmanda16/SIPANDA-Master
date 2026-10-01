using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sipanda.Data;
using Sipanda.Models.ViewModels;

namespace Sipanda.Controllers
{
    public class HomeController : Controller
    {
        private readonly SipandaDbContext _context;

        public HomeController(SipandaDbContext context)
        {
            _context = context;
        }

        // Halaman Utama: Dashboard Publik Tanpa Login
        public async Task<IActionResult> Index()
        {
            var vm = new DashboardViewModel
            {
                TotalRekening = await _context.MasterRekening.CountAsync(),
                TotalSkpd = await _context.MasterSkpd.CountAsync(),
                TotalFaskes = await _context.MasterFaskes.CountAsync(),
                TotalPeraturan = await _context.MasterPeraturan.CountAsync(),
                TotalKategoriInduk = await _context.MasterKategori.Where(k => k.ParentId == null).CountAsync(),
                TotalSubKategori = await _context.MasterKategori.Where(k => k.ParentId != null).CountAsync(),
                TotalKomponen = await _context.MasterKomponen.CountAsync(),
                TotalTarifAktif = await _context.TarifRetribusi.Where(t => t.Status == "aktif").CountAsync(),
                PeraturanList = await _context.MasterPeraturan.OrderByDescending(p => p.Tahun).ToListAsync()
            };

            // Statistik per Faskes
            var faskesList = await _context.MasterFaskes
                .Include(f => f.Rekening)
                .ToListAsync();

            foreach (var f in faskesList)
            {
                var tarifFaskes = await _context.TarifRetribusi
                    .Where(t => t.FaskesId == f.Id && t.Status == "aktif")
                    .ToListAsync();

                vm.FaskesStats.Add(new FaskesStatItem
                {
                    FaskesId = f.Id,
                    KodeFaskes = f.KodeFaskes,
                    NamaFaskes = f.NamaFaskes,
                    KodeRekening = f.Rekening?.KodeRekening ?? "-",
                    TotalTarif = tarifFaskes.Count,
                    MinTarif = tarifFaskes.Any() ? tarifFaskes.Min(t => t.Harga) : 0,
                    MaxTarif = tarifFaskes.Any() ? tarifFaskes.Max(t => t.Harga) : 0,
                    AvgTarif = tarifFaskes.Any() ? tarifFaskes.Average(t => t.Harga) : 0
                });
            }

            // Top Kategori Induk
            var kategoriInduk = await _context.MasterKategori
                .Where(k => k.ParentId == null)
                .Include(k => k.SubKategoriList)
                .ThenInclude(s => s.KomponenList)
                .Take(6)
                .ToListAsync();

            foreach (var kat in kategoriInduk)
            {
                int jmlKomp = kat.SubKategoriList.SelectMany(s => s.KomponenList).Count() + 
                              await _context.MasterKomponen.Where(c => c.KategoriId == kat.Id).CountAsync();
                
                int jmlTarif = await _context.TarifRetribusi.Where(t => t.KategoriId == kat.Id || 
                    _context.MasterKategori.Where(k => k.ParentId == kat.Id).Select(k => k.Id).Contains(t.KategoriId)).CountAsync();

                vm.KategoriStats.Add(new KategoriStatItem
                {
                    NamaKategori = kat.NamaKategori,
                    JumlahKomponen = jmlKomp,
                    JumlahTarif = jmlTarif
                });
            }

            // Contoh Data Tarif Terbaru / Teratas
            vm.RecentTarifList = await _context.VTarifLengkap
                .Take(10)
                .ToListAsync();

            return View(vm);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
