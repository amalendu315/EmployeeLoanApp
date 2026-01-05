using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EmployeeLoanApp.Models; // For referencing the Company entity

namespace EmployeeLoanApp.Models.PMS
{
    // ---------------------------------------------------------
    // ENUMS (Efficient integer storage)
    // ---------------------------------------------------------
    public enum OwnedPropertyCategory
    {
        Land = 1,
        CommercialOffice = 2,
        LandAndBuilding = 3,
        Flat = 4
    }

    public enum LeasedPropertyCategory
    {
        Land = 1,
        CommercialOffice = 2,
        LandAndBuilding = 3,
        Flat = 4
    }

    public enum OwnershipType
    {
        Owned = 1,
        Leased = 2
    }

    public enum RegistrationStatus
    {
        Registered = 1,
        Unregistered = 2
    }

    public enum PurchaseMethod
    {
        Cash = 1,
        Loan = 2,
        Both = 3
    }

    // ---------------------------------------------------------
    // 1. PMS USERS (Authentication)
    // ---------------------------------------------------------
    [Table("Users", Schema = "pms")]
    public class PmsUser
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; } // Auto-generated

        [Required]
        public string PasswordHash { get; set; } // Hashed Password

        public bool IsSuperAdmin { get; set; }

        // JSON string to store granular permissions (e.g. {"ViewFinancials": true})
        public string? AccessConfigJson { get; set; }
    }

    // ---------------------------------------------------------
    // 2. MAIN PROPERTY TABLE
    // ---------------------------------------------------------
    [Table("Properties", Schema = "pms")]
    public class Property
    {
        [Key]
        public int Id { get; set; }

        // --- Classifiers ---
        public OwnershipType OwnershipType { get; set; }
        public RegistrationStatus? RegistrationStatus { get; set; } // Nullable (Only for Owned)
        public OwnedPropertyCategory Category { get; set; }

        // --- Basic Info ---
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } // Land Name OR Property/Company Name

        [MaxLength(100)]
        public string? PropertyIdentifier { get; set; } // E.g., Shop1, Office 302

        // --- Foreign Keys to Loan CRM (Companies) ---
        // Purchaser Name (Company associated with your company)
        public int PurchaserId { get; set; }
        // We link this in OnModelCreating to the existing Company table

        public int? UsedByCompanyId { get; set; } // For Flats/Offices

        // --- Measurement & Location ---
        public double AreaMeasurement { get; set; } // SqFt/Acres
        public string? FloorNumber { get; set; }
        [Required]
        public string LocationAddress { get; set; }

        // --- Relationships ---
        public virtual PreviousOwnerDetails PreviousOwner { get; set; }
        public virtual PropertyFinancials Financials { get; set; }
        public virtual ICollection<ComplianceDocument> Compliances { get; set; } = new List<ComplianceDocument>();
    }

    // ---------------------------------------------------------
    // 3. PREVIOUS OWNER (1-to-1 with Property)
    // ---------------------------------------------------------
    [Table("PreviousOwners", Schema = "pms")]
    public class PreviousOwnerDetails
    {
        [Key, ForeignKey("Property")]
        public int PropertyId { get; set; }

        public virtual Property Property { get; set; }

        [MaxLength(150)]
        public string FullName { get; set; }

        [MaxLength(20)]
        public string PhoneNumber { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        public string FullAddress { get; set; }

        // Identity Document
        public string? IdentityDocNumber { get; set; }
        public string? IdentityDocFilePath { get; set; }
    }

    // ---------------------------------------------------------
    // 4. FINANCIALS (1-to-1 with Property)
    // ---------------------------------------------------------
    [Table("PropertyFinancials", Schema = "pms")]
    public class PropertyFinancials
    {
        [Key, ForeignKey("Property")]
        public int PropertyId { get; set; }

        public virtual Property Property { get; set; }

        public PurchaseMethod PurchaseMethod { get; set; }

        // Breakdown
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrincipalAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal RegistryTax { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OtherCharges { get; set; }

        // Loan Details (Optional)
        public int? LoanCompanyId { get; set; } // Dropdown

        [Column(TypeName = "decimal(18,2)")]
        public decimal? BankValuation { get; set; }
    }

    // ---------------------------------------------------------
    // 5. COMPLIANCES (1-to-Many)
    // ---------------------------------------------------------
    [Table("ComplianceDocuments", Schema = "pms")]
    public class ComplianceDocument
    {
        [Key]
        public int Id { get; set; }

        public int PropertyId { get; set; }
        public virtual Property Property { get; set; }

        [Required]
        [MaxLength(150)]
        public string DocumentName { get; set; }

        [Required]
        public string FilePath { get; set; }

        public DateTime? ExpiryDate { get; set; }
    }
}