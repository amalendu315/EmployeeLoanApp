using System.Security.Claims;
using System.Threading.Tasks;

namespace EmployeeLoanApp.Interfaces
{
    public interface IAuthService
    {
        Task<bool> LoginAsync(string username, string password);
        Task LogoutAsync();
        Task<ClaimsPrincipal> GetUserAsync();
        Task ClearAuthState();
    }
}