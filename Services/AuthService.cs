using System.Security.Cryptography;
using System.Text;
using EmployeeLoanApp.Data;
using EmployeeLoanApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Security.Claims;

namespace EmployeeLoanApp.Services
{
    public class AuthService : AuthenticationStateProvider
    {
        private readonly IDbContextFactory<EmployeeLoanContext> _factory;
        private readonly ProtectedLocalStorage _localStorage;
        private ClaimsPrincipal _currentUser = new ClaimsPrincipal(new ClaimsIdentity()); // Default to anonymous

        public AuthService(IDbContextFactory<EmployeeLoanContext> factory, ProtectedLocalStorage localStorage)
        {
            _factory = factory;
            _localStorage = localStorage;
        }

        // 1. Persist Login State (The "Auto-Login" Logic)
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                // If we already have a user in memory, return it (Optimization)
                if (_currentUser.Identity?.IsAuthenticated == true)
                {
                    return new AuthenticationState(_currentUser);
                }

                // Otherwise, try to read from Browser Storage
                var userSessionResult = await _localStorage.GetAsync<UserSession>("UserSession");

                if (userSessionResult.Success && userSessionResult.Value != null)
                {
                    var session = userSessionResult.Value;
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, session.Username),
                        new Claim(ClaimTypes.Role, session.Role)
                    };

                    var identity = new ClaimsIdentity(claims, "LocalStorageAuth");
                    _currentUser = new ClaimsPrincipal(identity);
                }
            }
            catch
            {
                // This catch is necessary because LocalStorage is not available during 
                // server-side pre-rendering. We simply ignore it and return Anonymous.
            }

            return new AuthenticationState(_currentUser);
        }

        // 2. Login Logic (DB Check + Save Session)
        public async Task<bool> LoginAsync(string username, string password)
        {
            using var context = await _factory.CreateDbContextAsync();
            var hashedPassword = HashPassword(password);

            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Username == username && u.PasswordHash == hashedPassword && u.IsActive);

            if (user != null)
            {
                // A. Create the simple session object
                var session = new UserSession
                {
                    Username = user.Username,
                    Role = user.Role
                };

                // B. Save to Browser Storage (Persist!)
                await _localStorage.SetAsync("UserSession", session);

                // C. Update In-Memory State
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role)
                };

                var identity = new ClaimsIdentity(claims, "CustomAuth");
                _currentUser = new ClaimsPrincipal(identity);

                // D. Notify Blazor
                NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
                return true;
            }

            return false;
        }

        // 3. Logout Logic
        public async Task LogoutAsync()
        {
            // A. Clear Storage
            await _localStorage.DeleteAsync("UserSession");

            // B. Clear Memory
            _currentUser = new ClaimsPrincipal(new ClaimsIdentity());

            // C. Notify Blazor
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        // --- HELPER METHODS (Kept from your original code) ---

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                var builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public async Task<bool> CreateUserAsync(string username, string password, string role, int? employeeId = null)
        {
            using var context = await _factory.CreateDbContextAsync();

            if (await context.Users.AnyAsync(u => u.Username == username))
                return false;

            var newUser = new User
            {
                Username = username,
                PasswordHash = HashPassword(password),
                Role = role,
                EmployeeID = employeeId,
                IsActive = true
            };

            context.Users.Add(newUser);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<List<User>> GetAllAdminsAsync()
        {
            using var context = await _factory.CreateDbContextAsync();
            return await context.Users.ToListAsync();
        }
    }

    // Small helper class to store minimal data in browser cookie/storage
    public class UserSession
    {
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}