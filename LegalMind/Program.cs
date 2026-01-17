using LegalMind.Web.Data;
using LegalMind.Web.Services.AI;
using LegalMind.Web.Services.Retrieval;
using LegalMind.Web.Services.Safety;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Database provider selection
// - Default: SQL Server (production)
// - Development-friendly: SQLite when configured
var dbProvider = builder.Configuration["Database:Provider"] ?? "SqlServer";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (string.Equals(dbProvider, "Sqlite", StringComparison.OrdinalIgnoreCase))
    {
        var sqliteConn = builder.Configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(sqliteConn))
        {
            sqliteConn = "Data Source=legalmind.dev.db";
        }

        options.UseSqlite(sqliteConn);
    }
    else
    {
        var sqlServerConn = builder.Configuration.GetConnectionString("DefaultConnection");
        options.UseSqlServer(sqlServerConn);
    }
});

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

// External authentication providers
var authBuilder = builder.Services.AddAuthentication();

var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];

// Only enable Google auth when credentials are configured.
// This prevents a startup crash when ClientId/ClientSecret are not set yet.
if (!string.IsNullOrWhiteSpace(googleClientId) && !string.IsNullOrWhiteSpace(googleClientSecret))
{
    authBuilder.AddGoogle(options =>
    {
        options.ClientId = googleClientId;
        options.ClientSecret = googleClientSecret;
    });
}

// App services
builder.Services.AddScoped<IAnswerPolicy, AnswerPolicy>();
builder.Services.AddScoped<ILegalSourceRepository, LegalSourceRepository>();
builder.Services.AddScoped<ILegalAiService, LegalAiService>();

var app = builder.Build();

// Create database automatically in Development (especially useful for SQLite).
// Keeps local setup simple without requiring a running SQL Server instance.
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages(); // Identity UI pages

app.Run();
