namespace EmployeeLoanApp.Models.PMS
{
    public class PmsAccessConfig
    {
        // --- Company Access ---
        public bool ViewAllCompanies { get; set; }
        public List<int> AllowedCompanyIds { get; set; } = new();

        // --- Property Access ---
        public bool ViewAllProperties { get; set; }

        // If ViewAllProperties is TRUE, these act as global defaults
        public bool GlobalFinancialsAccess { get; set; }
        public bool GlobalComplianceAccess { get; set; }

        // Specific configurations (Overrides or Specific Grants)
        public List<PropertyAccessConfig> SpecificProperties { get; set; } = new();
    }

    public class PropertyAccessConfig
    {
        public int PropertyId { get; set; }
        public string PropertyName { get; set; } = ""; // Stored for easier UI binding
        public bool CanViewFinancials { get; set; }
        public bool CanViewCompliance { get; set; }
    }
}
