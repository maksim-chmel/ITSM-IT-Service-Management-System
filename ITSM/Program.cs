using ITSM.Data;
using ITSM.Middleware;
using ITSM.Models;
using ITSM.Services.Archive;
using ITSM.Services.Automation;
using ITSM.Services.Authorisation;
using ITSM.Services.Charts;
using ITSM.Services.Discussion;
using ITSM.Services.KnowledgeBase;
using ITSM.Services.Qualification;
using ITSM.Services.RoleManager;
using ITSM.Services.Ticket;
using ITSM.Services.TicketAssignment;
using ITSM.Services.TicketCategory;
using ITSM.Services.TicketSort;
using ITSM.Services.UserManagement;
using ITSM.Services.UserProfile;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection;
using System.IO;
using ITSM.Authorization;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

var connectionString =
    builder.Configuration["CONNECTION_STRING"] ??
    builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Database connection string is not configured. Set CONNECTION_STRING or ConnectionStrings:DefaultConnection.");
}

// Configure Data Protection to persist keys in a volume-mapped directory.
// In Docker we mount a volume and pass DATA_PROTECTION_KEYS_PATH=/app/keys.
var keysPath =
    builder.Configuration["DATA_PROTECTION_KEYS_PATH"] ??
    Path.Combine(builder.Environment.ContentRootPath, "keys");
if (!Directory.Exists(keysPath))
{
    Directory.CreateDirectory(keysPath);
}

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keysPath))
    .SetApplicationName("ITSM_Application");

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<DBaseContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<DBaseContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthorization();
builder.Services.AddSingleton<IAuthorizationHandler, TicketAuthorizationHandler>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITicketCategoryService, TicketCategoryService>();
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddScoped<IUserRolesService, UserRolesService>();
builder.Services.AddScoped<ITicketAssignmentService, TicketAssignmentService>();
builder.Services.AddScoped<ITicketSortService, TicketSortService>();
builder.Services.AddScoped<IUserProfileService, UserProfileService>();
builder.Services.AddScoped<IDiscussionService, DiscussionService>();
builder.Services.AddScoped<IQualificationService, QualificationService>();
builder.Services.AddScoped<IKnowledgeBaseService, KnowledgeBaseService>();
builder.Services.AddScoped<IAutoServiceService,AutoServiceService>();
builder.Services.AddScoped<IArchiveService,ArchiveService>();
builder.Services.AddScoped<ITicketChartService,TicketChartService>();
builder.Services.AddLogging();
builder.Services.AddHealthChecks();


var app = builder.Build();

app.MapHealthChecks("/health");var autoMigrate = builder.Configuration.GetValue("AUTO_MIGRATE", true);
var seedDemo = builder.Configuration.GetValue("SEED_DEMO", builder.Environment.IsDevelopment());
if (autoMigrate || seedDemo)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<DBaseContext>();

    if (autoMigrate)
        await dbContext.Database.MigrateAsync();

    if (seedDemo)
    {
        await SeedRoles.Initialize(services);
        await SeedUsers.Initialize(services);
    }
}

if (builder.Configuration.GetValue("ENABLE_HTTPS_REDIRECT", true))
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<ExceptionMiddleware>();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Landing}/{action=Index}/{id?}");
app.MapControllerRoute(
        name: "authorized",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .RequireAuthorization();
app.Run();
