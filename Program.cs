using EmployeeLoanApp.Components;
using EmployeeLoanApp.Data;
using EmployeeLoanApp.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// --- 1. AUTHENTICATION SERVICES (Register ONCE) ---
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/login"; // Redirect here if a Controller requests auth
    options.ExpireTimeSpan = TimeSpan.FromDays(7); // Optional: Keep logged in for 7 days
});

// --- 2. BLAZOR AUTH & STORAGE ---
builder.Services.AddScoped<ProtectedLocalStorage>(); // Required for your new AuthService
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<AuthenticationStateProvider>(p => p.GetRequiredService<AuthService>());
builder.Services.AddAuthorizationCore();

// --- 3. CORE SERVICES ---
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddMudServices();

// --- 4. DATABASE & CUSTOM SERVICES ---
builder.Services.AddDbContextFactory<EmployeeLoanContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<LoanService>();
builder.Services.AddScoped<DigiGoService>();
builder.Services.AddScoped<EmailService>();

var app = builder.Build();

// --- 5. HTTP REQUEST PIPELINE ---
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

// CRITICAL: These must be in this exact order, between UseStaticFiles and MapControllers
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();