using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sipanda.Models.Entities
{
    [Table("audit_log")]
    public class AuditLog
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("user_id")]
        public int? UserId { get; set; }

        [ForeignKey("UserId")]
        public MasterUser? User { get; set; }

        [Required]
        [Column("entity_type")]
        [StringLength(50)]
        public string EntityType { get; set; } = string.Empty;

        [Required]
        [Column("entity_id")]
        public long EntityId { get; set; }

        [Required]
        [Column("action")]
        [StringLength(50)]
        public string Action { get; set; } = string.Empty;

        [Column("before_data", TypeName = "jsonb")]
        public string? BeforeData { get; set; }

        [Column("after_data", TypeName = "jsonb")]
        public string? AfterData { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("ip_address")]
        [StringLength(45)]
        public string? IpAddress { get; set; }
    }
}
