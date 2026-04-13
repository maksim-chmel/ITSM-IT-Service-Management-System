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

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<DBaseContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<DBaseContext>()
    .AddDefaultTokenProviders();

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



var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<DBaseContext>();
    await dbContext.Database.MigrateAsync();
    await SeedRoles.Initialize(services);
    await SeedUsers.Initialize(services);
}


app.UseHttpsRedirection();
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
