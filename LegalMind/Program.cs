using LegalMind.Web.Data;
using LegalMind.Web.Services.AI;
using LegalMind.Web.Services.Retrieval;
using LegalMind.Web.Services.Safety;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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
.AddRoles<IdentityRole>()
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
//
// Also seeds a default Admin role + account when an Admin email is configured.
// In Development, Admin email/password default to dj.idorot@gmail.com / DevOnly!ChangeMe123.
var adminEmailCfg = app.Configuration["Admin:Email"];
var adminPasswordCfg = app.Configuration["Admin:Password"];

var effectiveAdminEmail = !string.IsNullOrWhiteSpace(adminEmailCfg)
    ? adminEmailCfg
    : (app.Environment.IsDevelopment() ? "dj.idorot@gmail.com" : null);

var effectiveAdminPassword = !string.IsNullOrWhiteSpace(adminPasswordCfg)
    ? adminPasswordCfg
    : (app.Environment.IsDevelopment() ? "DevOnly!ChangeMe123" : null);

if (app.Environment.IsDevelopment() || !string.IsNullOrWhiteSpace(effectiveAdminEmail))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();

    // Seed a default Admin role + admin account.
    // Configure via appsettings*.json (Admin:Email, Admin:Password).
    // NOTE: In non-Development environments, seeding only happens when Admin:Email is explicitly set.
    var adminEmail = effectiveAdminEmail;
    var adminPassword = effectiveAdminPassword;

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    if (!string.IsNullOrWhiteSpace(adminEmail))
    {
        if (!roleManager.RoleExistsAsync("Admin").GetAwaiter().GetResult())
        {
            roleManager.CreateAsync(new IdentityRole("Admin")).GetAwaiter().GetResult();
        }

        var adminUser = userManager.FindByEmailAsync(adminEmail).GetAwaiter().GetResult();
        if (adminUser == null)
        {
            adminUser = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            // Only create a local password user when a password is provided (Development defaults one).
            if (!string.IsNullOrWhiteSpace(adminPassword))
            {
                userManager.CreateAsync(adminUser, adminPassword).GetAwaiter().GetResult();
            }
            else
            {
                userManager.CreateAsync(adminUser).GetAwaiter().GetResult();
            }
        }

        if (!userManager.IsInRoleAsync(adminUser, "Admin").GetAwaiter().GetResult())
        {
            userManager.AddToRoleAsync(adminUser, "Admin").GetAwaiter().GetResult();
        }
    }
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

// Safety net: if the signed-in user's email matches Admin:Email, ensure they are in the Admin role.
// This prevents "Access denied" when the user was created via external login after startup, or when
// the app was run without the expected environment settings.
app.Use(async (context, next) =>
{
    if (context.User?.Identity?.IsAuthenticated == true)
    {
        var adminEmail = app.Configuration["Admin:Email"];
        if (string.IsNullOrWhiteSpace(adminEmail) && app.Environment.IsDevelopment())
        {
            adminEmail = "dj.idorot@gmail.com";
        }

        var userEmail = context.User.FindFirstValue(ClaimTypes.Email)
            ?? context.User.FindFirstValue("email")
            ?? context.User.Identity?.Name;

        if (!string.IsNullOrWhiteSpace(adminEmail)
            && !string.IsNullOrWhiteSpace(userEmail)
            && string.Equals(adminEmail, userEmail, StringComparison.OrdinalIgnoreCase))
        {
            var roleManager = context.RequestServices.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = context.RequestServices.GetRequiredService<UserManager<IdentityUser>>();
            var signInManager = context.RequestServices.GetRequiredService<SignInManager<IdentityUser>>();

            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            var dbUser = await userManager.FindByEmailAsync(userEmail);
            if (dbUser != null && !await userManager.IsInRoleAsync(dbUser, "Admin"))
            {
                await userManager.AddToRoleAsync(dbUser, "Admin");
                await signInManager.RefreshSignInAsync(dbUser); // updates cookie with role claim
            }
        }
    }

    await next();
});

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages(); // Identity UI pages

app.Run();
