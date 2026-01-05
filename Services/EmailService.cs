using System.Net;
using System.Net.Mail;
using System.Text;
using EmployeeLoanApp.Models;

namespace EmployeeLoanApp.Services
{
    public class EmailService
    {
        private const string SmtpServer = "smtp.gmail.com";
        private const int SmtpPort = 587;
        private const string SenderEmail = "pandeyjigaming700@gmail.com"; // MUST match the account generating the App Password
        private const string SenderPassword = "nsxt qinh yysj ezyz"; // PASTE YOUR 16-CHAR APP PASSWORD HERE

        public async Task SendAgreementMailAsync(Employee employee, LoanApplication loan, LoanApproval approval)
        {
            try
            {
                string agreementHtml = GenerateAgreementHtml(employee, loan, approval);

                // In production: Convert HTML to PDF using a library (e.g. DinkToPdf) and attach.
                // For MVP: Sending as HTML Body.
                using (var client = new SmtpClient(SmtpServer, SmtpPort))
                {
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(SenderEmail, SenderPassword);
                    // Create the message
                    var mailMessage = new MailMessage();
                    mailMessage.From = new MailAddress(SenderEmail, "Kalyana Impex HR");
                    mailMessage.To.Add(new MailAddress(employee.Email!));
                    mailMessage.Subject = $"Loan Agreement - Application #{loan.ApplicationID}";
                    mailMessage.Body = agreementHtml;
                    mailMessage.IsBodyHtml = true;
                    await client.SendMailAsync(mailMessage);

                }

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
            string loanAmount = approval.SanctionedAmount.ToString("N0");
            string emiAmount = approval.SanctionedEMIAmount.ToString("N0");

            return $@"
            <html>
            <head>
                <style>
                    body {{ font-family: 'Times New Roman', serif; font-size: 14px; line-height: 1.4; color: #000; }}
                    h3 {{ text-align: center; text-transform: uppercase; text-decoration: underline; font-size: 16px; margin-bottom: 20px; }}
                    p {{ margin-bottom: 10px; text-align: justify; }}
                    table {{ width: 100%; border-collapse: collapse; margin-top: 15px; font-size: 13px; }}
                    td, th {{ border: 1px solid #000; padding: 6px; vertical-align: top; }}
                    .bold {{ font-weight: bold; }}
                    .center {{ text-align: center; }}
                    .schedule-header {{ text-align: center; font-weight: bold; margin-top: 30px; text-decoration: underline; }}
                </style>
            </head>
            <body>
                
                <h3>SUPPLEMENTAL PERSONAL LOAN AGREEMENT</h3>

                <p>This Supplemental Business Loan Agreement (“Agreement”) is made and executed at the place and on the date (“Execution Date”) mentioned in Schedule I hereto.</p>

                <p class='bold'>BY AND BETWEEN</p>
                
                <p><b>KALYANA IMPEX PRIVATE LIMITED</b>, a company registered under the provisions of the Companies Act 2013 and registered with the Reserve Bank of India (“RBI”) as a Non-Banking Financial Company (“NBFC” License No – B.05.03683) having its registered office at N/84/4/2283, ASMI SQUARE 2ND MILE SEVOKE ROAD, District: Jalpaiguri, Pin code: 734001 in Ward Number 43, West Bengal (hereinafter collectively referred to as “Lender”...), of the <b>First part</b>;</p>
                
                <p class='bold'>AND</p>
                
                <p>The person/entity as more particularly described and identified as the Borrower in Schedule I hereto (hereinafter referred to as “Borrower”...) of the <b>Second Part</b>.</p>

                <p>Hereinafter, Lender and Borrower shall be referred to individually as “Party” and collectively as “Parties”.</p>

                <p><b>WHEREAS,</b></p>
                <p>The Parties have entered into a Personal Loan Agreement dated <b>{date}</b> for loan account no. <b>{loan.ApplicationID}</b> along with General Terms and conditions... as per which the Lender has provided a Personal Loan to the Borrower...</p>
                
                <p>Subsequently, the Borrower has requested the Lender to Personal Loan the facility for a period mentioned in Schedule I with the same terms & conditions... The Lender has agreed to this request...</p>

                <div class='schedule-header'>SCHEDULE - I</div>
                <div class='center bold'>Detailed Terms of Loan/Key Fact Statement</div>
                
                <table>
                    <tr>
                        <th width='50'>Sr.</th>
                        <th width='40%'>Particulars</th>
                        <th>Details</th>
                    </tr>
                    <tr><td>1.</td><td>Agreement Execution Date</td><td>{date}</td></tr>
                    <tr><td>2.</td><td>Agreement Execution Place</td><td>Siliguri</td></tr>
                    <tr>
                        <td>3.</td>
                        <td><b>Details of the Borrowing</b><br/>Name<br/>Address<br/>PAN<br/>Contact<br/>Email</td>
                        <td>
                            <br/><b>{emp.FullName}</b><br/>{emp.ResidentialAddress}<br/>{emp.PANNumber}<br/>{emp.PhoneNumber}<br/>{emp.Email}
                        </td>
                    </tr>
                    <tr>
                        <td></td>
                        <td><b>Details of the Guarantor</b><br/>Name<br/>Address</td>
                        <td><br/>N/A<br/>-</td>
                    </tr>
                    <tr><td colspan='3' class='bold' style='background:#f0f0f0;'>Details of Loan</td></tr>
                    <tr><td>9.</td><td>Loan Amount</td><td><b>INR {loanAmount} /-</b></td></tr>
                    <tr><td>10.</td><td>Additional Loan amount</td><td>NIL</td></tr>
                    <tr><td>11.</td><td>Total Loan Amount</td><td><b>INR {loanAmount} /-</b></td></tr>
                    <tr><td>12.</td><td>Monthly EMI amount</td><td><b>INR {emiAmount} /-</b></td></tr>
                    <tr><td>13.</td><td>Tenure</td><td>{approval.ApprovedTenureMonths} Months</td></tr>
                    <tr><td>14.</td><td>Purpose of Loan</td><td>{loan.PurposeOfLoan}</td></tr>
                    <tr>
                        <td>15.</td>
                        <td>Disbursed Bank Account</td>
                        <td>{loan.AccountNumber} ({loan.BankName})<br/>Holder: {loan.AccountHolderName}</td>
                    </tr>
                    <tr><td>16.</td><td>Repayment Bank</td><td>Kalyana Impex Pvt Ltd, Axis Bank</td></tr>
                    <tr><td>17.</td><td>Repayment Frequency</td><td>Monthly</td></tr>
                    <tr><td>18.</td><td>Processing Fees</td><td>Nil</td></tr>
                    <tr><td>19.</td><td>Interest Rate (P.M.)</td><td>9%</td></tr>
                    <tr><td>20.</td><td>Penal Charges</td><td>Nil</td></tr>
                    <tr><td>24.</td><td>Grievance Officer</td><td>Name: _____________</td></tr>
                </table>

                <br/><br/>
                <table style='border:none;'>
                    <tr style='border:none;'>
                        <td style='border:none; width:50%;'>
                            <b>For Kalyana Impex Pvt. Ltd.</b><br/><br/><br/><br/>
                            (Authorized Signatory)<br/>Date: {date}
                        </td>
                        <td style='border:none; width:50%; text-align:right;'>
                            <b>Accepted By Borrower</b><br/><br/><br/><br/>
                            ({emp.FullName})
                        </td>
                    </tr>
                </table>
            </body>
            </html>";
        }
    }
}