using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sipanda.Models.Entities
{
    [Table("budget_pendapatan_detail")]
    public class BudgetPendapatanDetail
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
        [Column("rekening_id")]
        [Display(Name = "Kode Rekening Pendapatan")]
        public int RekeningId { get; set; }

        [ForeignKey("RekeningId")]
        public MasterRekening? Rekening { get; set; }

        [Required]
        [Column("uraian")]
        [Display(Name = "Uraian Objek Pendapatan")]
        public string Uraian { get; set; } = string.Empty;

        public ICollection<BudgetPendapatanVersiDetail> VersiDetailList { get; set; } = new List<BudgetPendapatanVersiDetail>();
    }
}
