using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sipanda.Models.Entities
{
    [Table("tarif_retribusi")]
    public class TarifRetribusi
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Peraturan dasar hukum wajib dipilih")]
        [Column("peraturan_id")]
        [Display(Name = "Dasar Peraturan")]
        public int PeraturanId { get; set; }

        [Required(ErrorMessage = "Fasilitas Kesehatan wajib dipilih")]
        [Column("faskes_id")]
        [Display(Name = "Fasilitas Kesehatan")]
        public int FaskesId { get; set; }

        [Required(ErrorMessage = "Kategori Layanan wajib dipilih")]
        [Column("kategori_id")]
        [Display(Name = "Kategori Layanan")]
        public int KategoriId { get; set; }

        [Required(ErrorMessage = "Komponen Layanan wajib dipilih")]
        [Column("komponen_id")]
        [Display(Name = "Layanan / Tindakan Medis")]
        public int KomponenId { get; set; }

        [Required(ErrorMessage = "Nominal Tarif (Harga) wajib diisi")]
        [Column("harga")]
        [Display(Name = "Nominal Tarif (Rp)")]
        [Range(0, 100000000000, ErrorMessage = "Tarif harus positif")]
        public long Harga { get; set; }

        [Required(ErrorMessage = "Tanggal Mulai Berlaku wajib diisi")]
        [Column("tanggal_mulai")]
        [DataType(DataType.Date)]
        [Display(Name = "Tanggal Mulai Berlaku")]
        public DateOnly TanggalMulai { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        [Column("tanggal_akhir")]
        [DataType(DataType.Date)]
        [Display(Name = "Tanggal Berakhir (Opsional)")]
        public DateOnly? TanggalAkhir { get; set; }

        [Required(ErrorMessage = "Status tarif wajib diisi")]
        [StringLength(20)]
        [Column("status")]
        [Display(Name = "Status")]
        public string Status { get; set; } = "aktif";

        // Navigation
        [ForeignKey("PeraturanId")]
        public virtual MasterPeraturan? Peraturan { get; set; }

        [ForeignKey("FaskesId")]
        public virtual MasterFaskes? Faskes { get; set; }

        [ForeignKey("KategoriId")]
        public virtual MasterKategori? Kategori { get; set; }

        [ForeignKey("KomponenId")]
        public virtual MasterKomponen? Komponen { get; set; }
    }
}
