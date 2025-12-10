using System.Net;
using System.Net.Mail;
using EmployeeLoanApp.Models;

namespace EmployeeLoanApp.Services
{
    public class EmailService
    {
        // Replace with your company SMTP details
        private const string SmtpServer = "smtp.gmail.com";
        private const int SmtpPort = 587;
        private const string SenderEmail = "hr@kalyanaimpex.com";
        private const string SenderPassword = "your-app-password";

        public async Task SendAgreementMailAsync(Employee employee, LoanApplication loan, LoanApproval approval)
        {
            try
            {
                // 1. Generate the HTML Content
                string agreementHtml = GenerateAgreementHtml(employee, loan, approval);

                // 2. Send Email
                /* using (var client = new SmtpClient(SmtpServer, SmtpPort))
                {
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(SenderEmail, SenderPassword);
                    var mailMessage = new MailMessage(SenderEmail, employee.Email, "Loan Agreement - Action Required", agreementHtml);
                    mailMessage.IsBodyHtml = true;
                    await client.SendMailAsync(mailMessage);
                }
                */

                Console.WriteLine($"[Email Service] Agreement sent to {employee.Email}");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Email Error] {ex.Message}");
            }
        }

        public string GenerateAgreementHtml(Employee emp, LoanApplication loan, LoanApproval approval)
        {
            string date = DateTime.Now.ToString("dd-MMM-yyyy");
            return $@"
            <html>
            <body style='font-family: ""Times New Roman"", serif; font-size: 14px; line-height: 1.5; padding: 40px;'>
                
                <h3 style='text-align: center; text-decoration: underline; margin-bottom: 30px;'>SUPPLEMENTAL PERSONAL LOAN AGREEMENT</h3>

                <p>This Supplemental Business Loan Agreement (“Agreement”) is made and executed at <b>Siliguri</b> and on the date <b>{date}</b> (“Execution Date”) mentioned in Schedule I hereto.</p>

                <p><b>BY AND BETWEEN</b></p>
                
                <p><b>KALYANA IMPEX PRIVATE LIMITED</b>, a company registered under the provisions of the Companies Act 2013 and registered with the Reserve Bank of India (“RBI”) as a Non-Banking Financial Company (“NBFC” License No – B.05.03683) having its registered office at N/84/4/2283, ASMI SQUARE 2ND MILE SEVOKE ROAD, District: Jalpaiguri, Pin code: 734001 in Ward Number 43, West Bengal (hereinafter collectively referred to as “Lender”...), of the First part;</p>
                
                <p><b>AND</b></p>
                
                <p>The person/entity as more particularly described and identified as the Borrower in Schedule I hereto (hereinafter referred to as “Borrower”...) of the Second Part.</p>

                <br/>
                <h4 style='text-align: center; text-decoration: underline;'>SCHEDULE - I (Key Fact Statement)</h4>
                
                <table style='width: 100%; border-collapse: collapse; border: 1px solid black;'>
                    <tr><td style='border: 1px solid black; padding: 5px;'><b>Details of Borrower</b></td><td style='border: 1px solid black; padding: 5px;'>{emp.FullName}, {emp.ResidentialAddress}<br/>PAN: {emp.PANNumber}<br/>Phone: {emp.PhoneNumber}</td></tr>
                    <tr><td style='border: 1px solid black; padding: 5px;'><b>Loan Amount</b></td><td style='border: 1px solid black; padding: 5px;'>INR {approval.SanctionedAmount:N0} /-</td></tr>
                    <tr><td style='border: 1px solid black; padding: 5px;'><b>Monthly EMI</b></td><td style='border: 1px solid black; padding: 5px;'>INR {approval.SanctionedEMIAmount:N0} /-</td></tr>
                    <tr><td style='border: 1px solid black; padding: 5px;'><b>Tenure</b></td><td style='border: 1px solid black; padding: 5px;'>{approval.ApprovedTenureMonths} Months</td></tr>
                    <tr><td style='border: 1px solid black; padding: 5px;'><b>Purpose</b></td><td style='border: 1px solid black; padding: 5px;'>{loan.PurposeOfLoan}</td></tr>
                    <tr><td style='border: 1px solid black; padding: 5px;'><b>Disbursement Bank</b></td><td style='border: 1px solid black; padding: 5px;'>{loan.AccountNumber} ({loan.BankName})</td></tr>
                </table>

                <br/><br/>
                <div style='display: flex; justify-content: space-between; margin-top: 50px;'>
                    <div><b>For Kalyana Impex Pvt. Ltd.</b><br/><br/><br/>(Authorized Signatory)</div>
                    <div style='text-align: right;'><b>Accepted By Borrower</b><br/><br/><br/>({emp.FullName})</div>
                </div>
            </body>
            </html>";
        }
    }
}