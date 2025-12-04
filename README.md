##🏦 Employee Loan Management System (CRM)

#A modern, full-stack Loan Management CRM built with .NET 8 Blazor Server and MudBlazor. This application streamlines the loan lifecycle for organizations, from employee application to HR sanctioning, agreement signing, and final disbursement.

##🚀 Features

🔹 1. Comprehensive Loan Lifecycle (3-Stage Workflow)

The system enforces a strict 3-step approval process to ensure compliance:

Pending Approval: Employee applies -> HR reviews the application.

Pending Agreement: HR Sanctions -> System generates an Agreement -> Employee signs digitally.

Pending Payment: Agreement Signed -> Finance/HR disburses the funds -> Loan becomes Active.

🔹 2. Master Data Management

Employee Directory: comprehensive management of employee records.

Legacy Data Support: Ability to import Opening Balances and NOC Status for existing loans (e.g., from KEKA or previous systems).

Bulk Import: CSV upload feature to onboard hundreds of employees instantly with validation.

🔹 3. Loan Application Portal

Self-Service: Employees can apply for loans via a dedicated portal.

Auto-Fill: Personal and banking details are automatically populated from the Master DB.

Loan Types: Support for Employee Loans, Individual, Pvt. Ltd., and Partnership Firm applications.

🔹 4. HR Dashboard & Sanctioning

Sanction Wizard: A step-by-step wizard to verify details, set tenure/interest, and add admin remarks.

Digital Trail: System tracks who verified the loan and when.

Automated Notifications: (Simulated) Emails sent to employees upon sanctioning.

🛠️ Tech Stack

Framework: .NET 8.0 (Blazor Server)

UI Library: MudBlazor (Material Design)

Database: Microsoft SQL Server (MSSQL) / Entity Framework Core

Architecture: Service-Repository Pattern

Services: Native File Uploads, Email Notification Service (SMTP)

📸 Screenshots

Dashboard

Application Form

HR Dashboard View

Employee Loan Application

Employee Directory

Sanction Wizard

Manage & Import Staff

Approval Process

⚙️ Installation & Setup

1. Prerequisites

.NET 8.0 SDK

SQL Server (LocalDB or Standard)

Visual Studio 2022

2. Clone & Restore

git clone [https://github.com/your-username/employee-loan-crm.git](https://github.com/your-username/employee-loan-crm.git)
cd employee-loan-crm
dotnet restore


3. Database Configuration

Open appsettings.json and update your connection string:

"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=EmployeeLoanDB;Trusted_Connection=True;"
}


Critical Step: Run the SQL Script located at /SQL/update_schema.sql (or see the repository root) to create the required tables and columns.

4. Run the Application

dotnet run


The application will launch at https://localhost:7000 (or similar).

📝 Usage Guide

For HR / Admin

Onboard Employees: Go to "Manage Employees" -> "New Entry" or use "Bulk Import" to upload your staff list.

Review Loans: Check the "Dashboard" for pending requests.

Sanction: Click "Sanction" -> Fill in the approved amount and tenure -> Submit.

Disburse: Once the employee signs the agreement, the status changes to "Pending Payment". Click "Disburse" to release funds.

For Employees

Apply: Go to "New Application" -> Select your name -> Fill loan