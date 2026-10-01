using System.Security.Claims;

namespace Sipanda.Helpers
{
    public static class AppMenus
    {
        public const string Dashboard = "dashboard";
        public const string BudgetPendapatan = "budget_pendapatan";
        public const string TarifRetribusi = "tarif_retribusi";
        public const string MasterTahap = "master_tahap";
        public const string MasterPeraturan = "master_peraturan";
        public const string MasterSkpd = "master_skpd";
        public const string MasterFaskes = "master_faskes";
        public const string MasterRekening = "master_rekening";
        public const string MasterKategori = "master_kategori";
        public const string MasterKomponen = "master_komponen";
        public const string UserManagement = "user_management";
        public const string AuditTrail = "audit_trail";

        public static readonly Dictionary<string, string> AllMenus = new()
        {
            { Dashboard, "Dashboard Utama" },
            { BudgetPendapatan, "Penganggaran Pendapatan (Budgeting)" },
            { TarifRetribusi, "Tabel Retribusi Perda 7" },
            { MasterTahap, "Master Tahapan & Approval Workflow" },
            { MasterPeraturan, "Master Peraturan & Regulasi" },
            { MasterSkpd, "Master SKPD / OPD" },
            { MasterFaskes, "Master Faskes" },
            { MasterRekening, "Master Rekening" },
            { MasterKategori, "Master Kategori" },
            { MasterKomponen, "Master Layanan/Komponen" },
            { UserManagement, "Manajemen User & Role" },
            { AuditTrail, "Audit Trail & Log Perubahan" }
        };
    }

    public static class MenuSecurityExtensions
    {
        public static bool HasMenuAccess(this ClaimsPrincipal user, string menuKey)
        {
            if (user?.Identity?.IsAuthenticated != true)
            {
                // Guest hanya bisa akses dashboard
                return menuKey.Equals(AppMenus.Dashboard, StringComparison.OrdinalIgnoreCase);
            }

            var allowedMenusClaim = user.FindFirst("AllowedMenus")?.Value;
            if (string.IsNullOrWhiteSpace(allowedMenusClaim))
            {
                // Fallback jika SuperAdmin
                if (user.IsInRole("SuperAdmin")) return true;
                return false;
            }

            var allowedList = allowedMenusClaim
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim().ToLower())
                .ToList();

            return allowedList.Contains("*") || allowedList.Contains(menuKey.Trim().ToLower());
        }
    }
}
