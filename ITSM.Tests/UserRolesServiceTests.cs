using Moq;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ITSM.Data;
using ITSM.Models;
using ITSM.Services.RoleManager;
using ITSM.ViewModels.Manage;
using Microsoft.AspNetCore.Identity;

namespace ITSM.Tests;

public class UserRolesServiceTests
{
    private readonly DBaseContext _db;
    private readonly UserRolesService _sut;
    private readonly Mock<UserManager<User>> _userManager;
    private readonly Mock<RoleManager<IdentityRole>> _roleManager;

    public UserRolesServiceTests()
    {
        var options = new DbContextOptionsBuilder<DBaseContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        _db = new DBaseContext(options);
        _db.Database.EnsureCreated();

        var userStore = new Mock<IUserStore<User>>();
        _userManager = new Mock<UserManager<User>>(
            userStore.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        var roleStore = new Mock<IRoleStore<IdentityRole>>();
        _roleManager = new Mock<RoleManager<IdentityRole>>(
            roleStore.Object, null!, null!, null!, null!);

        _sut = new UserRolesService(_userManager.Object, _roleManager.Object, _db);
    }

    private User MakeUser(string id = "user-1") => new()
    {
        Id = id,
        UserName = "testuser",
        Email = "test@test.com"
    };

    // ── GetUserRolesViewModel ──────────────────────────────────────────────────

    [Fact]
    public async Task GetUserRolesViewModel_ReturnsNull_WhenUserNotFound()
    {
        _userManager.Setup(m => m.FindByIdAsync("missing")).ReturnsAsync((User?)null);

        var result = await _sut.GetUserRolesViewModel("missing");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetUserRolesViewModel_ReturnsViewModel_WithCorrectRoleSelection()
    {
        var user = MakeUser();
        var role = new IdentityRole { Id = "r1", Name = "Admin" };
        _db.Roles.Add(role);
        await _db.SaveChangesAsync();

        _userManager.Setup(m => m.FindByIdAsync(user.Id)).ReturnsAsync(user);
        _userManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Admin" });
        _roleManager.Setup(m => m.Roles).Returns(_db.Roles);

        var result = await _sut.GetUserRolesViewModel(user.Id);

        result.Should().NotBeNull();
        result!.UserId.Should().Be(user.Id);
        result.Roles.Should().Contain(r => r.RoleName == "Admin" && r.IsSelected);
    }

    // ── UpdateUserRolesAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task UpdateUserRolesAsync_ReturnsFailure_WhenUserNotFound()
    {
        _userManager.Setup(m => m.FindByIdAsync("missing")).ReturnsAsync((User?)null);

        var result = await _sut.UpdateUserRolesAsync("missing", new List<RoleSelectionViewModel>());

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateUserRolesAsync_ReturnsSuccess_WhenRolesUpdated()
    {
        var user = MakeUser();
        _userManager.Setup(m => m.FindByIdAsync(user.Id)).ReturnsAsync(user);
        _userManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "User" });
        _userManager.Setup(m => m.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>()))
                   .ReturnsAsync(IdentityResult.Success);
        _userManager.Setup(m => m.AddToRolesAsync(user, It.IsAny<IEnumerable<string>>()))
                   .ReturnsAsync(IdentityResult.Success);

        var result = await _sut.UpdateUserRolesAsync(user.Id, new List<RoleSelectionViewModel>
        {
            new() { RoleName = "Admin", IsSelected = true }
        });

        result.IsSuccess.Should().BeTrue();
        _userManager.Verify(m => m.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>()), Times.Once);
        _userManager.Verify(m => m.AddToRolesAsync(user, It.Is<IEnumerable<string>>(r => r.Contains("Admin"))), Times.Once);
    }

    [Fact]
    public async Task UpdateUserRolesAsync_ReturnsFailure_WhenRemoveFails()
    {
        var user = MakeUser();
        _userManager.Setup(m => m.FindByIdAsync(user.Id)).ReturnsAsync(user);
        _userManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string>());
        _userManager.Setup(m => m.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>()))
                   .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Remove failed" }));

        var result = await _sut.UpdateUserRolesAsync(user.Id, new List<RoleSelectionViewModel>());

        result.IsSuccess.Should().BeFalse();
    }
}
