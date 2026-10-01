using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sipanda.Models.Entities
{
    [Table("master_approval_flow")]
    public class MasterApprovalFlow
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("tahap_id")]
        public int TahapId { get; set; }

        [ForeignKey("TahapId")]
        public MasterTahapBudgeting? Tahap { get; set; }

        [Required]
        [Column("urutan")]
        [Display(Name = "Tingkat Hirarki Approval")]
        public int Urutan { get; set; }

        [Required]
        [Column("role_id")]
        public int RoleId { get; set; }

        [ForeignKey("RoleId")]
        public MasterRole? Role { get; set; }

        [Column("is_wajib")]
        public bool IsWajib { get; set; } = true;
    }
}
