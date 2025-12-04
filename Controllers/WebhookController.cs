using Microsoft.AspNetCore.Mvc;
using EmployeeLoanApp.Services;

namespace EmployeeLoanApp.Controllers
{
    [Route("api/webhooks")]
    [ApiController]
    public class WebhookController : ControllerBase
    {
        private readonly LoanService _loanService;

        public WebhookController(LoanService loanService)
        {
            _loanService = loanService;
        }

        // DigiGo calls this when document is signed
        [HttpPost("digigo")]
        public async Task<IActionResult> HandleDigiGoCallback([FromBody] DigiGoWebhookPayload payload)
        {
            // 1. Verify payload status
            if (payload.status == "success")
            {
                // 2. Extract Loan ID (You would typically pass this in metadata or file_name)
                // For demo, we assume the payload contains the ID or we parse it
                if (int.TryParse(payload.file_name?.Split('_')[1], out int loanId))
                {
                    Console.WriteLine($"[Webhook] Agreement Signed for Loan #{loanId}");

                    // 3. Update Status to "Pending Payment"
                    await _loanService.ConfirmAgreementSignedAsync(loanId);
                }
            }

            return Ok("Webhook Received");
        }
    }

    // Simple class to catch the JSON sent by DigiGo
    public class DigiGoWebhookPayload
    {
        public string? status { get; set; }
        public string? file_name { get; set; }
        public string? document_id { get; set; }
    }
}