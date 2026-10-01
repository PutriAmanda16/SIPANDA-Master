using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sipanda.Models.Entities
{
    [Table("master_tahun_anggaran")]
    public class MasterTahunAnggaran
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tahun anggaran wajib diisi")]
        [Column("tahun")]
        [Display(Name = "Tahun Anggaran")]
        public int Tahun { get; set; }

        [Required]
        [Column("status")]
        [StringLength(20)]
        [Display(Name = "Status Anggaran")]
        public string Status { get; set; } = "aktif"; // aktif, tutup, persiapan

        public ICollection<BudgetPendapatan> BudgetPendapatanList { get; set; } = new List<BudgetPendapatan>();
    }
}
