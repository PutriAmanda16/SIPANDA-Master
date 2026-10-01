using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sipanda.Models.Entities
{
    [Table("master_rekening")]
    public class MasterRekening
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Kode Rekening wajib diisi")]
        [StringLength(50)]
        [Column("kode_rekening")]
        [Display(Name = "Kode Rekening")]
        public string KodeRekening { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nama Rekening wajib diisi")]
        [StringLength(255)]
        [Column("nama_rekening")]
        [Display(Name = "Nama / Uraian Rekening")]
        public string NamaRekening { get; set; } = string.Empty;

        // Navigation
        public virtual ICollection<MasterFaskes> FaskesList { get; set; } = new List<MasterFaskes>();
    }
}
