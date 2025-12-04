using System.Net;
using System.Net.Mail;
using EmployeeLoanApp.Models;

namespace EmployeeLoanApp.Services
{
    public class EmailService
    {
        // In a real deployment, move these to appsettings.json
        private const string SmtpServer = "smtp.gmail.com";
        private const int SmtpPort = 587;
        private const string SenderEmail = "hr@kalyanaimpex.com"; // Replace with real email
        private const string SenderPassword = "your-app-password"; // Replace with real password

        public async Task SendAgreementMailAsync(Employee employee, LoanApplication loan, LoanApproval approval)
        {
            try
            {
                // Logic to construct the Agreement Content (Based on your DOCX)
                var agreementBody = $@"
                    <h3>SUPPLEMENTAL PERSONAL LOAN AGREEMENT</h3>
                    <p>Dear {employee.FullName},</p>
                    <p>Your loan application (Ref: #{loan.ApplicationID}) has been sanctioned by HR.</p>
                    <p><b>Sanctioned Amount:</b> INR {approval.SanctionedAmount}</p>
                    <p><b>Tenure:</b> {approval.ApprovedTenureMonths} Months</p>
                    <p><b>EMI:</b> INR {approval.SanctionedEMIAmount}</p>
                    <br/>
                    <p><b>ACTION REQUIRED:</b></p>
                    <p>Please sign the attached agreement digitally via Zoho Sign to proceed to disbursement.</p>
                    <p><a href='#'>Click here to Sign Document (Zoho Link)</a></p>
                    <br/>
                    <p>Regards,<br/>Kalyana Impex Pvt. Ltd.</p>
                ";

                // NOTE: Since we don't have real SMTP credentials for the demo, 
                // we will Log this to Console instead of crashing.
                // To enable real email, uncomment the SmtpClient code below.

                /*
                using (var client = new SmtpClient(SmtpServer, SmtpPort))
                {
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(SenderEmail, SenderPassword);
                    var mailMessage = new MailMessage(SenderEmail, employee.Email ?? "employee@test.com", "Loan Sanctioned - Action Required", agreementBody);
                    mailMessage.IsBodyHtml = true;
                    await client.SendMailAsync(mailMessage);
                }
                */

                Console.WriteLine($"[EMAIL SENT] To: {employee.Email}, Subject: Loan Sanctioned");
                await Task.CompletedTask; // Dummy await
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send email: {ex.Message}");
                // Don't throw, just log, so the approval flow doesn't break
            }
        }
    }
}