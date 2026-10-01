using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sipanda.Models.Entities
{
    [Table("budget_approval")]
    public class BudgetApproval
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
        [Column("tahap_id")]
        public int TahapId { get; set; }

        [ForeignKey("TahapId")]
        public MasterTahapBudgeting? Tahap { get; set; }

        [Required]
        [Column("approval_level")]
        public int ApprovalLevel { get; set; } = 1;

        [Required]
        [Column("approver_user_id")]
        public int ApproverUserId { get; set; }

        [ForeignKey("ApproverUserId")]
        public MasterUser? ApproverUser { get; set; }

        [Required]
        [Column("status")]
        [StringLength(30)]
        public string Status { get; set; } = "DISETUJUI"; // DISETUJUI, DIKEMBALIKAN, DITOLAK

        [Column("tanggal_approval")]
        public DateTime TanggalApproval { get; set; } = DateTime.Now;

        [Column("catatan")]
        public string? Catatan { get; set; }
    }
}
