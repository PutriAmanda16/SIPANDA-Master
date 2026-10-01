using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sipanda.Models.Entities
{
    [Table("master_kategori")]
    public class MasterKategori
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Nama Kategori wajib diisi")]
        [StringLength(255)]
        [Column("nama_kategori")]
        [Display(Name = "Nama Kategori")]
        public string NamaKategori { get; set; } = string.Empty;

        [Column("parent_id")]
        [Display(Name = "Kategori Induk (Parent)")]
        public int? ParentId { get; set; }

        // Navigation
        [ForeignKey("ParentId")]
        public virtual MasterKategori? Parent { get; set; }

        public virtual ICollection<MasterKategori> SubKategoriList { get; set; } = new List<MasterKategori>();

        public virtual ICollection<MasterKomponen> KomponenList { get; set; } = new List<MasterKomponen>();

        public virtual ICollection<TarifRetribusi> TarifList { get; set; } = new List<TarifRetribusi>();
    }
}
