using Moq;
using FluentAssertions;
using ITSM.Models;
using ITSM.Services.Authorisation;
using Microsoft.AspNetCore.Identity;

namespace ITSM.Tests;

public class AuthServiceTests
{
    private readonly AuthService _sut;
    private readonly Mock<SignInManager<User>> _signInManager;

    public AuthServiceTests()
    {
        var userStore = new Mock<IUserStore<User>>();
        var userManager = new Mock<UserManager<User>>(
            userStore.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        var contextAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
        var claimsFactory = new Mock<IUserClaimsPrincipalFactory<User>>();
        _signInManager = new Mock<SignInManager<User>>(
            userManager.Object, contextAccessor.Object, claimsFactory.Object, null!, null!, null!, null!);

        _sut = new AuthService(_signInManager.Object);
    }

    private static User MakeUser() => new() { Id = "user-1", UserName = "testuser", Email = "test@test.com" };

    // ── RefreshSignInAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task RefreshSignInAsync_CallsSignOut_ThenSignIn()
    {
        var user = MakeUser();

        await _sut.RefreshSignInAsync(user);

        _signInManager.Verify(m => m.SignOutAsync(), Times.Once);
        _signInManager.Verify(m => m.SignInAsync(user, false, null), Times.Once);
    }
}
