using EmployeeLoanApp.Components;
using EmployeeLoanApp.Data;
using EmployeeLoanApp.Services;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// 1. MudBlazor Service
builder.Services.AddMudServices();
builder.Services.AddScoped<EmailService>();
// 2. FIX: Use 'AddDbContextFactory' instead of 'AddDbContext'
// This is critical for Blazor Server concurrency safety.
builder.Services.AddDbContextFactory<EmployeeLoanContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3. Loan Logic Service
builder.Services.AddScoped<LoanService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();