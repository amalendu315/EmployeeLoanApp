using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeLoanApp.Models
{
    // --- NEW MASTER MODELS ---
    public class ApplicationType
    {
        [Key]
        public int TypeID { get; set; }
        public string TypeName { get; set; } = "";
        public bool IsActive { get; set; } = true;
    }

    public class Company
    {
        [Key]
        public int CompanyID { get; set; }
        public string CompanyName { get; set; } = "";
        public bool IsActive { get; set; } = true;
    }

    public class LoanPurpose
    {
        [Key]
        public int PurposeID { get; set; }
        public string PurposeName { get; set; } = "";
        public bool IsActive { get; set; } = true;
    }

    public class Employee
    {
        [Key]
        public int EmployeeID { get; set; }

        [Required]
        public string EmployeeCode { get; set; } = string.Empty;

        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public string Department { get; set; } = string.Empty;

        public string? Designation { get; set; }
        public string? Company { get; set; }
        public DateTime? DateOfJoining { get; set; }

        [Required]
        public string PANNumber { get; set; } = string.Empty;

        [Required]
        public string AadhaarNumber { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? ResidentialAddress { get; set; }

        public string? ReportingManagerName { get; set; }
        public string? ReportingManagerID { get; set; }

        // --- MASTER DATA / OPENING BALANCE FIELDS ---
        [Column(TypeName = "decimal(18, 2)")]
        public decimal OpeningLoanBalance { get; set; } = 0;

        public bool IsNocClear { get; set; } = true;
        public string? NocAttachmentPath { get; set; }

        // --- ADD THESE MISSING FIELDS ---
        public string? BankName { get; set; }
        public string? BankBranch { get; set; }
        public string? BankAccountHolderName { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? BankIFSC { get; set; }

        // KYC Docs
        public string? PanCardPath { get; set; }
        public string? AadhaarCardPath { get; set; }
    }

    public class LoanApplication
    {
        [Key]
        public int ApplicationID { get; set; }

        public int? EmployeeID { get; set; }
        [ForeignKey("EmployeeID")]
        public Employee? Employee { get; set; }

        // NEW: Loan Type Selection
        public string LoanType { get; set; } = "Employee";

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal LoanAmountRequested { get; set; }

        public string? ExistingLoanDetails { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? ProposedEMIAmount { get; set; }

        [Required]
        public int LoanTenureMonths { get; set; }

        public string? PurposeOfLoan { get; set; }

        // Bank Details
        public string? BankName { get; set; }
        public string? BranchName { get; set; }
        public string? AccountHolderName { get; set; }
        public string? AccountNumber { get; set; }
        public string? IFSCCode { get; set; }

        public string? EmployeeRemarks { get; set; }

        [Required]
        public bool IsDeclared { get; set; }

        // Status Workflow: Pending Approval -> Pending Agreement -> Pending Payment -> Active
        public string? ApplicationStatus { get; set; } = "Pending Approval";

        public DateTime SubmissionDate { get; set; } = DateTime.Now;

        public string? SignedAgreementPath { get; set; }
    }

    public class LoanApproval
    {
        [Key]
        public int ApprovalID { get; set; }

        public int? ApplicationID { get; set; }
        [ForeignKey("ApplicationID")]
        public LoanApplication? Application { get; set; }

        public string? VerifiedBy { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal SanctionedAmount { get; set; }

        public int ApprovedTenureMonths { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal SanctionedEMIAmount { get; set; }

        public string? ApprovingAuthorityName { get; set; }
        public string? ApprovingAuthorityDesignation { get; set; }
        public string? ApprovalProofAttachmentPath { get; set; }

        public DateTime ApprovalDate { get; set; } = DateTime.Now;
        public string? AdminComments { get; set; }
    }
}