using EmployeeLoanApp.Models;
using EmployeeLoanApp.Models.PMS;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLoanApp.Data
{
    public class EmployeeLoanContext : DbContext
    {
        public EmployeeLoanContext(DbContextOptions<EmployeeLoanContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; } // NEW

        public DbSet<Employee> Employees { get; set; }
        public DbSet<LoanApplication> LoanApplications { get; set; }
        public DbSet<LoanApproval> LoanApprovals { get; set; }
        // NEW MASTER TABLES
        public DbSet<ApplicationType> ApplicationTypes { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<LoanPurpose> LoanPurposes { get; set; }
        public DbSet<LoanRepayment> LoanRepayments { get; set; }
        public DbSet<LoanAuditLog> LoanAuditLogs { get; set; }

        // --- NEW PMS TABLES (pms schema) ---
        public DbSet<PmsUser> PmsUsers { get; set; }
        public DbSet<Property> Properties { get; set; }
        public DbSet<PreviousOwnerDetails> PreviousOwners { get; set; }
        public DbSet<PropertyFinancials> PropertyFinancials { get; set; }
        public DbSet<ComplianceDocument> ComplianceDocuments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 1. Configure Relationships
            modelBuilder.Entity<LoanApplication>()
                .HasOne(l => l.Employee)
                .WithMany()
                .HasForeignKey(l => l.EmployeeID);

            modelBuilder.Entity<LoanApproval>()
                .HasOne(l => l.Application)
                .WithMany()
                .HasForeignKey(l => l.ApplicationID);

            // 2. FIX: Configure Decimal Precision to match SQL "DECIMAL(18, 2)"
            // This removes the warnings and prevents data truncation errors.

            // For LoanApplication Table
            modelBuilder.Entity<LoanApplication>()
                .Property(p => p.LoanAmountRequested)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<LoanApplication>()
                .Property(p => p.ProposedEMIAmount)
                .HasColumnType("decimal(18,2)");

            // For LoanApproval Table
            modelBuilder.Entity<LoanApproval>()
                .Property(p => p.SanctionedAmount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<LoanApproval>()
                .Property(p => p.SanctionedEMIAmount)
                .HasColumnType("decimal(18,2)");
            // Decimal precision config
            modelBuilder.Entity<LoanApplication>().Property(p => p.LoanAmountRequested).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<LoanApplication>().Property(p => p.ProposedEMIAmount).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<LoanApproval>().Property(p => p.SanctionedAmount).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<LoanApproval>().Property(p => p.SanctionedEMIAmount).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Employee>().Property(p => p.OpeningLoanBalance).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<LoanRepayment>().Property(p => p.EMIAmount).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<LoanRepayment>().Property(p => p.PaymentAmount).HasColumnType("decimal(18,2)");
            // --- PMS CONFIGURATIONS (New) ---

            // 1. Configure PMS User unique username
            modelBuilder.Entity<PmsUser>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // 2. Cross-Schema Relationship: Property -> Company (Purchaser)
            // We assume 'Company' is in the default dbo schema.
            modelBuilder.Entity<Property>()
                .HasOne<Company>() // Relationship to existing Company entity
                .WithMany()        // Company has many Properties (implicitly)
                .HasForeignKey(p => p.PurchaserId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent deleting a company if it owns properties

            // 3. Cross-Schema Relationship: Property -> Company (Used By)
            modelBuilder.Entity<Property>()
                .HasOne<Company>()
                .WithMany()
                .HasForeignKey(p => p.UsedByCompanyId)
                .IsRequired(false) // Nullable
                .OnDelete(DeleteBehavior.Restrict);

            // 4. Cross-Schema Relationship: Financials -> Company (Loan Company)
            modelBuilder.Entity<PropertyFinancials>()
                .HasOne<Company>()
                .WithMany()
                .HasForeignKey(f => f.LoanCompanyId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // 5. Configure 1-to-1 Relationships (Shared Primary Key)
            modelBuilder.Entity<Property>()
                .HasOne(p => p.PreviousOwner)
                .WithOne(po => po.Property)
                .HasForeignKey<PreviousOwnerDetails>(po => po.PropertyId);

            modelBuilder.Entity<Property>()
                .HasOne(p => p.Financials)
                .WithOne(pf => pf.Property)
                .HasForeignKey<PropertyFinancials>(pf => pf.PropertyId);
        }
    }
}