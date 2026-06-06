using Moq;
using FluentAssertions;
using ITSM.Models;
using ITSM.Services.UserProfile;
using ITSM.ViewModels.Manage;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace ITSM.Tests;

public class UserProfileServiceTests
{
    private readonly UserProfileService _sut;
    private readonly Mock<UserManager<User>> _userManager;
    private readonly Mock<SignInManager<User>> _signInManager;

    public UserProfileServiceTests()
    {
        var userStore = new Mock<IUserStore<User>>();
        _userManager = new Mock<UserManager<User>>(
            userStore.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        var contextAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
        var claimsFactory = new Mock<IUserClaimsPrincipalFactory<User>>();
        _signInManager = new Mock<SignInManager<User>>(
            _userManager.Object, contextAccessor.Object, claimsFactory.Object, null!, null!, null!, null!);

        _sut = new UserProfileService(_userManager.Object, _signInManager.Object);
    }

    private static User MakeUser() => new()
    {
        Id = "user-1",
        UserName = "testuser",
        Email = "test@test.com",
        PhoneNumber = "123"
    };

    // ── GetUserProfileAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task GetUserProfileAsync_ReturnsViewModelWithUserData()
    {
        var user = MakeUser();

        var result = await _sut.GetUserProfileAsync(user);

        result.UserName.Should().Be(user.UserName);
        result.Email.Should().Be(user.Email);
        result.PhoneNumber.Should().Be(user.PhoneNumber);
    }

    // ── UpdateUserProfileAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task UpdateUserProfileAsync_ReturnsSuccess_AndUpdatesFields()
    {
        var user = MakeUser();
        _userManager.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        var result = await _sut.UpdateUserProfileAsync(user, new EditUserViewModel
        {
            UserName = "newname",
            Email = "new@test.com",
            PhoneNumber = "999"
        });

        result.IsSuccess.Should().BeTrue();
        user.UserName.Should().Be("newname");
        user.Email.Should().Be("new@test.com");
        user.PhoneNumber.Should().Be("999");
    }

    [Fact]
    public async Task UpdateUserProfileAsync_ReturnsFailure_WhenUpdateFails()
    {
        var user = MakeUser();
        _userManager.Setup(m => m.UpdateAsync(user))
                   .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "DB error" }));

        var result = await _sut.UpdateUserProfileAsync(user, new EditUserViewModel
        {
            UserName = "u",
            Email = "u@t.com"
        });

        result.IsSuccess.Should().BeFalse();
    }

    // ── ChangeUserPasswordAsync ────────────────────────────────────────────────

    [Fact]
    public async Task ChangeUserPasswordAsync_ReturnsSuccess_WhenPasswordChanged()
    {
        var user = MakeUser();
        _userManager.Setup(m => m.ChangePasswordAsync(user, "old", "newpass"))
                   .ReturnsAsync(IdentityResult.Success);

        var result = await _sut.ChangeUserPasswordAsync(user, "old", "newpass");

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ChangeUserPasswordAsync_ReturnsFailure_WhenPasswordChangeFails()
    {
        var user = MakeUser();
        _userManager.Setup(m => m.ChangePasswordAsync(user, "wrong", "new"))
                   .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Wrong password" }));

        var result = await _sut.ChangeUserPasswordAsync(user, "wrong", "new");

        result.IsSuccess.Should().BeFalse();
    }

    // ── GetUserAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetUserAsync_DelegatesToUserManager()
    {
        var user = MakeUser();
        var principal = new ClaimsPrincipal();
        _userManager.Setup(m => m.GetUserAsync(principal)).ReturnsAsync(user);

        var result = await _sut.GetUserAsync(principal);

        result.Should().Be(user);
    }

    // ── RefreshUserSignInAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task RefreshUserSignInAsync_DelegatesToSignInManager()
    {
        var user = MakeUser();

        await _sut.RefreshUserSignInAsync(user);

        _signInManager.Verify(m => m.RefreshSignInAsync(user), Times.Once);
    }
}
