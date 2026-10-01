using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sipanda.Models.Entities
{
    [Table("budget_pendapatan")]
    public class BudgetPendapatan
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("tahun_anggaran_id")]
        [Display(Name = "Tahun Anggaran")]
        public int TahunAnggaranId { get; set; }

        [ForeignKey("TahunAnggaranId")]
        public MasterTahunAnggaran? TahunAnggaran { get; set; }

        [Required]
        [Column("skpd_id")]
        [Display(Name = "SKPD / Perangkat Daerah")]
        public int SkpdId { get; set; }

        [ForeignKey("SkpdId")]
        public MasterSkpd? Skpd { get; set; }

        [Required(ErrorMessage = "Nomor usulan wajib diisi")]
        [Column("nomor_usulan")]
        [StringLength(100)]
        [Display(Name = "Nomor Dokumen Usulan")]
        public string NomorUsulan { get; set; } = string.Empty;

        [Required]
        [Column("tanggal_usulan")]
        [DataType(DataType.Date)]
        [Display(Name = "Tanggal Usulan")]
        public DateTime TanggalUsulan { get; set; } = DateTime.Today;

        [Required]
        [Column("jenis_anggaran")]
        [StringLength(20)]
        [Display(Name = "Jenis Anggaran")]
        public string JenisAnggaran { get; set; } = "MURNI"; // MURNI, PERGESERAN, PERUBAHAN

        [Column("dpa_induk_id")]
        [Display(Name = "Referensi DPA Induk (Murni)")]
        public int? DpaIndukId { get; set; }

        [ForeignKey("DpaIndukId")]
        public BudgetPendapatan? DpaInduk { get; set; }

        [Required]
        [Column("current_tahap_id")]
        [Display(Name = "Tahapan Aktif Saat Ini")]
        public int CurrentTahapId { get; set; }

        [ForeignKey("CurrentTahapId")]
        public MasterTahapBudgeting? CurrentTahap { get; set; }

        [Required]
        [Column("status_dokumen")]
        [StringLength(30)]
        [Display(Name = "Status Dokumen")]
        public string StatusDokumen { get; set; } = "DRAFT"; // DRAFT, DIAJUKAN, DISETUJUI, DIKEMBALIKAN, FINAL

        [Column("program")]
        [StringLength(255)]
        [Display(Name = "Program")]
        public string? Program { get; set; } = "PROGRAM PENGELOLAAN PENDAPATAN DAERAH";

        [Column("kegiatan")]
        [StringLength(255)]
        [Display(Name = "Kegiatan")]
        public string? Kegiatan { get; set; } = "Kegiatan Pengelolaan Pendapatan Daerah";

        [Column("sub_kegiatan")]
        [StringLength(255)]
        [Display(Name = "Sub Kegiatan")]
        public string? SubKegiatan { get; set; } = "Pembinaan dan Pengawasan Pengelolaan Retribusi Daerah";

        [Column("sumber_pendanaan")]
        [StringLength(100)]
        [Display(Name = "Sumber Pendanaan")]
        public string? SumberPendanaan { get; set; } = "Pendapatan Asli Daerah (PAD)";

        [Column("lokasi")]
        [StringLength(255)]
        [Display(Name = "Lokasi")]
        public string? Lokasi { get; set; }

        [Column("keterangan")]
        [Display(Name = "Keterangan")]
        public string? Keterangan { get; set; }

        [Column("created_by")]
        public int? CreatedBy { get; set; }

        [ForeignKey("CreatedBy")]
        public MasterUser? Creator { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public ICollection<BudgetPendapatanDetail> DetailList { get; set; } = new List<BudgetPendapatanDetail>();
        public ICollection<BudgetPendapatanVersi> VersiList { get; set; } = new List<BudgetPendapatanVersi>();
    }
}
