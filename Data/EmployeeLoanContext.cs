using Microsoft.EntityFrameworkCore;
using EmployeeLoanApp.Models;

namespace EmployeeLoanApp.Data
{
    public class EmployeeLoanContext : DbContext
    {
        public EmployeeLoanContext(DbContextOptions<EmployeeLoanContext> options)
            : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<LoanApplication> LoanApplications { get; set; }
        public DbSet<LoanApproval> LoanApprovals { get; set; }
        // NEW MASTER TABLES
        public DbSet<ApplicationType> ApplicationTypes { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<LoanPurpose> LoanPurposes { get; set; }

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
        }
    }
}