using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sipanda.Models.Entities
{
    [Table("budget_pendapatan_versi_detail")]
    public class BudgetPendapatanVersiDetail
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("versi_id")]
        public int VersiId { get; set; }

        [ForeignKey("VersiId")]
        public BudgetPendapatanVersi? Versi { get; set; }

        [Required]
        [Column("detail_id")]
        public int DetailId { get; set; }

        [ForeignKey("DetailId")]
        public BudgetPendapatanDetail? Detail { get; set; }

        [Column("tarif_retribusi_id")]
        [Display(Name = "Referensi Tarif Perda 7")]
        public int? TarifRetribusiId { get; set; }

        [ForeignKey("TarifRetribusiId")]
        public TarifRetribusi? TarifRetribusi { get; set; }

        [Required]
        [Column("uraian_layanan")]
        [Display(Name = "Uraian Komponen / Layanan")]
        public string UraianLayanan { get; set; } = string.Empty;

        [Required]
        [Column("volume")]
        [Display(Name = "Volume / Koefisien")]
        public decimal Volume { get; set; } = 1;

        [Required]
        [Column("satuan")]
        [StringLength(50)]
        [Display(Name = "Satuan")]
        public string Satuan { get; set; } = "Layanan";

        [Required]
        [Column("tarif_satuan")]
        [Display(Name = "Tarif Satuan (Rp)")]
        public long TarifSatuan { get; set; }

        [Required]
        [Column("nilai_target")]
        [Display(Name = "Jumlah Target (Rp)")]
        public long NilaiTarget { get; set; }

        // Field Komparasi Khusus PAK / Perubahan (Semula)
        [Column("volume_semula")]
        [Display(Name = "Volume Semula")]
        public decimal VolumeSemula { get; set; } = 0;

        [Column("tarif_semula")]
        [Display(Name = "Tarif Semula (Rp)")]
        public long TarifSemula { get; set; } = 0;

        [Column("nilai_semula")]
        [Display(Name = "Jumlah Semula (Rp)")]
        public long NilaiSemula { get; set; } = 0;

        [Column("keterangan")]
        [Display(Name = "Keterangan / Dasar Perhitungan")]
        public string? Keterangan { get; set; }

        [NotMapped]
        public long SelisihNilai => NilaiTarget - NilaiSemula;

        [NotMapped]
        public decimal SelisihVolume => Volume - VolumeSemula;
    }
}
