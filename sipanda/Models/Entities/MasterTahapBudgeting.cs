using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sipanda.Models.Entities
{
    [Table("master_tahap_budgeting")]
    public class MasterTahapBudgeting
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("kode_tahap")]
        [StringLength(50)]
        [Display(Name = "Kode Tahap")]
        public string KodeTahap { get; set; } = string.Empty;

        [Required]
        [Column("nama_tahap")]
        [StringLength(150)]
        [Display(Name = "Nama Tahapan Pembahasan")]
        public string NamaTahap { get; set; } = string.Empty;

        [Required]
        [Column("urutan")]
        [Display(Name = "Urutan Workflow")]
        public int Urutan { get; set; }

        [Required]
        [Column("jenis_anggaran")]
        [StringLength(20)]
        [Display(Name = "Jenis Anggaran")]
        public string JenisAnggaran { get; set; } = "MURNI"; // MURNI, PERGESERAN, PERUBAHAN

        [Required]
        [Column("jenis_tahap")]
        [StringLength(50)]
        [Display(Name = "Jenis Aksi")]
        public string JenisTahap { get; set; } = "PEMBAHASAN"; // INPUT, PEMBAHASAN, PENYESUAIAN, PENYUSUNAN, PENETAPAN

        [Column("perlu_approval")]
        [Display(Name = "Perlu Approval")]
        public bool PerluApproval { get; set; } = true;

        [Column("is_aktif")]
        public bool IsAktif { get; set; } = true;

        public ICollection<BudgetPendapatan> BudgetPendapatanList { get; set; } = new List<BudgetPendapatan>();
        public ICollection<BudgetPendapatanVersi> VersiList { get; set; } = new List<BudgetPendapatanVersi>();
        public ICollection<MasterApprovalFlow> ApprovalFlowList { get; set; } = new List<MasterApprovalFlow>();
    }
}
