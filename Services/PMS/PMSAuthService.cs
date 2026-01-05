//using System.Security.Claims;
//using System.Security.Cryptography;
//using System.Text;
//using System.Text.Json;
//using Microsoft.EntityFrameworkCore;
//using EmployeeLoanApp.Data;
//using EmployeeLoanApp.Models.PMS;
//using Microsoft.AspNetCore.Components.Authorization;
//using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
//using EmployeeLoanApp.Interfaces;


//namespace EmployeeLoanApp.Services.PMS
//{
//    public class PMSAuthService : AuthenticationStateProvider
//    {
//        private readonly IDbContextFactory<EmployeeLoanContext> _dbFactory;
//        private readonly ProtectedLocalStorage _localStorage;
//        private ClaimsPrincipal _currentUser = new ClaimsPrincipal(new ClaimsIdentity()); // Default to anonymous

//        public PMSAuthService(IDbContextFactory<EmployeeLoanContext> dbFactory, ProtectedLocalStorage localStorage)
//        {
//            _dbFactory = dbFactory;
//            _localStorage = localStorage;
//        }

//        // --- 1. PERSIST LOGIN STATE (Auto-Login) ---
//        public async Task<ClaimsPrincipal> GetUserAsync()
//        {
//            try
//            {
//                if (_currentUser.Identity?.IsAuthenticated == true) return _currentUser;

//                var userSessionResult = await _localStorage.GetAsync<PmsUserSession>("PmsUserSession");
//                if (userSessionResult.Success && userSessionResult.Value != null)
//                {
//                    var session = userSessionResult.Value;
//                    var claims = GenerateClaims(session.Username, session.IsSuperAdmin, session.AccessConfigJson);
//                    var identity = new ClaimsIdentity(claims, "PmsLocalStorageAuth");
//                    _currentUser = new ClaimsPrincipal(identity);
//                }
//            }
//            catch { /* Handle Prerendering */ }

//            return _currentUser;
//        }

//        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
//        {
//            try
//            {
//                // If we already have a user in memory, return it (Optimization)
//                if (_currentUser.Identity?.IsAuthenticated == true)
//                {
//                    return new AuthenticationState(_currentUser);
//                }

//                // Otherwise, try to read from Browser Storage
//                var userSessionResult = await _localStorage.GetAsync<UserSession>("PmsUserSession");

//                if (userSessionResult.Success && userSessionResult.Value != null)
//                {
//                    var session = userSessionResult.Value;
//                    var claims = new List<Claim>
//                    {
//                        new Claim(ClaimTypes.Name, session.Username),
//                        new Claim(ClaimTypes.Role, session.Role)
//                    };

//                    var identity = new ClaimsIdentity(claims, "PmsCustomAuth");
//                    _currentUser = new ClaimsPrincipal(identity);
//                }
//            }
//            catch
//            {
//                // This catch is necessary because LocalStorage is not available during 
//                // server-side pre-rendering. We simply ignore it and return Anonymous.
//            }

//            return new AuthenticationState(_currentUser);
//        }

//        // --- 2. LOGIN LOGIC ---
//        public async Task<PmsUser?> LoginAsync(string username, string password)
//        {
//            using var context = _dbFactory.CreateDbContext();

//            // 1. Fetch user
//            var user = await context.PmsUsers
//                .AsNoTracking()
//                .FirstOrDefaultAsync(u => u.Username == username);

//            if (user == null) return null;

//            // 2. Verify Password
//            if (VerifyHash(password, user.PasswordHash))
//            {
//                // A. Create Session Object
//                var session = new PmsUserSession
//                {
//                    Username = user.Username,
//                    IsSuperAdmin = user.IsSuperAdmin,
//                    AccessConfigJson = user.AccessConfigJson
//                };

//                await _localStorage.SetAsync("PmsUserSession", session);

//                var claims = GenerateClaims(user.Username, user.IsSuperAdmin, user.AccessConfigJson);
//                var identity = new ClaimsIdentity(claims, "PmsCustomAuth");

//                // Update internal state
//                _currentUser = new ClaimsPrincipal(identity);

//                // REMOVED: NotifyAuthenticationStateChanged (The Unified Provider handles this)
//                return user;
//            }

//            return null;
//        }

//        // --- 3. LOGOUT LOGIC ---
//        public async Task LogoutAsync()
//        {
//            // A. Clear Storage
//            await _localStorage.DeleteAsync("PmsUserSession");

//            // B. Clear Memory
//            _currentUser = new ClaimsPrincipal(new ClaimsIdentity());

//        }

//        // Add this method to AuthService.cs
//        public async Task ClearAuthState()
//        {
//            await _localStorage.DeleteAsync("UserSession");
//            _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
//            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
//        }

//        // --- 4. REGISTRATION (Admin Only) ---
//        public async Task<string> RegisterUserAsync(string fullName, string password, bool isSuperAdmin, Dictionary<string, bool> permissions)
//        {
//            using var context = _dbFactory.CreateDbContext();

//            string baseUsername = fullName.ToLower().Replace(" ", ".");
//            string finalUsername = baseUsername;
//            int counter = 1;

//            while (await context.PmsUsers.AnyAsync(u => u.Username == finalUsername))
//            {
//                finalUsername = $"{baseUsername}{counter++}";
//            }

//            var newUser = new PmsUser
//            {
//                FullName = fullName,
//                Username = finalUsername,
//                PasswordHash = HashPassword(password),
//                IsSuperAdmin = isSuperAdmin,
//                AccessConfigJson = JsonSerializer.Serialize(permissions)
//            };

//            context.PmsUsers.Add(newUser);
//            await context.SaveChangesAsync();

//            return finalUsername;
//        }

//        // --- HELPER METHODS ---

//        private List<Claim> GenerateClaims(string username, bool isSuperAdmin, string? accessJson)
//        {
//            var claims = new List<Claim>
//            {
//                new Claim(ClaimTypes.Name, username),
//                new Claim("IsSuperAdmin", isSuperAdmin.ToString())
//            };

//            if (isSuperAdmin)
//            {
//                claims.Add(new Claim(ClaimTypes.Role, "SuperAdmin"));
//            }

//            // We can parse the access JSON and add specific permission claims if needed
//            if (!string.IsNullOrEmpty(accessJson))
//            {
//                claims.Add(new Claim("AccessConfig", accessJson));
//            }

//            return claims;
//        }

//        private static string HashPassword(string password)
//        {
//            using var sha256 = SHA256.Create();
//            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
//            return Convert.ToBase64String(bytes);
//        }

//        private static bool VerifyHash(string inputPassword, string storedHash)
//        {
//            var inputHash = HashPassword(inputPassword);
//            return inputHash == storedHash;
//        }

//        public bool HasAccess(PmsUser user, string permissionKey)
//        {
//            // Overload for checking access on a live object
//            if (user.IsSuperAdmin) return true;
//            if (string.IsNullOrEmpty(user.AccessConfigJson)) return false;
//            try
//            {
//                var perms = JsonSerializer.Deserialize<Dictionary<string, bool>>(user.AccessConfigJson);
//                return perms != null && perms.ContainsKey(permissionKey) && perms[permissionKey];
//            }
//            catch { return false; }
//        }
//    }

//    // --- PMS SESSION DTO ---
//    public class PmsUserSession
//    {
//        public string Username { get; set; } = string.Empty;
//        public bool IsSuperAdmin { get; set; }
//        public string? AccessConfigJson { get; set; }
//    }
//}

using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using EmployeeLoanApp.Data;
using EmployeeLoanApp.Models.PMS;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace EmployeeLoanApp.Services.PMS
{
    public class PMSAuthService
    {
        private readonly IDbContextFactory<EmployeeLoanContext> _dbFactory;
        private readonly ProtectedLocalStorage _localStorage;

        public ClaimsPrincipal CurrentUser { get; private set; } = new ClaimsPrincipal(new ClaimsIdentity());

        public PMSAuthService(IDbContextFactory<EmployeeLoanContext> dbFactory, ProtectedLocalStorage localStorage)
        {
            _dbFactory = dbFactory;
            _localStorage = localStorage;
        }

        public async Task<ClaimsPrincipal> GetUserAsync()
        {
            try
            {
                if (CurrentUser.Identity?.IsAuthenticated == true) return CurrentUser;

                var result = await _localStorage.GetAsync<PmsUserSession>("PmsUserSession");
                if (result.Success && result.Value != null)
                {
                    var s = result.Value;
                    CurrentUser = CreatePrincipal(s.Username, s.IsSuperAdmin, s.AccessConfigJson);
                }
            }
            catch { /* Ignored */ }
            return CurrentUser;
        }

        public async Task<PmsUser?> LoginAsync(string username, string password)
        {
            using var context = _dbFactory.CreateDbContext();
            var user = await context.PmsUsers.AsNoTracking().FirstOrDefaultAsync(u => u.Username == username);

            if (user == null) return null;

            if (VerifyHash(password, user.PasswordHash))
            {
                var session = new PmsUserSession
                {
                    Username = user.Username,
                    IsSuperAdmin = user.IsSuperAdmin,
                    AccessConfigJson = user.AccessConfigJson
                };

                await _localStorage.SetAsync("PmsUserSession", session);
                CurrentUser = CreatePrincipal(user.Username, user.IsSuperAdmin, user.AccessConfigJson);
                return user;
            }
            return null;
        }

        public async Task LogoutAsync()
        {
            await _localStorage.DeleteAsync("PmsUserSession");
            CurrentUser = new ClaimsPrincipal(new ClaimsIdentity());
        }

        // --- CORE HELPER: Adds the "SystemName" = "PMS" Claim ---
        private ClaimsPrincipal CreatePrincipal(string username, bool isSuperAdmin, string? accessJson)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username),
                new Claim("IsSuperAdmin", isSuperAdmin.ToString()),
                new Claim("SystemName", "PMS") // <--- CRITICAL TAG
            };

            if (isSuperAdmin) claims.Add(new Claim(ClaimTypes.Role, "SuperAdmin"));
            if (!string.IsNullOrEmpty(accessJson)) claims.Add(new Claim("AccessConfig", accessJson));

            var identity = new ClaimsIdentity(claims, "PMS_Auth");
            return new ClaimsPrincipal(identity);
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        private static bool VerifyHash(string inputPassword, string storedHash)
        {
            var inputHash = HashPassword(inputPassword);
            return inputHash == storedHash;
        }

        public async Task<string> RegisterUserAsync(string fullName, string password, bool isSuperAdmin, Dictionary<string, bool> permissions)
        {
            using var context = _dbFactory.CreateDbContext();
            string finalUsername = fullName.ToLower().Replace(" ", ".");
            // ... (Keep your existing registration logic logic here) ...
            // Simplified for brevity, assume typical implementation
            return finalUsername;
        }
    }

    public class PmsUserSession
    {
        public string Username { get; set; } = string.Empty;
        public bool IsSuperAdmin { get; set; }
        public string? AccessConfigJson { get; set; }
    }
}