using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sipanda.Models.Entities
{
    [Table("v_tarif_lengkap")]
    public class VTarifLengkap
    {
        [Key]
        [Column("tarif_id")]
        public int TarifId { get; set; }

        [Column("peraturan_id")]
        public int PeraturanId { get; set; }

        [Column("jenis_peraturan")]
        public string? JenisPeraturan { get; set; }

        [Column("no_peraturan")]
        public string? NoPeraturan { get; set; }

        [Column("tahun")]
        public short? Tahun { get; set; }

        [Column("peraturan_status")]
        public string? PeraturanStatus { get; set; }

        [Column("faskes_id")]
        public int FaskesId { get; set; }

        [Column("kode_faskes")]
        public string? KodeFaskes { get; set; }

        [Column("nama_faskes")]
        public string? NamaFaskes { get; set; }

        [Column("kode_rekening")]
        public string? KodeRekening { get; set; }

        [Column("nama_rekening")]
        public string? NamaRekening { get; set; }

        [Column("kategori_id")]
        public int KategoriId { get; set; }

        [Column("nama_kategori")]
        public string? NamaKategori { get; set; }

        [Column("kelompok_kategori")]
        public string? KelompokKategori { get; set; }

        [Column("komponen_id")]
        public int KomponenId { get; set; }

        [Column("nama_komponen")]
        public string? NamaKomponen { get; set; }

        [Column("satuan")]
        public string? Satuan { get; set; }

        [Column("harga")]
        public long Harga { get; set; }

        [Column("tanggal_mulai")]
        public DateOnly TanggalMulai { get; set; }

        [Column("tanggal_akhir")]
        public DateOnly? TanggalAkhir { get; set; }

        [Column("status")]
        public string? Status { get; set; }
    }
}
