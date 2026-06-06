using Moq;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ITSM.Data;
using ITSM.Models;
using ITSM.Services.UserManagement;
using ITSM.ViewModels.Manage;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace ITSM.Tests;

public class UserManagementServiceTests
{
    private readonly DBaseContext _db;
    private readonly UserManagementService _sut;
    private readonly Mock<UserManager<User>> _userManager;

    public UserManagementServiceTests()
    {
        var options = new DbContextOptionsBuilder<DBaseContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new DBaseContext(options);
        _db.Database.EnsureCreated();

        var store = new Mock<IUserStore<User>>();
        _userManager = new Mock<UserManager<User>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _userManager
            .Setup(m => m.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Success);

        _userManager
            .Setup(m => m.UpdateSecurityStampAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Success);

        _userManager
            .Setup(m => m.GetRolesAsync(It.IsAny<User>()))
            .ReturnsAsync(new List<string>());

        _sut = new UserManagementService(_db, _userManager.Object);
    }

    private async Task<User> SeedUser(bool isDeleted = false, string? email = null)
    {
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "testuser",
            Email = email ?? "test@test.com",
            IsDeleted = isDeleted
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }

    // ── GetUserById ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetUserById_ReturnsUser_WhenFound()
    {
        var user = await SeedUser();

        var result = await _sut.GetUserById(user.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task GetUserById_ReturnsNull_WhenNotFound()
    {
        var result = await _sut.GetUserById("nonexistent");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetUserById_ReturnsDeletedUser_BecauseIgnoresQueryFilter()
    {
        var user = await SeedUser(isDeleted: true);

        var result = await _sut.GetUserById(user.Id);

        result.Should().NotBeNull();
    }

    // ── SoftDeleteUserById ─────────────────────────────────────────────────────

    [Fact]
    public async Task SoftDeleteUserById_ReturnsFailure_WhenUserNotFound()
    {
        var result = await _sut.SoftDeleteUserById("nonexistent");

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task SoftDeleteUserById_ReturnsSuccess_AndMarksUserDeleted()
    {
        var user = await SeedUser();

        var result = await _sut.SoftDeleteUserById(user.Id);

        result.IsSuccess.Should().BeTrue();
        var updated = await _db.Users.IgnoreQueryFilters().FirstAsync(u => u.Id == user.Id);
        updated.IsDeleted.Should().BeTrue();
        updated.LockoutEnabled.Should().BeTrue();
        updated.LockoutEnd.Should().Be(DateTimeOffset.MaxValue);
    }

    // ── EditUser ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task EditUser_ReturnsFailure_WhenUserNotFound()
    {
        var result = await _sut.EditUser("nonexistent", new EditUserViewModel
        {
            UserName = "x",
            Email = "x@test.com"
        });

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task EditUser_ReturnsFailure_WhenEmailTakenByAnotherUser()
    {
        var other = await SeedUser(email: "taken@test.com");
        var user = await SeedUser(email: "mine@test.com");
        _userManager.Setup(m => m.FindByEmailAsync("taken@test.com")).ReturnsAsync(other);

        var result = await _sut.EditUser(user.Id, new EditUserViewModel
        {
            UserName = "u",
            Email = "taken@test.com"
        });

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task EditUser_UpdatesUserFields()
    {
        var user = await SeedUser(email: "old@test.com");
        _userManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        _userManager.Setup(m => m.NormalizeName(It.IsAny<string>())).Returns<string>(s => s.ToUpper());
        _userManager.Setup(m => m.NormalizeEmail(It.IsAny<string>())).Returns<string>(s => s.ToUpper());

        var result = await _sut.EditUser(user.Id, new EditUserViewModel
        {
            UserName = "updatedname",
            Email = "new@test.com",
            PhoneNumber = "555"
        });

        result.IsSuccess.Should().BeTrue();
        var updated = await _db.Users.IgnoreQueryFilters().FirstAsync(u => u.Id == user.Id);
        updated.UserName.Should().Be("updatedname");
        updated.Email.Should().Be("new@test.com");
        updated.PhoneNumber.Should().Be("555");
    }

    [Fact]
    public async Task EditUser_ReturnsFailure_WhenPasswordsDoNotMatch()
    {
        var user = await SeedUser();
        _userManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

        var result = await _sut.EditUser(user.Id, new EditUserViewModel
        {
            UserName = "u",
            Email = user.Email,
            NewPassword = "abc123",
            ConfirmPassword = "xyz789"
        });

        result.IsSuccess.Should().BeFalse();
    }

    // ── SearchUser ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task SearchUser_ReturnsAllUsers_WhenSearchIsNull()
    {
        await SeedUser(email: "a@test.com");
        await SeedUser(email: "b@test.com");

        var result = await _sut.SearchUser(null);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task SearchUser_FiltersByEmail_CaseInsensitive()
    {
        await SeedUser(email: "alice@example.com");
        await SeedUser(email: "bob@example.com");

        var result = await _sut.SearchUser("ALICE");

        result.Should().HaveCount(1)
              .And.Contain(u => u.Email == "alice@example.com");
    }

    [Fact]
    public async Task SearchUser_ReturnsEmpty_WhenNoMatch()
    {
        await SeedUser(email: "alice@example.com");

        var result = await _sut.SearchUser("xyz_no_match");

        result.Should().BeEmpty();
    }

    // ── GetAllUsersToList ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllUsersToList_ReturnsAllUsers_IncludingDeleted()
    {
        await SeedUser(email: "active@test.com");
        await SeedUser(isDeleted: true, email: "deleted@test.com");

        var result = await _sut.GetAllUsersToList();

        result.Should().HaveCount(2); // IgnoreQueryFilters includes deleted
    }

    // ── GetCurrentUserAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task GetCurrentUserAsync_DelegatesToUserManager()
    {
        var user = await SeedUser();
        var principal = new ClaimsPrincipal();
        _userManager.Setup(m => m.GetUserAsync(principal)).ReturnsAsync(user);

        var result = await _sut.GetCurrentUserAsync(principal);

        result.Should().Be(user);
    }

    // ── CreateEditUserViewModel ────────────────────────────────────────────────

    [Fact]
    public async Task CreateEditUserViewModel_ReturnsViewModelWithUserData()
    {
        var user = await SeedUser(email: "edit@test.com");
        user.UserName = "edituser";
        await _db.SaveChangesAsync();

        var result = await _sut.CreateEditUserViewModel(user.Id);

        result.UserName.Should().Be("edituser");
        result.Email.Should().Be("edit@test.com");
    }

    [Fact]
    public async Task CreateEditUserViewModel_ThrowsArgumentException_WhenUserNotFound()
    {
        Func<Task> act = () => _sut.CreateEditUserViewModel("nonexistent");
        await act.Should().ThrowAsync<ArgumentException>();
    }
}
