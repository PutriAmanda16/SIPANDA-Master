using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sipanda.Models.Entities
{
    [Table("master_skpd")]
    public class MasterSkpd
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Kode SKPD / OPD wajib diisi")]
        [StringLength(50)]
        [Column("kode_skpd")]
        [Display(Name = "Kode SKPD / OPD")]
        public string KodeSkpd { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nama SKPD / OPD wajib diisi")]
        [StringLength(255)]
        [Column("nama_skpd")]
        [Display(Name = "Nama SKPD / OPD / Unit Kerja")]
        public string NamaSkpd { get; set; } = string.Empty;

        [Column("parent_id")]
        [Display(Name = "OPD Induk (Parent)")]
        public int? ParentId { get; set; }

        // Navigation
        [ForeignKey("ParentId")]
        public virtual MasterSkpd? Parent { get; set; }

        public virtual ICollection<MasterSkpd> SubSkpdList { get; set; } = new List<MasterSkpd>();
    }
}
