using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sipanda.Models.Entities
{
    [Table("master_faskes")]
    public class MasterFaskes
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Kode Faskes wajib diisi")]
        [StringLength(20)]
        [Column("kode_faskes")]
        [Display(Name = "Kode Faskes")]
        public string KodeFaskes { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nama Faskes wajib diisi")]
        [StringLength(255)]
        [Column("nama_faskes")]
        [Display(Name = "Nama Fasilitas Kesehatan")]
        public string NamaFaskes { get; set; } = string.Empty;

        [Required(ErrorMessage = "Rekening Pendapatan wajib dipilih")]
        [Column("rekening_id")]
        [Display(Name = "Rekening Pendapatan")]
        public int RekeningId { get; set; }

        // Navigation
        [ForeignKey("RekeningId")]
        public virtual MasterRekening? Rekening { get; set; }

        public virtual ICollection<TarifRetribusi> TarifList { get; set; } = new List<TarifRetribusi>();
    }
}
