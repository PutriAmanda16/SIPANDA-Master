using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sipanda.Models.Entities
{
    [Table("master_user")]
    public class MasterUser
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Username wajib diisi")]
        [StringLength(50)]
        [Column("username")]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nama Lengkap wajib diisi")]
        [StringLength(100)]
        [Column("nama_lengkap")]
        [Display(Name = "Nama Lengkap")]
        public string NamaLengkap { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Format email tidak valid")]
        [StringLength(100)]
        [Column("email")]
        [Display(Name = "Alamat Email")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Password wajib diisi")]
        [StringLength(255)]
        [Column("password_hash")]
        [Display(Name = "Password")]
        public string PasswordHash { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role wajib dipilih")]
        [Column("role_id")]
        [Display(Name = "Role / Hak Akses")]
        public int RoleId { get; set; }

        [Column("skpd_id")]
        [Display(Name = "Unit Kerja / SKPD Terkait (Opsional)")]
        public int? SkpdId { get; set; }

        [Column("faskes_id")]
        [Display(Name = "Fasilitas Kesehatan Terkait (Opsional)")]
        public int? FaskesId { get; set; }

        [Required]
        [StringLength(20)]
        [Column("status")]
        [Display(Name = "Status Pengguna")]
        public string Status { get; set; } = "aktif";

        [Column("created_at")]
        [Display(Name = "Waktu Dibuat")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey("RoleId")]
        public virtual MasterRole? Role { get; set; }

        [ForeignKey("SkpdId")]
        public virtual MasterSkpd? Skpd { get; set; }

        [ForeignKey("FaskesId")]
        public virtual MasterFaskes? Faskes { get; set; }
    }
}
