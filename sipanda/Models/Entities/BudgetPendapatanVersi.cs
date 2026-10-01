using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sipanda.Models.Entities
{
    [Table("budget_pendapatan_versi")]
    public class BudgetPendapatanVersi
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("budget_id")]
        public int BudgetId { get; set; }

        [ForeignKey("BudgetId")]
        public BudgetPendapatan? Budget { get; set; }

        [Required]
        [Column("nomor_versi")]
        [StringLength(20)]
        [Display(Name = "Nomor Versi")]
        public string NomorVersi { get; set; } = "V1.0";

        [Required]
        [Column("tahap_id")]
        public int TahapId { get; set; }

        [ForeignKey("TahapId")]
        public MasterTahapBudgeting? Tahap { get; set; }

        [Column("versi_sebelumnya_id")]
        public int? VersiSebelumnyaId { get; set; }

        [ForeignKey("VersiSebelumnyaId")]
        public BudgetPendapatanVersi? VersiSebelumnya { get; set; }

        [Column("is_baseline")]
        [Display(Name = "Baseline Disetujui")]
        public bool IsBaseline { get; set; } = false;

        [Required]
        [Column("status_approval")]
        [StringLength(30)]
        [Display(Name = "Status Approval")]
        public string StatusApproval { get; set; } = "DRAFT"; // DRAFT, DIAJUKAN, DISETUJUI, DIKEMBALIKAN

        [Column("tanggal_versi")]
        public DateTime TanggalVersi { get; set; } = DateTime.Now;

        [Column("created_by")]
        public int? CreatedBy { get; set; }

        [ForeignKey("CreatedBy")]
        public MasterUser? Creator { get; set; }

        [Column("catatan_pembahasan")]
        [Display(Name = "Catatan Hasil Pembahasan")]
        public string? CatatanPembahasan { get; set; }

        public ICollection<BudgetPendapatanVersiDetail> VersiDetailList { get; set; } = new List<BudgetPendapatanVersiDetail>();
        public ICollection<BudgetApproval> ApprovalList { get; set; } = new List<BudgetApproval>();
    }
}
