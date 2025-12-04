using System.Net.Http.Json;
using EmployeeLoanApp.Models;

namespace EmployeeLoanApp.Services
{
    public class DigiGoService
    {
        private readonly HttpClient _httpClient;

        // Replace with your actual Digio Credentials
        private const string BaseUrl = "https://api.digio.in/v2";
        private const string ClientId = "YOUR_CLIENT_ID";
        private const string ClientSecret = "YOUR_CLIENT_SECRET";

        public DigiGoService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Triggered by HR Approval to send Agreement via Email
        public async Task<string?> SendAgreementForSignatureAsync(Employee employee, LoanApplication loan, LoanApproval approval)
        {
            try
            {
                // 1. Generate the Agreement Content (HTML or PDF)
                // Ideally, this HTML matches your 'AGREEMENT 2.docx' content
                string htmlContent = $@"
                    <html>
                    <body>
                        <h1>SUPPLEMENTAL PERSONAL LOAN AGREEMENT</h1>
                        <p>This Agreement is made on <b>{DateTime.Now:dd-MMM-yyyy}</b> between <b>KALYANA IMPEX PVT. LTD.</b> and <b>{employee.FullName}</b>.</p>
                        <p>Loan Amount: <b>INR {approval.SanctionedAmount}</b></p>
                        <p>Tenure: <b>{approval.ApprovedTenureMonths} Months</b></p>
                        <p>EMI: <b>INR {approval.SanctionedEMIAmount}</b></p>
                        <br/>
                        <p>Please sign this document to proceed with the loan disbursement.</p>
                    </body>
                    </html>";

                // 2. Prepare Digio Payload for Email Signing
                // This payload instructs Digio to email the document to the signer
                var payload = new
                {
                    signers = new[]
                    {
                        new {
                            identifier = employee.Email, // Email where the link is sent
                            name = employee.FullName,
                            reason = "Loan Agreement Signature"
                        }
                    },
                    expire_in_days = 10,
                    display_on_page = "all",
                    notify_signers = true, // Digio will send the email
                    file_name = $"Loan_Agreement_{loan.ApplicationID}.pdf",
                    file_data = htmlContent, // HTML content to be converted to PDF
                    // Important: Set your Webhook URL here or in the Digio Dashboard
                    // webhook_url = "https://your-app-url/api/webhooks/digigo" 
                };

                // 3. Call Digio API to Create Request (Simulated)
                /* var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/client/document/uploadxml");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", 
                    Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{ClientId}:{ClientSecret}")));
                request.Content = JsonContent.Create(payload);
                
                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                // Parse response to get Document ID...
                */

                Console.WriteLine($"[DigiGo] Sent Agreement Email to {employee.Email} for Loan #{loan.ApplicationID}");

                // Return a dummy Request ID for tracking in this demo
                return $"DIGIGO_REQ_{Guid.NewGuid()}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DigiGo Error] {ex.Message}");
                return null;
            }
        }
    }
}