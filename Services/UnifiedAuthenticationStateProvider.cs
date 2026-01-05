using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using EmployeeLoanApp.Services.PMS;
using EmployeeLoanApp.Services;

namespace EmployeeLoanApp.Services
{
    public class UnifiedAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly PMSAuthService _pmsService;
        private readonly AuthService _loanService;

        public UnifiedAuthenticationStateProvider(PMSAuthService pmsService, AuthService loanService)
        {
            _pmsService = pmsService;
            _loanService = loanService;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var identities = new List<ClaimsIdentity>();

            // 1. Fetch LMS User
            var lmsUser = await _loanService.GetUserAsync();
            if (lmsUser.Identity?.IsAuthenticated == true)
            {
                identities.Add((ClaimsIdentity)lmsUser.Identity);
            }

            // 2. Fetch PMS User
            var pmsUser = await _pmsService.GetUserAsync();
            if (pmsUser.Identity?.IsAuthenticated == true)
            {
                identities.Add((ClaimsIdentity)pmsUser.Identity);
            }

            // 3. Return Combined Principal (Might contain 0, 1, or 2 identities)
            var compositeUser = new ClaimsPrincipal(identities);
            return new AuthenticationState(compositeUser);
        }

        public async Task LogoutSpecificAsync(string route)
        {
            if (route == "pms")
            {
                await _pmsService.LogoutAsync();
            }
            else if (route == "loan")
            {
                await _loanService.LogoutAsync();
            }

            NotifyAuthStateChanged();
        }

        public void NotifyAuthStateChanged()
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}