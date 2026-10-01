using Sipanda.Models.Entities;

namespace Sipanda.Models.ViewModels
{
    public class TarifFilterViewModel
    {
        public string? Keyword { get; set; }
        public int? FaskesId { get; set; }
        public int? KategoriId { get; set; }
        public int? PeraturanId { get; set; }
        public string? Status { get; set; }
        public long? MinHarga { get; set; }
        public long? MaxHarga { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);

        public List<VTarifLengkap> Items { get; set; } = new();

        // Filter Options
        public List<MasterFaskes> AvailableFaskes { get; set; } = new();
        public List<MasterKategori> AvailableKategori { get; set; } = new();
        public List<MasterPeraturan> AvailablePeraturan { get; set; } = new();
    }
}
