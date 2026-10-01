using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sipanda.Models.Entities
{
    [Table("master_role")]
    public class MasterRole
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Nama Role wajib diisi")]
        [StringLength(50)]
        [Column("nama_role")]
        [Display(Name = "Nama Role / Peran")]
        public string NamaRole { get; set; } = string.Empty;

        [StringLength(255)]
        [Column("deskripsi")]
        [Display(Name = "Deskripsi Role")]
        public string? Deskripsi { get; set; }

        [Required(ErrorMessage = "Hak Akses Menu wajib ditentukan")]
        [Column("allowed_menus")]
        [Display(Name = "Daftar Menu yang Diizinkan (Koma / Keys)")]
        public string AllowedMenus { get; set; } = string.Empty;

        // Navigation
        public virtual ICollection<MasterUser> UserList { get; set; } = new List<MasterUser>();

        [NotMapped]
        public List<string> MenuKeyList => (AllowedMenus ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(m => m.Trim().ToLower())
            .ToList();

        public bool HasMenu(string menuKey)
        {
            if (string.IsNullOrWhiteSpace(menuKey)) return false;
            return MenuKeyList.Contains(menuKey.Trim().ToLower()) || MenuKeyList.Contains("*");
        }
    }
}
