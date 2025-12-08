using EmployeeLoanApp.Data;
using EmployeeLoanApp.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLoanApp.Services
{
    public class LoanService
    {
        private readonly IDbContextFactory<EmployeeLoanContext> _factory;
        private readonly DigiGoService _digiGoService; // Switched to DigiGo

        public LoanService(IDbContextFactory<EmployeeLoanContext> factory, DigiGoService digiGoService)
        {
            _factory = factory;
            _digiGoService = digiGoService;
        }

        // --- MASTER DATA METHODS --- (Kept as is)
        public async Task<List<Company>> GetCompaniesAsync()
        {
            using var context = await _factory.CreateDbContextAsync();
            return await context.Companies.Where(x => x.IsActive).ToListAsync();
        }
        public async Task AddCompanyAsync(string name)
        {
            using var context = await _factory.CreateDbContextAsync();
            if (!await context.Companies.AnyAsync(c => c.CompanyName == name))
            {
                context.Companies.Add(new Company { CompanyName = name, IsActive = true });
                await context.SaveChangesAsync();
            }
        }
        public async Task DeleteCompanyAsync(int id)
        {
            using var context = await _factory.CreateDbContextAsync();
            var item = await context.Companies.FindAsync(id);
            if (item != null) { context.Companies.Remove(item); await context.SaveChangesAsync(); }
        }

        public async Task<List<LoanPurpose>> GetLoanPurposesAsync()
        {
            using var context = await _factory.CreateDbContextAsync();
            return await context.LoanPurposes.Where(x => x.IsActive).ToListAsync();
        }
        public async Task AddLoanPurposeAsync(string name)
        {
            using var context = await _factory.CreateDbContextAsync();
            if (!await context.LoanPurposes.AnyAsync(p => p.PurposeName == name))
            {
                context.LoanPurposes.Add(new LoanPurpose { PurposeName = name, IsActive = true });
                await context.SaveChangesAsync();
            }
        }
        public async Task DeleteLoanPurposeAsync(int id)
        {
            using var context = await _factory.CreateDbContextAsync();
            var item = await context.LoanPurposes.FindAsync(id);
            if (item != null) { context.LoanPurposes.Remove(item); await context.SaveChangesAsync(); }
        }

        public async Task<List<ApplicationType>> GetApplicationTypesAsync()
        {
            using var context = await _factory.CreateDbContextAsync();
            return await context.ApplicationTypes.Where(x => x.IsActive).ToListAsync();
        }
        public async Task AddApplicationTypeAsync(string name)
        {
            using var context = await _factory.CreateDbContextAsync();
            if (!await context.ApplicationTypes.AnyAsync(t => t.TypeName == name))
            {
                context.ApplicationTypes.Add(new ApplicationType { TypeName = name, IsActive = true });
                await context.SaveChangesAsync();
            }
        }
        public async Task DeleteApplicationTypeAsync(int id)
        {
            using var context = await _factory.CreateDbContextAsync();
            var item = await context.ApplicationTypes.FindAsync(id);
            if (item != null) { context.ApplicationTypes.Remove(item); await context.SaveChangesAsync(); }
        }
        // --- NEW: MISSING METHOD FOR MY LOANS PAGE ---
        public async Task<List<LoanApplication>> GetMyLoansAsync()
        {
            using var context = await _factory.CreateDbContextAsync();
            // In a real app, filter by logged-in user. For demo, return all.
            return await context.LoanApplications
                .Include(l => l.Employee)
                .OrderByDescending(l => l.SubmissionDate)
                .ToListAsync();
        }

        // --- LOAN & EMPLOYEE METHODS ---
        public async Task<List<Employee>> GetAllEmployeesAsync()
        {
            using var context = await _factory.CreateDbContextAsync();
            return await context.Employees.ToListAsync();
        }

        public async Task<Employee?> GetEmployeeWithLoansAsync(int employeeId)
        {
            using var context = await _factory.CreateDbContextAsync();
            return await context.Employees.FindAsync(employeeId);
        }

        public async Task<List<LoanApplication>> GetLoansByEmployeeIdAsync(int employeeId)
        {
            using var context = await _factory.CreateDbContextAsync();
            return await context.LoanApplications
                .Where(l => l.EmployeeID == employeeId)
                .OrderByDescending(l => l.SubmissionDate)
                .ToListAsync();
        }

        public async Task<LoanApplication?> GetActiveOrPendingLoanAsync(int employeeId)
        {
            using var context = await _factory.CreateDbContextAsync();
            return await context.LoanApplications
                .Where(l => l.EmployeeID == employeeId &&
                           (l.ApplicationStatus == "Pending Approval" ||
                            l.ApplicationStatus == "Pending Agreement" ||
                            l.ApplicationStatus == "Pending Payment" ||
                            l.ApplicationStatus == "Active"))
                .OrderByDescending(l => l.SubmissionDate)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> CreateEmployeeAsync(Employee emp)
        {
            using var context = await _factory.CreateDbContextAsync();
            if (await context.Employees.AnyAsync(e => e.EmployeeCode == emp.EmployeeCode)) return false;
            context.Employees.Add(emp);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<int> ImportEmployeesAsync(List<Employee> employees)
        {
            using var context = await _factory.CreateDbContextAsync();
            int count = 0;
            foreach (var emp in employees)
            {
                if (!await context.Employees.AnyAsync(e => e.EmployeeCode == emp.EmployeeCode))
                {
                    context.Employees.Add(emp);
                    count++;
                }
            }
            await context.SaveChangesAsync();
            return count;
        }

        public async Task<bool> SubmitApplicationAsync(LoanApplication app)
        {
            using var context = await _factory.CreateDbContextAsync();
            if (app.LoanAmountRequested <= 0) return false;
            if (app.Employee != null && app.Employee.EmployeeID > 0)
            {
                context.Entry(app.Employee).State = EntityState.Unchanged;
            }
            app.ApplicationStatus = "Pending Approval";
            context.LoanApplications.Add(app);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<List<LoanApplication>> GetPendingApplicationsAsync()
        {
            using var context = await _factory.CreateDbContextAsync();
            return await context.LoanApplications
                .Include(a => a.Employee)
                .Where(a => a.ApplicationStatus != "Rejected" && a.ApplicationStatus != "Active")
                .AsNoTracking()
                .ToListAsync();
        }

        // STEP 1: HR Sanctions -> Trigger DigiGo -> Status: Pending Agreement
        public async Task ApproveLoanAsync(LoanApproval approval)
        {
            using var context = await _factory.CreateDbContextAsync();
            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                // 1. Save Approval
                context.LoanApprovals.Add(approval);

                // 2. Update Status
                var application = await context.LoanApplications
                    .Include(a => a.Employee)
                    .FirstOrDefaultAsync(a => a.ApplicationID == approval.ApplicationID);

                if (application != null)
                {
                    application.ApplicationStatus = "Pending Agreement";

                    // 3. Send to DigiGo for Signature (API Call)
                    if (application.Employee != null)
                    {
                        // This sends the prefilled agreement to the user's email/phone via DigiGo
                        // The Webhook (WebhookController) will listen for the 'signed' event
                        var requestId = await _digiGoService.SendAgreementForSignatureAsync(application.Employee, application, approval);
                    }
                }

                await context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // STEP 2: Webhook calls this when DigiGo confirms signing
        public async Task ConfirmAgreementSignedAsync(int applicationId)
        {
            using var context = await _factory.CreateDbContextAsync();
            var app = await context.LoanApplications.FindAsync(applicationId);
            if (app != null)
            {
                app.ApplicationStatus = "Pending Payment"; // Auto-update status
                await context.SaveChangesAsync();
            }
        }

        public async Task DisburseLoanAsync(int applicationId)
        {
            using var context = await _factory.CreateDbContextAsync();
            var app = await context.LoanApplications.FindAsync(applicationId);
            if (app != null)
            {
                app.ApplicationStatus = "Active";
                await context.SaveChangesAsync();
            }
        }

        // Report 1: Full Statement (Payment & Repayment)
        public async Task<List<LoanRepayment>> GetLoanStatementAsync(int applicationId)
        {
            using var context = await _factory.CreateDbContextAsync();
            return await context.LoanRepayments
                .Where(r => r.ApplicationID == applicationId)
                .OrderBy(r => r.EMIDueDate)
                .ToListAsync();
        }
        public async Task UpdateLoanAsync(LoanApplication loan, string user)
        {
            using var context = await _factory.CreateDbContextAsync();
            var existing = await context.LoanApplications.FindAsync(loan.ApplicationID);

            if (existing != null)
            {
                // Log changes (Simplified for brevity, ideally check each field)
                await LogEditAsync(loan.ApplicationID, user, "LoanDetails", "Old", "New", "HR Update");

                // Update Fields
                existing.LoanAmountRequested = loan.LoanAmountRequested;
                existing.LoanTenureMonths = loan.LoanTenureMonths;
                existing.ProposedEMIAmount = loan.ProposedEMIAmount;
                existing.EMIStartDate = loan.EMIStartDate;
                //existing.AdminComments = loan.EmployeeRemarks; // Or map to a specific admin field if different
                                                               // Update other editable fields as needed

                await context.SaveChangesAsync();
            }
        }

        // Report 2: Total Loans of Employee
        public async Task<List<LoanApplication>> GetEmployeeLoanHistoryAsync(int employeeId)
        {
            using var context = await _factory.CreateDbContextAsync();
            return await context.LoanApplications
                .Include(l => l.Repayments) // Include repayment status
                .Where(l => l.EmployeeID == employeeId)
                .OrderByDescending(l => l.SubmissionDate)
                .ToListAsync();
        }

        // Report 3: Company-wise Summary
        public async Task<List<CompanyLoanSummary>> GetCompanyWiseSummaryAsync()
        {
            using var context = await _factory.CreateDbContextAsync();

            // This requires grouping by Employee.Company
            // We fetch raw data first to avoid complex LINQ translation issues
            var loans = await context.LoanApplications
                .Include(l => l.Employee)
                .Where(l => l.ApplicationStatus == "Active" || l.ApplicationStatus == "Closed")
                .ToListAsync();

            return loans
                .GroupBy(l => l.Employee?.Company ?? "Unknown")
                .Select(g => new CompanyLoanSummary
                {
                    CompanyName = g.Key,
                    TotalLoans = g.Count(),
                    TotalAmountDisbursed = g.Sum(l => l.LoanAmountRequested),
                    // Pending logic: Sum of unpaid repayments
                    // (Simplified for MVP: Total - Paid)
                    TotalAmountPending = g.Sum(l => l.LoanAmountRequested) // Placeholder logic
                })
                .ToList();
        }

        // --- REPAYMENT LOGIC ---

        // Call this when Loan becomes "Active" (Disbursed)
        public async Task GenerateEMIScheduleAsync(int applicationId, DateTime startDate)
        {
            using var context = await _factory.CreateDbContextAsync();
            var loan = await context.LoanApplications.FindAsync(applicationId);
            if (loan == null) return;

            // Save Start Date
            loan.EMIStartDate = startDate;

            // Calculate Monthly EMI (Principal / Tenure for simplicity, or use PMT formula if interest exists)
            // Assuming user input "ProposedEMIAmount" is the final agreed EMI
            decimal emiAmount = loan.ProposedEMIAmount ?? (loan.LoanAmountRequested / loan.LoanTenureMonths);

            // Generate Schedule
            for (int i = 0; i < loan.LoanTenureMonths; i++)
            {
                context.LoanRepayments.Add(new LoanRepayment
                {
                    ApplicationID = applicationId,
                    EMIDueDate = startDate.AddMonths(i),
                    EMIAmount = emiAmount,
                    Status = "Pending"
                });
            }
            await context.SaveChangesAsync();
        }

        // Mark EMI as Paid (HR Action)
        public async Task PayEMIAsync(int repaymentId, decimal amount, string proofPath)
        {
            using var context = await _factory.CreateDbContextAsync();
            var emi = await context.LoanRepayments.FindAsync(repaymentId);
            if (emi != null)
            {
                emi.PaymentDate = DateTime.Now;
                emi.PaymentAmount = amount;
                emi.PaymentProofPath = proofPath;
                emi.Status = "Paid";
                await context.SaveChangesAsync();
            }
        }

        // --- AUDIT LOGIC ---
        public async Task LogEditAsync(int appId, string user, string field, string oldVal, string newVal, string reason)
        {
            using var context = await _factory.CreateDbContextAsync();
            context.LoanAuditLogs.Add(new LoanAuditLog
            {
                ApplicationID = appId,
                ModifiedBy = user,
                FieldChanged = field,
                OldValue = oldVal,
                NewValue = newVal,
                Reason = reason
            });
            await context.SaveChangesAsync();
        }
    }
}