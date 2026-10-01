using Sipanda.Models.Entities;

namespace Sipanda.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalRekening { get; set; }
        public int TotalSkpd { get; set; }
        public int TotalFaskes { get; set; }
        public int TotalPeraturan { get; set; }
        public int TotalKategoriInduk { get; set; }
        public int TotalSubKategori { get; set; }
        public int TotalKomponen { get; set; }
        public int TotalTarifAktif { get; set; }

        public List<FaskesStatItem> FaskesStats { get; set; } = new();
        public List<KategoriStatItem> KategoriStats { get; set; } = new();
        public List<MasterPeraturan> PeraturanList { get; set; } = new();
        public List<VTarifLengkap> RecentTarifList { get; set; } = new();
    }

    public class FaskesStatItem
    {
        public int FaskesId { get; set; }
        public string KodeFaskes { get; set; } = string.Empty;
        public string NamaFaskes { get; set; } = string.Empty;
        public string KodeRekening { get; set; } = string.Empty;
        public int TotalTarif { get; set; }
        public long MinTarif { get; set; }
        public long MaxTarif { get; set; }
        public double AvgTarif { get; set; }
    }

    public class KategoriStatItem
    {
        public string NamaKategori { get; set; } = string.Empty;
        public int JumlahKomponen { get; set; }
        public int JumlahTarif { get; set; }
    }
}
