using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sipanda.Models.Entities
{
    [Table("master_komponen")]
    public class MasterKomponen
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Kategori wajib dipilih")]
        [Column("kategori_id")]
        [Display(Name = "Kategori Layanan")]
        public int KategoriId { get; set; }

        [Required(ErrorMessage = "Nama Layanan / Komponen wajib diisi")]
        [Column("nama_komponen")]
        [Display(Name = "Nama Layanan / Tindakan")]
        public string NamaKomponen { get; set; } = string.Empty;

        [StringLength(100)]
        [Column("satuan")]
        [Display(Name = "Satuan (misal: Per Layanan, Per Hari, Per Sampel)")]
        public string? Satuan { get; set; }

        // Navigation
        [ForeignKey("KategoriId")]
        public virtual MasterKategori? Kategori { get; set; }

        public virtual ICollection<TarifRetribusi> TarifList { get; set; } = new List<TarifRetribusi>();
    }
}
