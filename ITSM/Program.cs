using ITSM;
using ITSM.DB;
using ITSM.Middleware;
using ITSM.Models;
using ITSM.Repositories.Archive;
using ITSM.Repositories.Authomatization;
using ITSM.Repositories.Authorisation;
using ITSM.Repositories.Discussion;
using ITSM.Repositories.KnowledgeBase;
using ITSM.Repositories.Qualification;
using ITSM.Repositories.RoleManager;
using ITSM.Repositories.Ticket;
using ITSM.Repositories.TicketAssignment;
using ITSM.Repositories.TicketCategory;
using ITSM.Repositories.TicketSort;
using ITSM.Repositories.UserManagment;
using ITSM.Repositories.UserProfile;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
    /*builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5000); 
});
*/

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<DBaseContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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
builder.Services.AddLogging();



var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await SeedRoles.Initialize(services);
}


app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
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