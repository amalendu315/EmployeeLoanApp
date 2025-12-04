using EmployeeLoanApp.Components;
using EmployeeLoanApp.Data;
using EmployeeLoanApp.Services;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddControllers(); // REQUIRED: For Webhook Controller
builder.Services.AddHttpClient();  // REQUIRED: For DigiGo Service to make API calls

// 1. MudBlazor Service
builder.Services.AddMudServices();

// 2. Database
builder.Services.AddDbContextFactory<EmployeeLoanContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3. Custom Services
builder.Services.AddScoped<LoanService>();
builder.Services.AddScoped<DigiGoService>(); // Register DigiGo
builder.Services.AddScoped<EmailService>();  // Register Email

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

// REQUIRED: Activate the Webhook routes
app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();