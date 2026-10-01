using Microsoft.EntityFrameworkCore;
using Sipanda.Models.Entities;

namespace Sipanda.Data
{
    public class SipandaDbContext : DbContext
    {
        public SipandaDbContext(DbContextOptions<SipandaDbContext> options) : base(options)
        {
        }

        public DbSet<MasterRekening> MasterRekening { get; set; } = null!;
        public DbSet<MasterFaskes> MasterFaskes { get; set; } = null!;
        public DbSet<MasterPeraturan> MasterPeraturan { get; set; } = null!;
        public DbSet<MasterKategori> MasterKategori { get; set; } = null!;
        public DbSet<MasterKomponen> MasterKomponen { get; set; } = null!;
        public DbSet<MasterSkpd> MasterSkpd { get; set; } = null!;
        public DbSet<MasterRole> MasterRoles { get; set; } = null!;
        public DbSet<MasterUser> MasterUsers { get; set; } = null!;
        public DbSet<TarifRetribusi> TarifRetribusi { get; set; } = null!;
        public DbSet<VTarifLengkap> VTarifLengkap { get; set; } = null!;

        // Modul Penganggaran Pendapatan (Budgeting)
        public DbSet<MasterTahunAnggaran> MasterTahunAnggaran { get; set; } = null!;
        public DbSet<MasterTahapBudgeting> MasterTahapBudgeting { get; set; } = null!;
        public DbSet<MasterApprovalFlow> MasterApprovalFlow { get; set; } = null!;
        public DbSet<BudgetPendapatan> BudgetPendapatan { get; set; } = null!;
        public DbSet<BudgetPendapatanDetail> BudgetPendapatanDetail { get; set; } = null!;
        public DbSet<BudgetPendapatanVersi> BudgetPendapatanVersi { get; set; } = null!;
        public DbSet<BudgetPendapatanVersiDetail> BudgetPendapatanVersiDetail { get; set; } = null!;
        public DbSet<BudgetApproval> BudgetApproval { get; set; } = null!;
        public DbSet<AuditLog> AuditLog { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Master Role
            modelBuilder.Entity<MasterRole>(entity =>
            {
                entity.ToTable("master_role");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.NamaRole).IsUnique();
            });

            // Master User
            modelBuilder.Entity<MasterUser>(entity =>
            {
                entity.ToTable("master_user");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Username).IsUnique();

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.UserList)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.Skpd)
                    .WithMany()
                    .HasForeignKey(d => d.SkpdId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(d => d.Faskes)
                    .WithMany()
                    .HasForeignKey(d => d.FaskesId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Master SKPD (Hierarki Induk - Anak)
            modelBuilder.Entity<MasterSkpd>(entity =>
            {
                entity.ToTable("master_skpd");
                entity.HasKey(e => e.Id);

                entity.HasOne(d => d.Parent)
                    .WithMany(p => p.SubSkpdList)
                    .HasForeignKey(d => d.ParentId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Master Rekening
            modelBuilder.Entity<MasterRekening>(entity =>
            {
                entity.ToTable("master_rekening");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.KodeRekening).IsUnique();
            });

            // Master Faskes
            modelBuilder.Entity<MasterFaskes>(entity =>
            {
                entity.ToTable("master_faskes");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.KodeFaskes).IsUnique();

                entity.HasOne(d => d.Rekening)
                    .WithMany(p => p.FaskesList)
                    .HasForeignKey(d => d.RekeningId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Master Peraturan
            modelBuilder.Entity<MasterPeraturan>(entity =>
            {
                entity.ToTable("master_peraturan");
                entity.HasKey(e => e.Id);
            });

            // Master Kategori (Parent - Child)
            modelBuilder.Entity<MasterKategori>(entity =>
            {
                entity.ToTable("master_kategori");
                entity.HasKey(e => e.Id);

                entity.HasOne(d => d.Parent)
                    .WithMany(p => p.SubKategoriList)
                    .HasForeignKey(d => d.ParentId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Master Komponen
            modelBuilder.Entity<MasterKomponen>(entity =>
            {
                entity.ToTable("master_komponen");
                entity.HasKey(e => e.Id);

                entity.HasOne(d => d.Kategori)
                    .WithMany(p => p.KomponenList)
                    .HasForeignKey(d => d.KategoriId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Tarif Retribusi
            modelBuilder.Entity<TarifRetribusi>(entity =>
            {
                entity.ToTable("tarif_retribusi");
                entity.HasKey(e => e.Id);

                entity.HasOne(d => d.Peraturan)
                    .WithMany(p => p.TarifList)
                    .HasForeignKey(d => d.PeraturanId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Faskes)
                    .WithMany(p => p.TarifList)
                    .HasForeignKey(d => d.FaskesId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Kategori)
                    .WithMany(p => p.TarifList)
                    .HasForeignKey(d => d.KategoriId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Komponen)
                    .WithMany(p => p.TarifList)
                    .HasForeignKey(d => d.KomponenId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // View v_tarif_lengkap
            modelBuilder.Entity<VTarifLengkap>(entity =>
            {
                entity.ToView("v_tarif_lengkap");
                entity.HasKey(e => e.TarifId);
            });

            // Budget Pendapatan
            modelBuilder.Entity<BudgetPendapatan>(entity =>
            {
                entity.ToTable("budget_pendapatan");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.NomorUsulan).IsUnique();

                entity.HasOne(d => d.TahunAnggaran)
                    .WithMany(p => p.BudgetPendapatanList)
                    .HasForeignKey(d => d.TahunAnggaranId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.Skpd)
                    .WithMany()
                    .HasForeignKey(d => d.SkpdId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.CurrentTahap)
                    .WithMany(p => p.BudgetPendapatanList)
                    .HasForeignKey(d => d.CurrentTahapId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.DpaInduk)
                    .WithMany()
                    .HasForeignKey(d => d.DpaIndukId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(d => d.Creator)
                    .WithMany()
                    .HasForeignKey(d => d.CreatedBy)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Budget Pendapatan Detail
            modelBuilder.Entity<BudgetPendapatanDetail>(entity =>
            {
                entity.ToTable("budget_pendapatan_detail");
                entity.HasKey(e => e.Id);

                entity.HasOne(d => d.Budget)
                    .WithMany(p => p.DetailList)
                    .HasForeignKey(d => d.BudgetId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Rekening)
                    .WithMany()
                    .HasForeignKey(d => d.RekeningId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Budget Pendapatan Versi
            modelBuilder.Entity<BudgetPendapatanVersi>(entity =>
            {
                entity.ToTable("budget_pendapatan_versi");
                entity.HasKey(e => e.Id);

                entity.HasOne(d => d.Budget)
                    .WithMany(p => p.VersiList)
                    .HasForeignKey(d => d.BudgetId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Tahap)
                    .WithMany(p => p.VersiList)
                    .HasForeignKey(d => d.TahapId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.VersiSebelumnya)
                    .WithMany()
                    .HasForeignKey(d => d.VersiSebelumnyaId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(d => d.Creator)
                    .WithMany()
                    .HasForeignKey(d => d.CreatedBy)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Budget Pendapatan Versi Detail
            modelBuilder.Entity<BudgetPendapatanVersiDetail>(entity =>
            {
                entity.ToTable("budget_pendapatan_versi_detail");
                entity.HasKey(e => e.Id);

                entity.HasOne(d => d.Versi)
                    .WithMany(p => p.VersiDetailList)
                    .HasForeignKey(d => d.VersiId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Detail)
                    .WithMany(p => p.VersiDetailList)
                    .HasForeignKey(d => d.DetailId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.TarifRetribusi)
                    .WithMany()
                    .HasForeignKey(d => d.TarifRetribusiId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Budget Approval
            modelBuilder.Entity<BudgetApproval>(entity =>
            {
                entity.ToTable("budget_approval");
                entity.HasKey(e => e.Id);

                entity.HasOne(d => d.Versi)
                    .WithMany(p => p.ApprovalList)
                    .HasForeignKey(d => d.VersiId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Tahap)
                    .WithMany()
                    .HasForeignKey(d => d.TahapId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.ApproverUser)
                    .WithMany()
                    .HasForeignKey(d => d.ApproverUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Master Tahap Budgeting
            modelBuilder.Entity<MasterTahapBudgeting>(entity =>
            {
                entity.ToTable("master_tahap_budgeting");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.KodeTahap).IsUnique();
            });

            // Master Approval Flow
            modelBuilder.Entity<MasterApprovalFlow>(entity =>
            {
                entity.ToTable("master_approval_flow");
                entity.HasKey(e => e.Id);

                entity.HasOne(d => d.Tahap)
                    .WithMany(p => p.ApprovalFlowList)
                    .HasForeignKey(d => d.TahapId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Role)
                    .WithMany()
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Master Tahun Anggaran
            modelBuilder.Entity<MasterTahunAnggaran>(entity =>
            {
                entity.ToTable("master_tahun_anggaran");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Tahun).IsUnique();
            });

            // Audit Log
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.ToTable("audit_log");
                entity.HasKey(e => e.Id);

                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}
