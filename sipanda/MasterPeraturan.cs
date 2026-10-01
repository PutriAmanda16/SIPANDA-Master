using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sipanda.Models.Entities
{
    [Table("master_peraturan")]
    public class MasterPeraturan
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Jenis Peraturan wajib diisi")]
        [StringLength(50)]
        [Column("jenis_peraturan")]
        [Display(Name = "Jenis Peraturan (contoh: Perda / Perwali / SK)")]
        public string JenisPeraturan { get; set; } = "Perda";

        [Required(ErrorMessage = "Nomor Peraturan wajib diisi")]
        [StringLength(20)]
        [Column("nomor")]
        [Display(Name = "Nomor Peraturan")]
        public string Nomor { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tahun Peraturan wajib diisi")]
        [Column("tahun")]
        [Display(Name = "Tahun")]
        public short Tahun { get; set; }

        [Required(ErrorMessage = "Tentang / Judul Peraturan wajib diisi")]
        [Column("tentang")]
        [Display(Name = "Tentang")]
        public string Tentang { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tanggal Berlaku wajib diisi")]
        [Column("tanggal_berlaku")]
        [DataType(DataType.Date)]
        [Display(Name = "Tanggal Berlaku")]
        public DateOnly TanggalBerlaku { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        [Required(ErrorMessage = "Status wajib diisi")]
        [StringLength(20)]
        [Column("status")]
        [Display(Name = "Status")]
        public string Status { get; set; } = "aktif";

        [NotMapped]
        public string JudulLengkap => $"{JenisPeraturan} No. {Nomor} Th. {Tahun}";

        // Navigation
        public virtual ICollection<TarifRetribusi> TarifList { get; set; } = new List<TarifRetribusi>();
    }
}
